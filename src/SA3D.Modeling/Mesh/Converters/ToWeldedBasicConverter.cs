using SA3D.Common;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Mesh.Weighted;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.ObjectData.Structs;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Converters
{
	internal class ToWeldedBasicConverter
	{
		private class MeshContainer
		{
			public WeightedMesh source;
			public WeightedVertex[] vertices;
			public ushort[]? vertexMap;
			public ushort[]? invVertexMap;
			public HashSet<ushort>? polygonRelevantVertexIndices;
			public BufferCorner[][] triangleSets;
			public BufferMaterial[] materials;
			public int relativeNodeIndex;
			public int rootNodeIndex;

			public ushort vertexOffset;

			public MeshContainer(WeightedMesh source, WeightedVertex[] vertices, ushort[]? vertexMap, ushort[]? invVertexMap, HashSet<ushort>? polygonRelevantVertexIndices, BufferCorner[][] triangleSets, BufferMaterial[] materials, int relativeNodeIndex)
			{
				this.source = source;
				this.vertices = vertices;
				this.vertexMap = vertexMap;

				if(invVertexMap == null && vertexMap != null)
				{
					invVertexMap = new ushort[vertexMap.Max() + 1];
					for(ushort i = 0; i < vertexMap.Length; i++)
					{
						invVertexMap[vertexMap[i]] = i;
					}
				}
				else
				{
					this.invVertexMap = invVertexMap;
				}

				this.polygonRelevantVertexIndices = polygonRelevantVertexIndices;
				this.triangleSets = triangleSets;
				this.materials = materials;
				this.relativeNodeIndex = relativeNodeIndex;
			}

			public string GetLabel()
			{
				if(source.Label == null)
				{
					return "attach_" + StringExtensions.GenerateIdentifier();
				}

				string label = source.Label;

				if(source.RootIndices.Count > 0)
				{
					label += $"_{rootNodeIndex}";
				}

				if(source.IsWeighted)
				{
					label += $"_{relativeNodeIndex}";
				}

				return label;
			}

			public void CorrectVertexSpace(Matrix4x4 rootMatrix, Matrix4x4 targetMatrix)
			{
				Matrix4x4.Invert(targetMatrix, out Matrix4x4 invTargetMatrix);
				Matrix4x4 vertexMatrix = rootMatrix * invTargetMatrix;
				Matrix4x4 normalMatrix = vertexMatrix.GetNormalMatrix();

				for(int i = 0; i < vertices.Length; i++)
				{
					WeightedVertex vertex = vertices[i];

					vertex.Position = Vector3.Transform(vertex.Position, vertexMatrix);
					vertex.Normal = Vector3.TransformNormal(vertex.Normal, normalMatrix);

					vertices[i] = vertex;
				}
			}
		}

		private readonly Node _model;
		private readonly WeightedMesh[] _meshData;
		private readonly bool _optimize;

		private readonly (Node node, Matrix4x4 worldMatrix)[] _nodesMatrices;

		private readonly List<MeshContainer>[] _nodeMeshContainers;

		private ToWeldedBasicConverter(
			Node model,
			WeightedMesh[] meshData,
			bool optimize)
		{
			_model = model;
			_meshData = meshData;
			_optimize = optimize;
			if(_optimize)
			{

			}

			_nodesMatrices = _model.GetWorldMatrixTree();

			_nodeMeshContainers = new List<MeshContainer>[_nodesMatrices.Length];
			for(int i = 0; i < _nodesMatrices.Length; i++)
			{
				_nodeMeshContainers[i] = new();
			}
		}

		public void Process()
		{
			foreach(WeightedMesh weightedMesh in _meshData)
			{
				if(weightedMesh.IsWeighted)
				{
					SplitWeightedMesh(weightedMesh);
				}
				else
				{
					MeshContainer container = new(
						weightedMesh,
						weightedMesh.Vertices,
						null, null, null,
						weightedMesh.TriangleSets,
						weightedMesh.Materials,
						0);

					foreach(int index in weightedMesh.RootIndices)
					{
						AddContainer(index, container, index);
					}
				}
			}

			_model.ClearAttachesFromTree();
			_model.ClearWeldingsFromTree();

			for(int i = 0; i < _nodeMeshContainers.Length; i++)
			{
				List<MeshContainer> containers = _nodeMeshContainers[i];

				if(containers.Count == 0)
				{
					continue;
				}

				WeightedVertex[] vertices;
				BufferCorner[][] triangleSets;
				BufferMaterial[] materials;
				bool hasColors;
				string label;

				if(containers.Count == 1)
				{
					MeshContainer container = containers[0];

					vertices = container.vertices;
					triangleSets = container.triangleSets;
					materials = container.materials;
					hasColors = container.source.HasColors;
					label = container.GetLabel();
				}
				else
				{
					MergeContainers(
						containers,
						out vertices,
						out triangleSets,
						out materials,
						out hasColors,
						out label);
				}

				Node node = _nodesMatrices[i].node;
				node.Attach = BasicConverter.CreateBasicAttach(vertices, triangleSets, materials, hasColors, label);
				node.Welding = AssembleVertexWelding(containers);
			}
		}

		private void AddContainer(int index, MeshContainer container, int rootIndex)
		{
			List<MeshContainer> containers = _nodeMeshContainers[index];
			ushort vertexOffset = 0;

			if(containers.Count > 0)
			{
				MeshContainer last = containers[^1];
				vertexOffset = (ushort)(last.vertexOffset + last.vertices.Length);
			}

			MeshContainer newContainer = new(
				container.source,
				(WeightedVertex[])container.vertices.Clone(),
				container.vertexMap,
				container.invVertexMap,
				container.polygonRelevantVertexIndices,
				container.triangleSets.ContentClone(),
				container.materials,
				container.relativeNodeIndex)
			{
				rootNodeIndex = rootIndex,
				vertexOffset = vertexOffset
			};

			if(newContainer.relativeNodeIndex != 0)
			{
				Matrix4x4 rootMatrix = _nodesMatrices[rootIndex].worldMatrix;
				Matrix4x4 targetMatrix = _nodesMatrices[rootIndex + newContainer.relativeNodeIndex].worldMatrix;
				newContainer.CorrectVertexSpace(rootMatrix, targetMatrix);
			}

			if(vertexOffset > 0)
			{
				foreach(BufferCorner[] triangleSet in newContainer.triangleSets)
				{
					for(int i = 0; i < triangleSet.Length; i++)
					{
						triangleSet[i].VertexIndex += vertexOffset;
					}
				}
			}

			_nodeMeshContainers[index].Add(newContainer);
		}

		private void SplitWeightedMesh(WeightedMesh weightedMesh)
		{
			BufferCorner[]?[,] splitPolygons = SplitPolygonsByWeights(weightedMesh);

			List<BufferCorner[]> tmpCorners = new();
			List<BufferMaterial> tmpMaterials = new();
			Dictionary<int, MeshContainer> containers = new();

			for(int i = 0; i < splitPolygons.GetLength(0); i++)
			{
				for(int j = 0; j < weightedMesh.TriangleSets.Length; j++)
				{
					BufferCorner[]? corners = splitPolygons[i, j];
					if(corners != null)
					{
						tmpCorners.Add(corners);
						tmpMaterials.Add(weightedMesh.Materials[j]);
					}
				}

				WeightedVertex[] vertices = ProcessTiangleSetVertices(weightedMesh.Vertices, tmpCorners, i, out HashSet<ushort>? polygonVertexIndices, out ushort[]? vertexIndexMap);

				if(tmpCorners.Count > 0 || vertices.Length > 0)
				{
					BufferCorner[][] triangleSets = tmpCorners.ToArray();
					BufferMaterial[] materials = tmpMaterials.ToArray();

					MeshContainer container = new(
						weightedMesh,
						vertices,
						vertexIndexMap,
						null,
						polygonVertexIndices,
						triangleSets,
						materials,
						i);

					containers.Add(i, container);

					tmpCorners.Clear();
					tmpMaterials.Clear();
				}
			}

			foreach(int index in weightedMesh.RootIndices)
			{
				foreach(KeyValuePair<int, MeshContainer> container in containers)
				{
					AddContainer(index + container.Key, container.Value, index);
				}
			}
		}

		private BufferCorner[]?[,] SplitPolygonsByWeights(WeightedMesh weightedMesh)
		{
			int weightNum = weightedMesh.DependingNodeIndices.Max + 1;
			float[] weightSum = new float[weightNum];

			//[relative node index, triangle set index] = null | Triangle set
			BufferCorner[]?[,] result = new BufferCorner[weightNum, weightedMesh.TriangleSets.Length][];
			List<BufferCorner>[] meshSplitPolygons = new List<BufferCorner>[weightNum];

			for(int i = 0; i < weightNum; i++)
			{
				meshSplitPolygons[i] = new();
			}

			for(int i = 0; i < weightedMesh.TriangleSets.Length; i++)
			{
				BufferCorner[] triangleSet = weightedMesh.TriangleSets[i];

				for(int j = 0; j < triangleSet.Length; j += 3)
				{
					Array.Clear(weightSum);

					for(int k = j; k < j + 3; k++)
					{
						int vertexIndex = triangleSet[k].VertexIndex;
						float[] vertexWeights = weightedMesh.Vertices[vertexIndex].Weights!;

						for(int w = 0; w < vertexWeights.Length; w++)
						{
							float weight = vertexWeights[w];
							if(weight > 0)
							{
								weightSum[w] += weight;
							}
						}
					}

					int maxWeightIndex = 0;
					float maxWeight = 0;

					for(int w = 0; w < weightSum.Length; w++)
					{
						float weight = weightSum[w];
						if(weight > maxWeight)
						{
							maxWeightIndex = w;
							maxWeight = weight;
						}
					}

					List<BufferCorner> targetList = meshSplitPolygons[maxWeightIndex];
					for(int k = j; k < j + 3; k++)
					{
						targetList.Add(triangleSet[k]);
					}
				}

				for(int j = 0; j < meshSplitPolygons.Length; j++)
				{
					List<BufferCorner> polygons = meshSplitPolygons[j];
					if(polygons.Count > 0)
					{
						result[j, i] = polygons.ToArray();
					}

					polygons.Clear();
				}
			}

			return result;
		}

		private WeightedVertex[] ProcessTiangleSetVertices(WeightedVertex[] vertices, List<BufferCorner[]> triangleSets, int targetWeightIndex, out HashSet<ushort>? polygonVertexIndices, out ushort[]? vertexIndexMap)
		{
			HashSet<ushort> vertexIndexSet = triangleSets
				.SelectMany(x => x)
				.Select(x => x.VertexIndex)
				.Order()
				.ToHashSet();

			if(vertexIndexSet.Count == vertices.Length)
			{
				vertexIndexMap = null;
				polygonVertexIndices = null;
				return vertices;
			}
			else
			{
				polygonVertexIndices = new(vertexIndexSet);
			}

			for(ushort i = 0; i < vertices.Length; i++)
			{
				if(vertices[i].Weights![targetWeightIndex] > 0f)
				{
					vertexIndexSet.Add(i);
				}
			}

			ushort[] vertexIndices = vertexIndexSet.Order().ToArray();
			if(vertexIndices.Length == vertices.Length)
			{
				vertexIndexMap = null;
				return vertices;
			}

			WeightedVertex[] usedVertices = new WeightedVertex[vertexIndices.Length];
			vertexIndexMap = new ushort[vertices.Length];

			for(ushort i = 0; i < vertexIndices.Length; i++)
			{
				int vertexIndex = vertexIndices[i];
				vertexIndexMap[vertexIndex] = i;
				usedVertices[i] = vertices[vertexIndex];
			}

			foreach(BufferCorner[] triangleSet in triangleSets)
			{
				for(int i = 0; i < triangleSet.Length; i++)
				{
					triangleSet[i].VertexIndex = vertexIndexMap[triangleSet[i].VertexIndex];
				}
			}

			return usedVertices;
		}

		private void MergeContainers(
			List<MeshContainer> containers,
			out WeightedVertex[] vertices,
			out BufferCorner[][] triangleSets,
			out BufferMaterial[] materials,
			out bool hasColors,
			out string label)
		{
			List<WeightedVertex> combinedVertices = new();
			List<BufferCorner[]> combinedTriangleSets = new();
			List<BufferMaterial> combinedMaterials = new();
			hasColors = false;

			foreach(MeshContainer container in containers)
			{
				combinedVertices.AddRange(container.vertices);
				combinedTriangleSets.AddRange(container.triangleSets);
				combinedMaterials.AddRange(container.materials);
				hasColors |= container.source.HasColors;
			}

			vertices = combinedVertices.ToArray();
			triangleSets = combinedTriangleSets.ToArray();
			materials = combinedMaterials.ToArray();
			label = containers[^1].GetLabel();
		}

		private static IEnumerable<ushort> UShortEnumerable(ushort length)
		{
			for(ushort i = 0; i < length; i++)
			{
				yield return i;
			}
		}

		private VertexWelding[]? AssembleVertexWelding(List<MeshContainer> containers)
		{
			List<VertexWelding> welding = new();

			foreach(MeshContainer container in containers)
			{
				if(!container.source.IsWeighted || container.triangleSets.Length == 0)
				{
					continue;
				}

				Node[] sourceNodes = _nodesMatrices.Select(x => x.node).Skip(container.rootNodeIndex).ToArray();
				MeshContainer[] sourceContainers = new MeshContainer[sourceNodes.Length];

				for(int i = container.rootNodeIndex; i < _nodeMeshContainers.Length; i++)
				{
					MeshContainer? sourceContainer = _nodeMeshContainers[i].FirstOrDefault(x =>
						x.source == container.source && x.rootNodeIndex == container.rootNodeIndex);

					if(sourceContainer != null)
					{
						sourceContainers[sourceContainer.relativeNodeIndex] = sourceContainer;
					}
				}

				IEnumerable<ushort> vertexIndices =
					container.polygonRelevantVertexIndices
					?? UShortEnumerable((ushort)container.vertices.Length);

				foreach(ushort vertexIndex in vertexIndices)
				{
					// Note: polygon indices have never been converted to the local index.

					ushort targetVertexIndex = container.vertexMap?[vertexIndex] ?? vertexIndex;
					WeightedVertex vertex = container.vertices[targetVertexIndex];
					if(vertex.Weights![container.relativeNodeIndex] == 1)
					{
						continue;
					}

					List<Weld> welds = new();

					for(int i = 0; i < vertex.Weights.Length; i++)
					{
						float weight = vertex.Weights[i];
						if(weight == 0)
						{
							continue;
						}

						MeshContainer sourceContainer = sourceContainers[i];

						uint sourceVertexIndex = sourceContainer.vertexMap?[vertexIndex] ?? vertexIndex;
						sourceVertexIndex += sourceContainer.vertexOffset;

						if(i < container.relativeNodeIndex && sourceContainer.polygonRelevantVertexIndices?.Contains(vertexIndex) == true)
						{
							welds.Clear();
							welds.Add(new(sourceNodes[i], sourceVertexIndex, 1));
							break;
						}
						else
						{
							welds.Add(new(sourceNodes[i], sourceVertexIndex, weight));
						}
					}

					targetVertexIndex += container.vertexOffset;
					welding.Add(new(targetVertexIndex, welds.ToArray()));
				}
			}

			return welding.Count == 0 ? null : welding.ToArray();
		}

		public static void ConvertWeightedToWeldedBasic(
			Node model,
			WeightedMesh[] meshData,
			bool optimize)
		{
			new ToWeldedBasicConverter(model, meshData, optimize).Process();
		}
	}
}
