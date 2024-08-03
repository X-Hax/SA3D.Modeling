using SA3D.Common;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Mesh.Weighted;
using SA3D.Modeling.ObjectData;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Converters
{
	internal static class FromWeightedConverter
	{
		private readonly struct BufferResult : IOffsetableAttachResult
		{
			public string Label { get; }
			public int VertexCount { get; }
			public bool Weighted { get; }
			public int[] AttachIndices { get; }
			public Attach[] Attaches { get; }

			public BufferResult(string label, int vertexCount, bool weighted, int[] attachIndices, Attach[] attaches)
			{
				Label = label;
				VertexCount = vertexCount;
				Weighted = weighted;
				AttachIndices = attachIndices;
				Attaches = attaches;
			}

			public void ModifyVertexOffset(int offset)
			{
				foreach(Attach atc in Attaches)
				{
					foreach(BufferMesh bm in atc.MeshData)
					{
						bm.VertexWriteOffset = (ushort)(bm.VertexWriteOffset + offset);
						bm.VertexReadOffset = (ushort)(bm.VertexReadOffset + offset);
					}
				}
			}
		}

		private class OffsettableBufferConverter : OffsetableAttachConverter<BufferResult>
		{
			protected override BufferResult ConvertWeighted(WeightedMesh wba, bool optimize)
			{
				List<(int nodeIndex, BufferMesh[])> meshSets = [];
				int[] weightInits = wba.Vertices.Select(x => x.GetFirstWeightIndex()).ToArray();

				foreach(int nodeIndex in wba.DependingNodeIndices)
				{
					List<BufferVertex> initVerts = [];
					List<BufferVertex> continueVerts = [];

					for(int i = 0; i < wba.Vertices.Length; i++)
					{
						WeightedVertex wVert = wba.Vertices[i];

						float weight = wVert.Weights![nodeIndex];
						if(weight == 0)
						{
							continue;
						}

						BufferVertex vert = new(wVert.Position, wVert.Normal, (ushort)i, weight);

						if(weightInits[i] == nodeIndex)
						{
							initVerts.Add(vert);
						}
						else
						{
							continueVerts.Add(vert);
						}
					}

					List<BufferMesh> vertexMeshes = [];

					if(initVerts.Count > 0)
					{
						vertexMeshes.Add(new(initVerts.ToArray(), false, true, 0));
					}

					if(continueVerts.Count > 0)
					{
						vertexMeshes.Add(new(continueVerts.ToArray(), true, true, 0));
					}

					meshSets.Add((nodeIndex, vertexMeshes.ToArray()));
				}

				BufferMesh[] polyMeshes = GetPolygonMeshes(wba, optimize);

				int[] nodeIndices = new int[meshSets.Count];
				Attach[] attaches = new Attach[meshSets.Count];

				for(int i = 0; i < meshSets.Count - 1; i++)
				{
					(int nodeIndex, BufferMesh[] vertexMeshes) = meshSets[i];
					nodeIndices[i] = nodeIndex;
					attaches[i] = new(vertexMeshes);
				}

				int lastIndex = meshSets.Count - 1;
				(int lastNodeIndex, BufferMesh[] lastMeshes) = meshSets[lastIndex];
				nodeIndices[lastIndex] = lastNodeIndex;

				List<BufferMesh> meshes = [.. lastMeshes, .. polyMeshes];
				attaches[lastIndex] = new(meshes.ToArray());

				return new(
					wba.Label ?? "BUFFER_" + StringExtensions.GenerateIdentifier(),
					wba.Vertices.Length,
					true,
					nodeIndices,
					attaches);
			}

			protected override BufferResult ConvertWeightless(WeightedMesh wba, bool optimize)
			{
				List<BufferMesh> meshes = [];

				BufferVertex[] vertices = new BufferVertex[wba.Vertices.Length];

				for(int i = 0; i < vertices.Length; i++)
				{
					WeightedVertex wVert = wba.Vertices[i];
					vertices[i] = new(wVert.Position, wVert.Normal, (ushort)i);
				}

				BufferMesh[] polygonMeshes = GetPolygonMeshes(wba, optimize);

				meshes.Add(new(vertices, false, true, 0));

				meshes.AddRange(polygonMeshes);

				BufferMesh[] result = BufferMesh.CompressLayout(meshes);

				return new(
					wba.Label ?? "BUFFER_" + StringExtensions.GenerateIdentifier(),
					vertices.Length,
					false,
					wba.RootIndices.ToArray(),
					new Attach[] { new(result) });
			}

			private static BufferMesh[] GetPolygonMeshes(WeightedMesh wba, bool optimize)
			{
				List<BufferMesh> result = [];

				for(int i = 0; i < wba.TriangleSets.Length; i++)
				{
					wba.Materials[i].BackfaceCulling = false;
					BufferMesh mesh = new(
						wba.Materials[i],
						(BufferCorner[])wba.TriangleSets[i].Clone(),
						null,
						false,
						wba.HasColors,
						0);

					if(optimize)
					{
						mesh.OptimizePolygons();
					}

					result.Add(mesh);
				}

				return result.ToArray();
			}

			protected override void CorrectSpace(Attach attach, Matrix4x4 vertexMatrix, Matrix4x4 normalMatrix)
			{
				foreach(BufferMesh mesh in attach.MeshData)
				{
					if(mesh.Vertices == null)
					{
						continue;
					}

					for(int j = 0; j < mesh.Vertices.Length; j++)
					{
						BufferVertex vertex = mesh.Vertices[j];

						vertex.Position = Vector3.Transform(vertex.Position, vertexMatrix);
						vertex.Normal = Vector3.TransformNormal(vertex.Normal, normalMatrix);

						mesh.Vertices[j] = vertex;
					}
				}
			}

			protected override BufferResult CreateWeightedResult(string label, int vertexCount, int[] attachIndices, Attach[] attaches)
			{
				return new(
					label,
					vertexCount,
					true,
					attachIndices,
					attaches);
			}

			protected override Attach CombineAttaches(List<Attach> attaches, string label)
			{
				List<BufferMesh> meshes = [];

				foreach(Attach atc in attaches)
				{
					meshes.AddRange(atc.MeshData);
				}

				return new Attach(meshes.ToArray()) { Label = label };
			}

		}

		public static void Convert(Node model, WeightedMesh[] meshData, bool optimize)
		{
			new OffsettableBufferConverter().Convert(model, meshData, optimize);
		}
	}
}
