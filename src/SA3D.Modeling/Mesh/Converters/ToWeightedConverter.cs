using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Mesh.Weighted;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Converters
{
	internal class ToWeightedConverter
	{
		private readonly Node[] _nodes;

		private readonly Dictionary<Node, Matrix4x4> _worldMatrices;
		private readonly List<WeightedMesh> _output;

		/// <summary>
		/// Mesh index to node index.
		/// <br/> Basically used to ignore nodes with no meshdata.
		/// </summary>
		private int[] _meshNodeIndexMapping;

		/// <summary>
		/// First index is vertex index, second is mesh index.
		/// <br/> Stores which meshes influence which vertices.
		/// </summary>
		private bool[,] _vertexIndexMapping;

		/// <summary>
		/// Index of the mesh currently being processed.
		/// </summary>
		private int _currentMeshIndex;

		/// <summary>
		/// Which vertices are used by the current meshes polygon.
		/// </summary>
		private readonly SortedSet<ushort> _usedVertices;

		private readonly Dictionary<Attach, WeightedMesh> _unweighted;

		private bool _finished;

		private ToWeightedConverter(Node model)
		{
			_nodes = model.GetTreeNodes();

			_worldMatrices = model.GetWorldMatrixTreeLUT();
			_output = new();

			_meshNodeIndexMapping = Array.Empty<int>();
			_vertexIndexMapping = new bool[0, 0];

			_currentMeshIndex = -1;
			_usedVertices = new();

			_unweighted = new();
		}

		private BufferMesh[] GetMeshData(int meshIndex)
		{
			int nodeIndex = _meshNodeIndexMapping[meshIndex];
			return _nodes[nodeIndex].Attach!.MeshData;
		}

		private void Verify()
		{
			foreach(Node node in _nodes)
			{
				if(node.Attach == null)
				{
					continue;
				}

				if(node.Attach.MeshData == null)
				{
					throw new FormatException("Not all attaches have meshdata! Please generate Meshdata before converting");
				}
			}
		}

		private void Setup()
		{
			List<int> nodeIndexMeshMap = new();

			for(int i = 0; i < _nodes.Length; i++)
			{
				Node node = _nodes[i];
				if(node.Attach == null || node.Attach.MeshData.Length == 0)
				{
					continue;
				}

				nodeIndexMeshMap.Add(i);
			}

			_meshNodeIndexMapping = nodeIndexMeshMap.ToArray();
			_vertexIndexMapping = new bool[0x10000, _meshNodeIndexMapping.Length];
		}

		private void MapVertices()
		{
			BufferMesh[] meshes = GetMeshData(_currentMeshIndex);

			_usedVertices.Clear();

			foreach(BufferMesh bufferMesh in meshes)
			{
				if(bufferMesh.Vertices != null)
				{
					if(bufferMesh.ContinueWeight)
					{
						foreach(BufferVertex vtx in bufferMesh.Vertices)
						{
							if(vtx.Weight == 0)
							{
								continue;
							}

							int vertexIndex = vtx.Index + bufferMesh.VertexWriteOffset;
							_vertexIndexMapping[vertexIndex, _currentMeshIndex] = true;
						}
					}
					else
					{
						foreach(BufferVertex vtx in bufferMesh.Vertices)
						{
							int index = vtx.Index + bufferMesh.VertexWriteOffset;

							Array.Clear(_vertexIndexMapping, index * _meshNodeIndexMapping.Length, _meshNodeIndexMapping.Length);

							if(vtx.Weight > 0)
							{
								_vertexIndexMapping[index, _currentMeshIndex] = true;
							}
						}
					}
				}

				if(bufferMesh.Corners != null)
				{
					foreach(BufferCorner corner in bufferMesh.Corners)
					{
						_usedVertices.Add((ushort)(corner.VertexIndex + bufferMesh.VertexReadOffset));
					}
				}
			}
		}

		private int[] GetDependingMeshNodeIndices()
		{
			SortedSet<int> result = new();

			foreach(ushort vtxIndex in _usedVertices)
			{
				for(int i = 0; i < _meshNodeIndexMapping.Length; i++)
				{
					if(_vertexIndexMapping[vtxIndex, i])
					{
						result.Add(i);
					}
				}
			}

			return result.ToArray();
		}

		public static int ComputeCommonNodeIndex(Node[] nodes, SortedSet<int> dependingNodeIndices)
		{
			Dictionary<Node, int> parentIndices = new();
			for(int i = 0; i < nodes.Length; i++)
			{
				parentIndices.Add(nodes[i], 0);
			}

			foreach(int i in dependingNodeIndices)
			{
				Node? node = nodes[i];
				while(node != null)
				{
					parentIndices[node]++;
					node = node.Parent;
				}

			}

			int target = dependingNodeIndices.Count;
			foreach(KeyValuePair<Node, int> t in parentIndices.Reverse())
			{
				if(t.Value == target)
				{
					return Array.IndexOf(nodes, t.Key);
				}
			}

			return 0;
		}

		private WeightedVertex[] EvaluateNoWeightVertices(int meshNodeIndex)
		{
			BufferMesh[] meshes = GetMeshData(meshNodeIndex);

			ushort[] indices = _usedVertices.ToArray();
			BufferVertex[] sourceVertices = new BufferVertex[0x10000];

			foreach(BufferMesh mesh in meshes)
			{
				if(mesh.Vertices == null)
				{
					continue;
				}

				for(int i = 0; i < mesh.Vertices.Length; i++)
				{
					BufferVertex sourceVertex = mesh.Vertices[i];
					sourceVertices[sourceVertex.Index + mesh.VertexWriteOffset] = sourceVertex;
				}
			}

			WeightedVertex[] resultVertices = new WeightedVertex[indices.Length];

			for(int i = 0; i < indices.Length; i++)
			{
				int vertexIndex = indices[i];
				BufferVertex sourceVertex = sourceVertices[vertexIndex];

				WeightedVertex resultVert = new(
					sourceVertex.Position,
					sourceVertex.Normal);

				resultVertices[i] = resultVert;
			}

			return resultVertices;
		}

		/// <param name="meshNodeIndices">Indices to meshes to use</param>
		/// <param name="baseNodeIndex">Index to the node that the vertex data should be made relative to.</param>
		/// <returns></returns>
		private WeightedVertex[] EvaluateWeightVertices(int[] meshNodeIndices, int baseNodeIndex)
		{
			Matrix4x4 baseMatrix = _worldMatrices[_nodes[baseNodeIndex]];
			Matrix4x4.Invert(baseMatrix, out Matrix4x4 invbaseMatrix);

			int meshNodeCount = _meshNodeIndexMapping[meshNodeIndices[^1]] - baseNodeIndex + 1;
			WeightedVertex[] vertexBuffer = new WeightedVertex[0x10000];

			foreach(int meshNodeIndex in meshNodeIndices)
			{
				int nodeIndex = _meshNodeIndexMapping[meshNodeIndex];

				Matrix4x4 vertexMatrix = Matrix4x4.Identity;
				if(baseNodeIndex != nodeIndex)
				{
					Node meshNode = _nodes[nodeIndex];
					Matrix4x4 worldMatrix = _worldMatrices[meshNode];

					vertexMatrix = worldMatrix * invbaseMatrix;
				}

				Matrix4x4 normalMatrix = vertexMatrix.GetNormalMatrix();

				BufferMesh[] meshes = GetMeshData(meshNodeIndex);
				int weightIndex = nodeIndex - baseNodeIndex;

				foreach(BufferMesh bufferMesh in meshes)
				{
					if(bufferMesh.Vertices == null)
					{
						continue;
					}

					foreach(BufferVertex vtx in bufferMesh.Vertices)
					{
						int index = vtx.Index + bufferMesh.VertexWriteOffset;
						if(vtx.Weight == 0.0f)
						{
							if(!bufferMesh.ContinueWeight)
							{
								vertexBuffer[index] = new(default, default, meshNodeCount);
							}

							continue;
						}

						Vector3 pos = Vector3.Transform(vtx.Position, vertexMatrix) * vtx.Weight;
						Vector3 nrm = Vector3.TransformNormal(vtx.Normal, normalMatrix) * vtx.Weight;

						if(bufferMesh.ContinueWeight)
						{
							vertexBuffer[index].Position += pos;
							vertexBuffer[index].Normal += nrm;
						}
						else
						{
							vertexBuffer[index] = new(pos, nrm, meshNodeCount);
						}

						// Its possible that this mesh continues vertices from another model of which the
						// init mesh was not included, so the weights array can be null.
						float[]? weights = vertexBuffer[index].Weights;
						if(weights != null)
						{
							weights[weightIndex] = vtx.Weight;
						}

					}
				}
			}

			ushort[] indices = _usedVertices.ToArray();
			WeightedVertex[] resultVertices = new WeightedVertex[indices.Length];

			for(int i = 0; i < indices.Length; i++)
			{
				int vertexIndex = indices[i];
				WeightedVertex vertex = vertexBuffer[vertexIndex];
				vertex.Normal = Vector3.Normalize(vertex.Normal);
				float sum = vertex.Weights?.Sum() ?? 1f;

				if(sum != 1)
				{
					vertex.Position /= sum;
					vertex.Normal /= sum;

					for(int j = 0; j < vertex.Weights!.Length; j++)
					{
						vertex.Weights[j] /= sum;
					}
				}

				resultVertices[i] = vertex;
			}

			return resultVertices;
		}

		private ushort[] GetVertexIndexMap()
		{
			ushort[] indices = _usedVertices.ToArray();

			ushort[] result = new ushort[indices[^1] + 1];
			for(ushort i = 0; i < indices.Length; i++)
			{
				result[indices[i]] = i;
			}

			return result;
		}

		private (BufferCorner[][] triangleSets, BufferMaterial[] materials, bool hasColors) EvaluatePolygons()
		{
			BufferMesh[] meshes = GetMeshData(_currentMeshIndex);

			ushort[] vertexIndexMap = GetVertexIndexMap();

			bool hasColors = false;
			List<BufferCorner[]> triangleSets = new();
			List<BufferMaterial> materials = new();

			foreach(BufferMesh bufferMesh in meshes)
			{
				if(bufferMesh.Corners == null)
				{
					continue;
				}

				hasColors |= bufferMesh.HasColors;

				BufferCorner[] corners = bufferMesh.GetCornerTriangleList();

				for(int i = 0; i < corners.Length; i++)
				{
					corners[i].VertexIndex = vertexIndexMap[corners[i].VertexIndex + bufferMesh.VertexReadOffset];
				}

				triangleSets.Add(corners);
				materials.Add(bufferMesh.Material);
			}

			return (triangleSets.ToArray(), materials.ToArray(), hasColors);
		}

		private void EvaluateMesh()
		{
			int[] dependingMeshNodeIndices = GetDependingMeshNodeIndices();
			int rootNodeIndex = _meshNodeIndexMapping[dependingMeshNodeIndices[0]];
			SortedSet<int> dependingRelativeNodeIndices = new();
			string label;

			WeightedVertex[] vertices;

			if(dependingMeshNodeIndices.Length == 1)
			{
				if(_unweighted.TryGetValue(_nodes[rootNodeIndex].Attach ?? throw new NullReferenceException(), out WeightedMesh? reuseWBA))
				{
					reuseWBA.RootIndices.Add(rootNodeIndex);
					return;
				}

				vertices = EvaluateNoWeightVertices(dependingMeshNodeIndices[0]);
				label = _nodes[rootNodeIndex].Attach!.Label;
			}
			else
			{
				SortedSet<int> absoluteDepends = new(dependingMeshNodeIndices.Select(x => _meshNodeIndexMapping[x]));
				rootNodeIndex = ComputeCommonNodeIndex(_nodes, absoluteDepends);
				dependingRelativeNodeIndices = new(absoluteDepends.Select(x => x - rootNodeIndex));

				vertices = EvaluateWeightVertices(dependingMeshNodeIndices, rootNodeIndex);
				label = _nodes[absoluteDepends.Max].Attach!.Label;
			}

			(BufferCorner[][] triangleSets, BufferMaterial[] materials, bool hasColors) = EvaluatePolygons();

			WeightedMesh wba = new(
				vertices,
				triangleSets,
				materials,
				new() { rootNodeIndex },
				dependingRelativeNodeIndices,
				hasColors
				)
			{
				Label = label
			};

			if(dependingRelativeNodeIndices.Count == 0)
			{
				_unweighted.Add(_nodes[rootNodeIndex].Attach ?? throw new NullReferenceException(), wba);
			}

			_output.Add(wba);
		}

		private WeightedMesh[] Process()
		{
			if(_finished)
			{
				return _output.ToArray();
			}

			Verify();
			Setup();

			foreach(Node node in _nodes)
			{
				if(node.Attach == null || node.Attach.MeshData.Length == 0)
				{
					continue;
				}

				_currentMeshIndex++;

				MapVertices();

				if(_usedVertices.Count > 0)
				{
					EvaluateMesh();
				}
			}

			_finished = true;
			return _output.ToArray();
		}

		public static WeightedMesh[] ConvertToWeighted(Node model)
		{
			return new ToWeightedConverter(model).Process();
		}
	}
}
