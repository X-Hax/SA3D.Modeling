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

namespace SA3D.Modeling.Mesh.Converters
{
	internal class WeldedBasicConverter
	{
		private readonly Node _rootNode;
		private readonly Node[] _weldingGroups;
		private readonly Dictionary<Node, int> _nodeIndices;
		private readonly Dictionary<Node, (int weightIndex, int groupIndex)> _relativeNodeIndices;

		private (Matrix4x4 vertex, Matrix4x4 normal)[] _matrices;
		private int _weightNum;

		private ushort[][] _vertexIndexMap;
		private WeightedVertex[] _vertices;
		private bool[] _welded;

		private BufferCorner[][] _polygonCorners;
		private BufferMaterial[] _materials;
		private bool _hasColors;

		public WeldedBasicConverter(Node rootNode, Node[] weldingGroups, Dictionary<Node, int> nodeIndices)
		{
			_rootNode = rootNode;
			_weldingGroups = weldingGroups;
			_nodeIndices = nodeIndices;
			_relativeNodeIndices = new();

			_matrices = Array.Empty<(Matrix4x4 vertex, Matrix4x4 normal)>();

			_vertexIndexMap = Array.Empty<ushort[]>();
			_vertices = Array.Empty<WeightedVertex>();
			_welded = Array.Empty<bool>();

			_polygonCorners = Array.Empty<BufferCorner[]>();
			_materials = Array.Empty<BufferMaterial>();
		}


		public WeightedMesh Process()
		{
			SetupNodeIndices();
			SetupMatrices();

			CollectVertices();
			MergeWelds();

			CollectPolygons();

			RemoveUnusedVertices();

			WeightedMesh result = WeightedMesh.Create(_vertices, _polygonCorners, _materials, _hasColors);

			result.Label = _weldingGroups[^1].Attach!.Label;
			result.RootIndices.Add(_nodeIndices[_rootNode]);

			return result;
		}

		private void SetupNodeIndices()
		{
			int rootNodeIndex = _nodeIndices[_rootNode];
			_vertexIndexMap = new ushort[_weldingGroups.Length][];

			for(int i = 0; i < _weldingGroups.Length; i++)
			{
				Node node = _weldingGroups[i];

				if(node.Attach is not BasicAttach attach)
				{
					throw new InvalidOperationException($"Node \"{node.Label}\" has no basic attach!");
				}

				_vertexIndexMap[i] = new ushort[attach.Positions.Length];

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
			_vertices = new WeightedVertex[_vertexIndexMap.Sum(x => x.Length)];
			_welded = new bool[_vertices.Length];

			for(int i = 0, vertexOffset = 0; i < _weldingGroups.Length; i++)
			{
				Node node = _weldingGroups[i];
				BasicAttach attach = (BasicAttach)node.Attach!;
				(Matrix4x4 vertexMatrix, Matrix4x4 normalMatrix) = _matrices[i];

				int weightIndex = _relativeNodeIndices[node].weightIndex;

				ushort[] vertexIndexMap = new ushort[attach.Positions.Length];
				_vertexIndexMap[i] = vertexIndexMap;

				for(int j = 0; j < vertexIndexMap.Length; j++)
				{
					Vector3 position = Vector3.Transform(attach.Positions[j], vertexMatrix);
					Vector3 normal = Vector3.TransformNormal(attach.Normals[j], normalMatrix);

					WeightedVertex weightedVert = new(position, normal, _weightNum);
					weightedVert.Weights![weightIndex] = 1;
					_vertices[j + vertexOffset] = weightedVert;

					vertexIndexMap[j] = (ushort)(j + vertexOffset);
				}

				InsertWeldings(node, weightIndex, vertexIndexMap);

				vertexOffset += vertexIndexMap.Length;
			}
		}

		private void InsertWeldings(Node node, int weightIndex, ushort[] vertexIndexMap)
		{
			if(node.Welding == null)
			{
				return;
			}

			foreach(VertexWelding vertexWelding in node.Welding)
			{
				uint destIndex = vertexIndexMap[vertexWelding.DestinationVertexIndex];
				WeightedVertex destVertex = _vertices[destIndex];
				_welded[destIndex] = true;

				destVertex.Position = default;
				destVertex.Normal = default;
				destVertex.Weights![weightIndex] = 0;

				foreach(Weld weld in vertexWelding.Welds)
				{
					(int sourceWeightIndex, int sourceGroupIndex) = _relativeNodeIndices[weld.SourceNode];
					destVertex.Weights![sourceWeightIndex] = weld.Weight;

					(Matrix4x4 sourceVertexMatrix, Matrix4x4 sourceNormalMatrix) = _matrices[sourceGroupIndex];

					BasicAttach sourceAttach = (BasicAttach)weld.SourceNode.Attach!;
					Vector3 sourcePosition = Vector3.Transform(sourceAttach.Positions[(ushort)weld.VertexIndex], sourceVertexMatrix);
					Vector3 sourceNormal = Vector3.TransformNormal(sourceAttach.Normals[(ushort)weld.VertexIndex], sourceNormalMatrix);

					destVertex.Position += sourcePosition * weld.Weight;
					destVertex.Normal += sourceNormal * weld.Weight;
				}

				_vertices[destIndex] = destVertex;
			}
		}

		private void MergeWelds()
		{
			Dictionary<int, int> mergeMapping = new();

			for(int i = 0; i < _vertices.Length; i++)
			{
				if(!_welded[i])
				{
					continue;
				}

				WeightedVertex vertex = _vertices[i];

				for(int j = 0; j < _vertices.Length; j++)
				{
					WeightedVertex other = _vertices[j];

					bool useable =
						Vector3.Distance(other.Position, vertex.Position) < 0.0001f
						&& Vector3.Distance(other.Normal, vertex.Normal) < 0.0001f
						&& Enumerable.SequenceEqual(vertex.Weights!, other.Weights!);

					if(useable && i != j && ((_welded[j] && j < i) || !_welded[j]))
					{
						mergeMapping.Add(i, j);
						break;
					}
				}
			}

			foreach(ushort[] item in _vertexIndexMap)
			{
				for(int i = 0; i < item.Length; i++)
				{
					if(mergeMapping.TryGetValue(item[i], out int newIndex))
					{
						item[i] = (ushort)newIndex;
					}
				}
			}
		}

		private void CollectPolygons()
		{
			List<List<BufferCorner>> polygonCorners = new();
			List<BufferMaterial> materials = new();
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

					BufferMaterial material = BasicConverter.ConvertToBufferMaterial(attach.Materials[j]);
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

			List<WeightedVertex> newVertices = new(_vertices);
			ushort[] map = new ushort[_vertices.Length];

			ushort realIndex = (ushort)(usedVertices.Count - 1);
			for(int i = _vertices.Length - 1; i >= 0; i--)
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

			_vertices = newVertices.ToArray();

			foreach(BufferCorner[] corners in _polygonCorners)
			{
				for(int i = 0; i < corners.Length; i++)
				{
					corners[i].VertexIndex = map[corners[i].VertexIndex];
				}
			}
		}

		public static WeightedMesh[] CreateWeightedFromWeldedBasicModel(Node model, Node[][] weldingGroups)
		{
			List<WeightedMesh> result = new();

			Node[] nodes = model.GetTreeNodes();
			Dictionary<Node, int> nodeIndices = new();

			for(int i = 0; i < nodes.Length; i++)
			{
				nodeIndices.Add(nodes[i], i);
			}

			HashSet<Node> groupedNodes = weldingGroups.SelectMany(x => x).ToHashSet();
			Dictionary<BasicAttach, WeightedMesh> reusedMeshes = new();

			foreach(Node node in model.GetTreeNodeEnumerable())
			{
				if(node.Attach == null || groupedNodes.Contains(node))
				{
					continue;
				}

				BasicAttach atc = (BasicAttach)node.Attach;

				if(!reusedMeshes.TryGetValue(atc, out WeightedMesh? mesh))
				{
					mesh = WeightedMesh.FromAttach(atc, BufferMode.None);
					reusedMeshes.Add(atc, mesh);
					result.Add(mesh);
				}

				mesh.RootIndices.Add(nodeIndices[node]);
			}

			foreach(Node[] group in weldingGroups)
			{
				SortedSet<int> dependencyNodes = new(group.Select(x => nodeIndices[x]));
				int rootNodeIndex = ToWeightedConverter.ComputeCommonNodeIndex(nodes, dependencyNodes);

				WeightedMesh mesh = new WeldedBasicConverter(nodes[rootNodeIndex], group, nodeIndices).Process();
				result.Add(mesh);
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
			HashSet<BasicAttach> buffered = new();
			foreach(Node node in model.GetTreeNodeEnumerable())
			{
				if(node.Attach == null || groupedNodes.Contains(node))
				{
					continue;
				}

				BasicAttach atc = (BasicAttach)node.Attach;

				if(!buffered.Contains(atc))
				{
					buffered.Add(atc);
					atc.MeshData = BasicConverter.ConvertBasicToBuffer(atc, optimize);
				}
			}

			WeightedMesh[] weightedMeshes = CreateWeightedFromWeldedBasicModel(model, weldingGroups);

			Dictionary<Node, BasicAttach> attachLUT = new();
			foreach(Node node in model.GetTreeNodeEnumerable())
			{
				if(node.Attach is BasicAttach atc)
				{
					attachLUT.Add(node, atc);
				}
			}

			WeightedMesh.ToModel(model, weightedMeshes, AttachFormat.Buffer, optimize, false);

			foreach(KeyValuePair<Node, BasicAttach> nodeAttach in attachLUT)
			{
				nodeAttach.Value.MeshData = nodeAttach.Key.Attach!.MeshData;
			}

			model.ClearAttachesFromTree();

			foreach(KeyValuePair<Node, BasicAttach> nodeAttach in attachLUT)
			{
				nodeAttach.Key.Attach = nodeAttach.Value;
			}

			return true;
		}
	}
}
