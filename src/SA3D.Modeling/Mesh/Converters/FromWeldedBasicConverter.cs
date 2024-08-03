using SA3D.Modeling.Mesh.Basic;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Mesh.Weighted;
using SA3D.Modeling.ObjectData.Structs;
using SA3D.Modeling.ObjectData;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System;
using SA3D.Modeling.Structs;
using SA3D.Common;

namespace SA3D.Modeling.Mesh.Converters
{
	internal class FromWeldedBasicConverter
	{
		private readonly Node _rootNode;
		private readonly Node[] _weldingGroups;
		private readonly Dictionary<Node, int> _nodeIndices;
		private readonly Dictionary<Node, (int weightIndex, int groupIndex)> _relativeNodeIndices;

		private (Matrix4x4 vertex, Matrix4x4 normal)[] _matrices;
		private int _weightNum;

		private WeightedVertex[][] _vertices;
		private bool[][] _welded;

		private WeightedVertex[] _outVertices;
		private ushort[][] _vertexIndexMap;

		private BufferCorner[][] _polygonCorners;
		private BufferMaterial[] _materials;
		private bool _hasColors;


		private FromWeldedBasicConverter(Node rootNode, Node[] weldingGroups, Dictionary<Node, int> nodeIndices)
		{
			_rootNode = rootNode;
			_weldingGroups = weldingGroups;
			_nodeIndices = nodeIndices;
			_relativeNodeIndices = [];

			_matrices = Array.Empty<(Matrix4x4 vertex, Matrix4x4 normal)>();

			_vertices = Array.Empty<WeightedVertex[]>();
			_welded = Array.Empty<bool[]>();

			_outVertices = Array.Empty<WeightedVertex>();
			_vertexIndexMap = Array.Empty<ushort[]>();
			_polygonCorners = Array.Empty<BufferCorner[]>();
			_materials = Array.Empty<BufferMaterial>();
		}


		public WeightedMesh? Process()
		{
			SetupNodeIndices();
			SetupMatrices();

			CollectVertices();
			InsertWeldings();
			MergeWelds();

			CollectPolygons();

			RemoveUnusedVertices();

			if(_vertices.Length == 0)
			{
				return null;
			}

			WeightedMesh result = WeightedMesh.Create(_outVertices, _polygonCorners, _materials, _hasColors, true);

			result.Label = _weldingGroups[^1].Attach!.Label;
			result.RootIndices.Add(_nodeIndices[_rootNode]);

			return result;
		}

		private void SetupNodeIndices()
		{
			int rootNodeIndex = _nodeIndices[_rootNode];

			for(int i = 0; i < _weldingGroups.Length; i++)
			{
				Node node = _weldingGroups[i];

				if(node.Attach is not BasicAttach attach)
				{
					throw new InvalidOperationException($"Node \"{node.Label}\" has no basic attach!");
				}

				int nodeIndex = _nodeIndices[node];
				_relativeNodeIndices.Add(node, (nodeIndex - rootNodeIndex, i));
			}

			_weightNum = _relativeNodeIndices.Values.Max(x => x.weightIndex) + 1;
		}

		private void SetupMatrices()
		{
			Matrix4x4 rootMatrix = _rootNode.GetWorldMatrix();
			Matrix4x4.Invert(rootMatrix, out Matrix4x4 invRootMatrix);

			_matrices = new (Matrix4x4 vertex, Matrix4x4 normal)[_weldingGroups.Length];

			for(int i = 0; i < _weldingGroups.Length; i++)
			{
				Node node = _weldingGroups[i];
				Matrix4x4 vertexMatrix = Matrix4x4.Identity;

				if(node != _rootNode)
				{
					Matrix4x4 worldMatrix = node.GetWorldMatrix();
					vertexMatrix = worldMatrix * invRootMatrix;
				}

				Matrix4x4 normalMatrix = vertexMatrix.GetNormalMatrix();

				_matrices[i] = (vertexMatrix, normalMatrix);
			}

		}

		private void CollectVertices()
		{
			_vertices = new WeightedVertex[_weldingGroups.Length][];

			for(int i = 0; i < _weldingGroups.Length; i++)
			{
				Node node = _weldingGroups[i];
				BasicAttach attach = (BasicAttach)node.Attach!;
				(Matrix4x4 vertexMatrix, Matrix4x4 normalMatrix) = _matrices[i];

				int weightIndex = _relativeNodeIndices[node].weightIndex;

				WeightedVertex[] newVertices = new WeightedVertex[attach.Positions.Length];

				for(int j = 0; j < newVertices.Length; j++)
				{
					Vector3 position = Vector3.Transform(attach.Positions[j], vertexMatrix);
					Vector3 normal = Vector3.Normalize(Vector3.TransformNormal(attach.Normals[j], normalMatrix));

					WeightedVertex weightedVert = new(position, normal, _weightNum);
					weightedVert.Weights![weightIndex] = 1;
					newVertices[j] = weightedVert;
				}

				_vertices[i] = newVertices;
			}
		}

		private void InsertWeldings()
		{
			_welded = new bool[_vertices.Length][];

			for(int i = 0; i < _weldingGroups.Length; i++)
			{
				Node node = _weldingGroups[i];

				if(node.Welding == null)
				{
					_welded[i] = new bool[_vertices[i].Length];
					continue;
				}

				WeightedVertex[] destVertices = _vertices[i].ContentClone();
				bool[] destWelded = new bool[destVertices.Length];

				foreach(VertexWelding vertexWelding in node.Welding)
				{
					WeightedVertex destVertex = destVertices[vertexWelding.DestinationVertexIndex];

					destVertex.Position = default;
					destVertex.Normal = default;
					Array.Clear(destVertex.Weights!);

					foreach(Weld weld in vertexWelding.Welds)
					{
						(int sourceWeightIndex, int sourceGroupIndex) = _relativeNodeIndices[weld.SourceNode];

						WeightedVertex sourceVertex = _vertices[sourceGroupIndex][weld.VertexIndex];

						destVertex.Position += sourceVertex.Position * weld.Weight;
						destVertex.Normal += sourceVertex.Normal * weld.Weight;

						for(int j = 0; j < sourceVertex.Weights!.Length; j++)
						{
							destVertex.Weights![j] += sourceVertex.Weights[j] * weld.Weight;
						}
					}

					if(vertexWelding.Welds.Length > 0)
					{
						destVertex.Normal = Vector3.Normalize(destVertex.Normal);
					}

					destWelded[vertexWelding.DestinationVertexIndex] = true;
					destVertices[vertexWelding.DestinationVertexIndex] = destVertex;
				}

				_vertices[i] = destVertices;
				_welded[i] = destWelded;
			}
		}

		private void MergeWelds()
		{
			Dictionary<ushort, ushort> mergeMapping = [];
			_outVertices = _vertices.SelectMany(x => x).ToArray();
			bool[] resultWelded = _welded.SelectMany(x => x).ToArray();

			for(ushort i = 0; i < _outVertices.Length; i++)
			{
				if(!resultWelded[i])
				{
					continue;
				}

				WeightedVertex vertex = _outVertices[i];

				for(ushort j = 0; j < _outVertices.Length; j++)
				{
					if(i == j || (resultWelded[j] && j > i))
					{
						continue;
					}

					WeightedVertex other = _outVertices[j];

					bool useable =
						Vector3.Distance(other.Position, vertex.Position) < 0.0001f
						&& Vector3.Distance(other.Normal, vertex.Normal) < 0.0001f
						&& Enumerable.SequenceEqual(vertex.Weights!, other.Weights!);

					if(useable)
					{
						mergeMapping.Add(i, j);
						break;
					}
				}
			}

			CreateVerteIndexMap(mergeMapping);
		}

		private void CreateVerteIndexMap(Dictionary<ushort, ushort> mergeMapping)
		{
			_vertexIndexMap = new ushort[_vertices.Length][];
			ushort vertexIndex = 0;
			for(int i = 0; i < _vertices.Length; i++)
			{
				ushort[] indexMap = new ushort[_vertices[i].Length];

				for(int j = 0; j < indexMap.Length; j++, vertexIndex++)
				{
					indexMap[j] = mergeMapping.TryGetValue(vertexIndex, out ushort newIndex)
						? newIndex
						: vertexIndex;
				}

				_vertexIndexMap[i] = indexMap;
			}
		}

		private void CollectPolygons()
		{
			List<List<BufferCorner>> polygonCorners = [];
			List<BufferMaterial> materials = [];
			_hasColors = false;

			int meshIndex = 0;
			for(int i = 0; i < _weldingGroups.Length; i++)
			{
				BasicAttach attach = (BasicAttach)_weldingGroups[i].Attach!;
				ushort[] vertexIndexMap = _vertexIndexMap[i];

				for(int j = 0; j < attach.Meshes.Length; j++, meshIndex++)
				{
					BasicMesh mesh = attach.Meshes[j];
					_hasColors |= mesh.Colors != null;

					BasicConverter.ConvertPolygons(mesh, out BufferCorner[] corners, out uint[]? indexList, out bool strippified);

					for(int k = 0; k < corners.Length; k++)
					{
						corners[k].VertexIndex = vertexIndexMap[corners[k].VertexIndex];
					}

					BufferMaterial material = BasicConverter.ConvertToBufferMaterial(attach.Materials[mesh.MaterialIndex]);
					BufferCorner[] resultCorners = BufferMesh.GetCornerTriangleList(corners, indexList, strippified);

					List<BufferCorner>? targetCorners = null;

					for(int k = 0; k < materials.Count; k++)
					{
						if(materials[k] == material)
						{
							targetCorners = polygonCorners[k];
							break;
						}
					}

					if(targetCorners != null)
					{
						targetCorners.AddRange(resultCorners);
					}
					else
					{
						polygonCorners.Add(new(resultCorners));
						materials.Add(material);
					}
				}
			}

			_polygonCorners = polygonCorners.Select(x => x.ToArray()).ToArray();
			_materials = materials.ToArray();
		}

		private void RemoveUnusedVertices()
		{
			HashSet<ushort> usedVertices = _polygonCorners
				.SelectMany(x => x)
				.Select(x => x.VertexIndex)
				.ToHashSet();

			List<WeightedVertex> newVertices = new(_outVertices);
			ushort[] map = new ushort[_outVertices.Length];

			ushort realIndex = (ushort)(usedVertices.Count - 1);
			for(int i = _outVertices.Length - 1; i >= 0; i--)
			{
				if(!usedVertices.Contains((ushort)i))
				{
					newVertices.RemoveAt(i);
				}
				else
				{
					map[i] = realIndex;
					realIndex--;
				}
			}

			_outVertices = newVertices.ToArray();

			foreach(BufferCorner[] corners in _polygonCorners)
			{
				for(int i = 0; i < corners.Length; i++)
				{
					corners[i].VertexIndex = map[corners[i].VertexIndex];
				}
			}
		}


		public static WeightedMesh[] CreateWeightedFromWeldedBasicModel(Node model, Node[][] weldingGroups, BufferMode bufferMode)
		{
			List<WeightedMesh> result = [];

			Node[] nodes = model.GetTreeNodes();
			Dictionary<Node, int> nodeIndices = [];

			for(int i = 0; i < nodes.Length; i++)
			{
				nodeIndices.Add(nodes[i], i);
			}

			HashSet<Node> groupedNodes = weldingGroups.SelectMany(x => x).ToHashSet();
			Dictionary<BasicAttach, WeightedMesh> reusedMeshes = [];

			foreach(Node node in model.GetTreeNodeEnumerable())
			{
				if(node.Attach == null || groupedNodes.Contains(node))
				{
					continue;
				}

				BasicAttach atc = (BasicAttach)node.Attach;

				if(!reusedMeshes.TryGetValue(atc, out WeightedMesh? mesh))
				{
					mesh = WeightedMesh.FromAttach(atc, bufferMode);

					reusedMeshes.Add(atc, mesh);
					result.Add(mesh);
				}

				mesh.RootIndices.Add(nodeIndices[node]);
			}

			foreach(Node[] group in weldingGroups)
			{
				SortedSet<int> dependencyNodes = new(group.Select(x => nodeIndices[x]));
				int rootNodeIndex = ToWeightedConverter.ComputeCommonNodeIndex(nodes, dependencyNodes);

				WeightedMesh? mesh = new FromWeldedBasicConverter(nodes[rootNodeIndex], group, nodeIndices).Process();

				if(mesh != null)
				{
					result.Add(mesh);
				}
			}

			return result.ToArray();
		}

		public static bool BufferWeldedBasicModel(Node model, bool optimize)
		{
			Node[][] weldingGroups = model.GetTreeWeldingGroups(false);
			if(weldingGroups.Length == 0)
			{
				return false;
			}

			HashSet<Node> groupedNodes = weldingGroups.SelectMany(x => x).ToHashSet();

			WeightedMesh[] weightedMeshes = CreateWeightedFromWeldedBasicModel(model, weldingGroups, optimize ? BufferMode.GenerateOptimized : BufferMode.Generate);

			Dictionary<Node, BasicAttach> attachLUT = [];
			Dictionary<Node, VertexWelding[]> weldingLUT = [];
			foreach(Node node in model.GetTreeNodeEnumerable())
			{
				if(node.Attach is BasicAttach atc)
				{
					attachLUT.Add(node, atc);
				}

				if(node.Welding != null)
				{
					weldingLUT.Add(node, node.Welding);
				}
			}

			WeightedMesh.ToModel(model, weightedMeshes, AttachFormat.Buffer, optimize);

			foreach(KeyValuePair<Node, BasicAttach> nodeAttach in attachLUT)
			{
				nodeAttach.Value.MeshData = nodeAttach.Key.Attach?.MeshData ?? Array.Empty<BufferMesh>();
			}

			model.ClearAttachesFromTree();

			foreach(KeyValuePair<Node, BasicAttach> nodeAttach in attachLUT)
			{
				nodeAttach.Key.Attach = nodeAttach.Value;
			}

			// WeightedMesh.ToModel removes the weldings when converting to Buffer attaches. We need to re-add them
			foreach(KeyValuePair<Node, VertexWelding[]> nodeWelding in weldingLUT)
			{
				nodeWelding.Key.Welding = nodeWelding.Value;
			}

			return true;
		}
	}
}
