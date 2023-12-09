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
			public int[]? sharedVertexmap;

			public WeightedVertex[] vertices;

			/// <summary>
			/// <see cref="vertices"/> index -> Source vertex index
			/// </summary>
			public ushort[]? polyVertexMap;

			/// <summary>
			/// Source vertex index -> <see cref="vertices"/>
			/// </summary>
			public ushort[]? weightVertexMap;

			/// <summary>
			/// Polygons with already translated vertex indices.
			/// </summary>
			public BufferCorner[][] triangleSets;
			public BufferMaterial[] materials;

			/// <summary>
			/// Index to the root node.
			/// </summary>
			public int rootNodeIndex;

			/// <summary>
			/// Node index relative to the root node
			/// </summary>
			public int relativeNodeIndex;

			public ushort vertexOffset;

			public MeshContainer(
				WeightedMesh source,
				int[]? sharedVertexmap,
				WeightedVertex[] vertices,
				ushort[]? polyVertexMap,
				ushort[]? weightVertexMap,
				BufferCorner[][] triangleSets,
				BufferMaterial[] materials,
				int relativeNodeIndex)
			{
				this.source = source;
				this.sharedVertexmap = sharedVertexmap;
				this.vertices = vertices;
				this.polyVertexMap = polyVertexMap;
				this.weightVertexMap = weightVertexMap;
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
					vertex.Normal = Vector3.Normalize(Vector3.TransformNormal(vertex.Normal, normalMatrix));

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
				MeshContainer[] containers = weightedMesh.IsWeighted
					? SplitWeightedMesh(weightedMesh, _optimize)
					: new[] { new MeshContainer(
							weightedMesh,
							null,
							weightedMesh.Vertices,
							null, null,
							weightedMesh.TriangleSets,
							weightedMesh.Materials,
							0)};

				foreach(int index in weightedMesh.RootIndices)
				{
					foreach(MeshContainer container in containers)
					{
						AddContainer(index + container.relativeNodeIndex, container, index);
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

				Node node = _nodesMatrices[i].node;
				node.Welding = AssembleVertexWelding(containers);

				WeightedVertex[] vertices;
				BufferCorner[][] triangleSets;
				BufferMaterial[] materials;
				bool hasColors;
				string label;

				if(containers.Count == 1)
				{
					MeshContainer container = containers[0];

					vertices = container.vertices.ToArray();
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

				node.Attach = BasicConverter.CreateBasicAttach(vertices, triangleSets, materials, hasColors, label);
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
				container.sharedVertexmap,
				container.vertices,
				container.polyVertexMap,
				container.weightVertexMap,
				container.triangleSets.ContentClone(),
				container.materials,
				container.relativeNodeIndex)
			{
				rootNodeIndex = rootIndex,
				vertexOffset = vertexOffset,
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

		private static MeshContainer[] SplitWeightedMesh(WeightedMesh weightedMesh, bool optimize)
		{
			int maxIndex = weightedMesh.DependingNodeIndices.Max;
			int nodeCount = maxIndex + 1;

			//[relative node index, triangle set index] = null | Triangle set
			BufferCorner[]?[,] splitPolygons = new BufferCorner[nodeCount, weightedMesh.TriangleSets.Length][];
			int[] sharedVertexmap = new int[weightedMesh.Vertices.Length];

			if(optimize)
			{
				SplitPolygonsByWeights(weightedMesh, splitPolygons);
				GetSharedVertexMap(splitPolygons, sharedVertexmap);
			}
			else
			{
				for(int i = 0; i < weightedMesh.TriangleSets.Length; i++)
				{
					splitPolygons[maxIndex, i] = weightedMesh.TriangleSets[i];
				}

				Array.Fill(sharedVertexmap, -1);
			}

			List<BufferCorner[]> tmpCorners = new();
			List<BufferMaterial> tmpMaterials = new();
			List<MeshContainer> containers = new();

			for(int i = 0; i < nodeCount; i++)
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

				MeshContainer? container = AssembleVertices(
					weightedMesh,
					i,
					sharedVertexmap,
					tmpCorners,
					tmpMaterials);

				if(container != null)
				{
					containers.Add(container);
				}

				tmpCorners.Clear();
				tmpMaterials.Clear();
			}

			return containers.ToArray();
		}

		private static void SplitPolygonsByWeights(WeightedMesh weightedMesh, BufferCorner[]?[,] result)
		{
			int weightNum = result.GetLength(0);
			float[] weightSum = new float[weightNum];
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
		}

		/// <summary>
		/// Fills a mapping for all vertices that are used for polygons in multiples attaches where: 
		/// <br/> [source vertex index] = index of the first node that uses the vertex
		/// </summary>
		private static void GetSharedVertexMap(BufferCorner[]?[,] polygons, int[] result)
		{
			int[,] counts = new int[result.Length, 2];
			int meshCount = polygons.GetLength(1);
			HashSet<ushort> usedIndices = new();

			for(int i = polygons.GetLength(0) - 1; i >= 0; i--)
			{
				usedIndices.Clear();
				for(int j = 0; j < meshCount; j++)
				{
					BufferCorner[]? corners = polygons[i, j];
					if(corners != null)
					{
						usedIndices.UnionWith(corners.Select(x => x.VertexIndex));
					}
				}

				foreach(ushort vertexIndex in usedIndices)
				{
					counts[vertexIndex, 0]++;
					counts[vertexIndex, 1] = i;
				}
			}

			for(int i = 0; i < result.Length; i++)
			{
				result[i] = counts[i, 0] <= 1
					? -1
					: counts[i, 1];
			}

		}

		private static MeshContainer? AssembleVertices(
			WeightedMesh source,
			int relativeNodeIndex,
			int[] sharedVertexmap,
			List<BufferCorner[]> triangleSets,
			List<BufferMaterial> materials)
		{
			/****************/
			// Evaluating polygon vertices

			HashSet<ushort> polygonVertexIndices = triangleSets
				.SelectMany(x => x)
				.Select(x => x.VertexIndex)
				.Order()
				.ToHashSet();

			ushort[] polygonVertexMap = new ushort[polygonVertexIndices.Count];
			ushort[] vertexIndexMap = new ushort[source.Vertices.Length];
			List<WeightedVertex> vertices = new();

			foreach(ushort index in polygonVertexIndices)
			{
				polygonVertexMap[vertices.Count] = index;
				vertexIndexMap[index] = (ushort)vertices.Count;
				vertices.Add(source.Vertices[index]);
			}

			foreach(BufferCorner[] corners in triangleSets)
			{
				for(int i = 0; i < corners.Length; i++)
				{
					corners[i].VertexIndex = vertexIndexMap[corners[i].VertexIndex];
				}
			}

			/****************/
			// Evaluating Weighted vertices

			List<WeightedVertex> weightVertexIndices = new();

			for(ushort i = 0; i < source.Vertices.Length; i++)
			{
				WeightedVertex vertex = source.Vertices[i];

				if(vertex.Weights![relativeNodeIndex] == 0)
				{
					continue;
				}

				int sharedIndex = sharedVertexmap[i];
				bool contains = polygonVertexIndices.Contains(i);

				if(!contains || (sharedIndex != -1 && sharedIndex < relativeNodeIndex))
				{
					vertexIndexMap[i] = (ushort)vertices.Count;
					vertices.Add(vertex);
				}
			}

			/****************/
			// Evaluating container

			if(vertices.Count == 0)
			{
				return null;
			}

			return new MeshContainer(
				source,
				sharedVertexmap,
				vertices.ToArray(),
				polygonVertexMap,
				vertexIndexMap,
				triangleSets.ToArray(),
				materials.ToArray(),
				relativeNodeIndex);
		}

		private static void MergeContainers(
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

		private VertexWelding[]? AssembleVertexWelding(List<MeshContainer> containers)
		{
			List<VertexWelding> welding = new();

			foreach(MeshContainer container in containers)
			{
				if(container.polyVertexMap == null)
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

				for(uint i = 0; i < container.polyVertexMap.Length; i++)
				{
					WeightedVertex vertex = container.vertices[i];
					if(vertex.Weights![container.relativeNodeIndex] == 1)
					{
						continue;
					}

					List<Weld> welds = new();
					ushort sourceVertexIndex = container.polyVertexMap[i];
					uint targetVertexIndex = i + container.vertexOffset;

					void AddWeld(int nodeIndex, float weight)
					{
						uint weightIndex;
						if(nodeIndex == container.relativeNodeIndex)
						{
							weightIndex = targetVertexIndex;
						}
						else
						{
							MeshContainer sourceContainer = sourceContainers[nodeIndex];
							weightIndex = (uint)(sourceContainer.weightVertexMap![sourceVertexIndex] + sourceContainer.vertexOffset);
						}

						welds.Add(new(sourceNodes[nodeIndex], weightIndex, weight));
					}

					int sharedIndex = container.sharedVertexmap![sourceVertexIndex];
					if(sharedIndex != -1 && sharedIndex < container.relativeNodeIndex)
					{
						AddWeld(sharedIndex, 1);
					}
					else
					{
						for(int j = 0; j < vertex.Weights.Length; j++)
						{
							float weight = vertex.Weights[j];
							if(weight > 0)
							{
								AddWeld(j, weight);
							}
						}
					}

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
