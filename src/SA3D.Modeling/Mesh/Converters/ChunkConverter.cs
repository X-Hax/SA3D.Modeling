using SA3D.Common;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Mesh.Chunk;
using SA3D.Modeling.Mesh.Chunk.PolyChunks;
using SA3D.Modeling.Mesh.Chunk.Structs;
using SA3D.Modeling.Mesh.Weighted;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Strippify;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Converters
{
	internal static class ChunkConverter
	{
		private readonly struct ChunkResult : IOffsetableAttachResult
		{
			public string Label { get; }
			public int VertexCount { get; }
			public bool Weighted { get; }
			public int[] AttachIndices { get; }
			public ChunkAttach[] Attaches { get; }

			Attach[] IOffsetableAttachResult.Attaches => Attaches;

			public ChunkResult(string label, int vertexCount, bool weighted, int[] attachIndices, ChunkAttach[] attaches)
			{
				Label = label;
				VertexCount = vertexCount;
				Weighted = weighted;
				AttachIndices = attachIndices;
				Attaches = attaches;
			}

			public void ModifyVertexOffset(int offset)
			{
				foreach(ChunkAttach attach in Attaches)
				{
					if(attach.VertexChunks != null)
					{
						foreach(VertexChunk vtx in attach.VertexChunks.OfType<VertexChunk>())
						{
							vtx.IndexOffset += (ushort)offset;
						}
					}

					if(attach.PolyChunks != null)
					{
						foreach(StripChunk stripChunk in attach.PolyChunks.OfType<StripChunk>())
						{
							foreach(ChunkStrip strip in stripChunk.Strips)
							{
								for(int i = 0; i < strip.Corners.Length; i++)
								{
									strip.Corners[i].Index += (ushort)offset;
								}
							}
						}
					}
				}
			}
		}

		private class OffsettableChunkConverter : OffsetableAttachConverter<ChunkResult>
		{
			private readonly struct IndexedWeightVertex
			{
				public readonly int index;
				public readonly WeightedVertex vertex;

				public IndexedWeightVertex(int index, WeightedVertex vertex)
				{
					this.index = index;
					this.vertex = vertex;
				}

				public override string ToString()
				{
					return $"{index} {vertex}";
				}
			}

			private readonly struct BinaryWeightColorVertex : IEquatable<BinaryWeightColorVertex>
			{
				public readonly int nodeIndex;
				public readonly Vector3 position;
				public readonly Color color;

				public BinaryWeightColorVertex(int nodeIndex, Vector3 position, Color color)
				{
					this.nodeIndex = nodeIndex;
					this.position = position;
					this.color = color;
				}

				public override bool Equals(object? obj)
				{
					return obj is BinaryWeightColorVertex vertex &&
						   nodeIndex == vertex.nodeIndex &&
						   position.Equals(vertex.position) &&
						   color.Equals(vertex.color);
				}

				bool IEquatable<BinaryWeightColorVertex>.Equals(BinaryWeightColorVertex other)
				{
					return Equals(other);
				}

				public override int GetHashCode()
				{
					return HashCode.Combine(nodeIndex, position, color);
				}
			}


			private static ChunkResult ConvertWeightedBinaryColored(WeightedMesh wba)
			{
				List<BinaryWeightColorVertex> vertices = new();
				ChunkCorner[][] cornerSets = new ChunkCorner[wba.TriangleSets.Length][];

				// Get every vertex per corner
				for(int i = 0; i < wba.TriangleSets.Length; i++)
				{
					BufferCorner[] bufferCorners = wba.TriangleSets[i];
					ChunkCorner[] corners = new ChunkCorner[bufferCorners.Length];
					for(int j = 0; j < bufferCorners.Length; j++)
					{
						BufferCorner bc = bufferCorners[j];
						corners[j] = ChunkCorner.DefaultValues;
						corners[j].Index = (ushort)vertices.Count;
						corners[j].Texcoord = bc.Texcoord;
						

						WeightedVertex vertex = wba.Vertices[bc.VertexIndex];
						vertices.Add(new(vertex.GetMaxWeightIndex(), vertex.Position, bc.Color));
					}

					cornerSets[i] = corners;
				}

				// first, get rid of all duplicate vertices
				DistinctMap<BinaryWeightColorVertex> distinctVerts = vertices.CreateDistinctMap();
				(int index, BinaryWeightColorVertex vert)[] sortedVertices = new (int index, BinaryWeightColorVertex)[distinctVerts.Values.Count];

				for(int i = 0; i < sortedVertices.Length; i++)
				{
					sortedVertices[i] = (i, distinctVerts.Values[i]);
				}

				// now sort the vertices by node index
				sortedVertices = sortedVertices.OrderBy(x => x.vert.nodeIndex).ToArray();

				// Create a vertex chunk per node index
				List<(int nodeIndex, VertexChunk chunk)> vertexChunks = new();

				int currentNodeIndex = -1;
				List<ChunkVertex> chunkVertices = new();
				ushort currentVertexOffset = 0;
				int[] sortedVertMap = new int[sortedVertices.Length];
				for(int i = 0; i < sortedVertices.Length; i++)
				{
					(int index, BinaryWeightColorVertex vert) vert = sortedVertices[i];
					if(vert.vert.nodeIndex != currentNodeIndex)
					{
						if(chunkVertices.Count > 0)
						{
							vertexChunks.Add((currentNodeIndex, new(
								VertexChunkType.Diffuse,
								WeightStatus.Start,
								currentVertexOffset,
								chunkVertices.ToArray())));
						}

						currentVertexOffset = (ushort)i;
						chunkVertices.Clear();
						currentNodeIndex = vert.vert.nodeIndex;
					}

					chunkVertices.Add(new(vert.vert.position, vert.vert.color, Color.ColorWhite));
					sortedVertMap[vert.index] = i;
				}

				vertexChunks.Add((currentNodeIndex, new(
					VertexChunkType.Diffuse,
					WeightStatus.Start,
					currentVertexOffset,
					chunkVertices.ToArray())));

				// get the poly chunks
				List<PolyChunk> polyChunks = new();
				for(int i = 0; i < cornerSets.Length; i++)
				{
					ChunkCorner[] corners = cornerSets[i];
					for(int j = 0; j < corners.Length; j++)
					{
						int index = distinctVerts[corners[j].Index];
						corners[j].Index = (ushort)sortedVertMap[index];
					}

					polyChunks.AddRange(CreateStripChunk(corners, wba.Materials[i], wba.WriteSpecular));
				}

				// assemble the attaches
				List<int> nodeAttachIndices = new();
				List<ChunkAttach> attaches = new();

				for(int i = 0; i < vertexChunks.Count - 1; i++)
				{
					(int nodeIndex, VertexChunk chunks) = vertexChunks[i];
					nodeAttachIndices.Add(nodeIndex);
					attaches.Add(new(new[] { chunks }, null));
				}

				(int lastNodeindex, VertexChunk lastVertexChunk) = vertexChunks[^1];
				nodeAttachIndices.Add(lastNodeindex);
				attaches.Add(new(new[] { lastVertexChunk }, polyChunks.ToArray()));

				return new(
					wba.Label ?? "CHUNK_" + StringExtensions.GenerateIdentifier(),
					sortedVertices.Length,
					true,
					nodeAttachIndices.ToArray(),
					attaches.ToArray());
			}

			private static ChunkResult ConvertWeighted(WeightedMesh wba)
			{

				List<IndexedWeightVertex> singleWeights = new();
				List<IndexedWeightVertex> multiWeights = new();

				for(int i = 0; i < wba.Vertices.Length; i++)
				{
					WeightedVertex vtx = wba.Vertices[i];
					int weightCount = vtx.GetWeightCount();
					if(weightCount == 0)
					{
						throw new InvalidDataException("Vertex has no specified weights");
					}
					else if(weightCount == 1)
					{
						singleWeights.Add(new(i, vtx));
					}
					else
					{
						multiWeights.Add(new(i, vtx));
					}
				}

				singleWeights = singleWeights.OrderBy(x => x.vertex.GetFirstWeightIndex()).ThenBy(x => x.index).ToList();
				int multiWeightOffset = singleWeights.Count;

				int[] firstWeightIndices = multiWeights.Select(x => x.vertex.GetFirstWeightIndex()).ToArray();
				int[] lastWeightIndices = multiWeights.Select(x => x.vertex.GetLastWeightIndex()).ToArray();

				// grouping the vertices together by node
				List<(int nodeIndex, VertexChunk[] chunks)> vertexChunks = new();

				foreach(int nodeIndex in wba.DependingNodeIndices.Order())
				{
					List<VertexChunk> chunks = new();

					// find out if any singleWeights belong to the node index
					int singleWeightIndexOffset = 0;
					List<ChunkVertex> singleWeightVerts = new();
					for(int i = 0; i < singleWeights.Count; i++)
					{
						WeightedVertex vert = singleWeights[i].vertex;
						bool contains = vert.Weights![nodeIndex] > 0f;
						if(contains)
						{
							if(singleWeightVerts.Count == 0)
							{
								singleWeightIndexOffset = i;
							}

							Vector3 pos = new(vert.Position.X, vert.Position.Y, vert.Position.Z);
							singleWeightVerts.Add(new(pos, vert.Normal, (ushort)i, 1f));
						}

						if(!contains && singleWeightVerts.Count > 0)
						{
							break;
						}
					}

					if(singleWeightVerts.Count > 0)
					{
						chunks.Add(
							new VertexChunk(
								VertexChunkType.Normal,
								WeightStatus.Start,
								(ushort)singleWeightIndexOffset,
								singleWeightVerts.ToArray()));
					}

					// now the ones with weights. we differentiate between
					// those that initiate and those that continue
					List<ChunkVertex> initWeightsVerts = new();
					List<ChunkVertex> continueWeightsVerts = new();
					List<ChunkVertex> endWeightsVerts = new();

					for(int i = 0; i < multiWeights.Count; i++)
					{
						WeightedVertex vert = multiWeights[i].vertex;

						float weight = vert.Weights![nodeIndex];
						if(weight == 0f)
						{
							continue;
						}

						ChunkVertex chunkVert = new(
							vert.Position,
							vert.Normal,
							(ushort)(i + multiWeightOffset),
							weight);

						if(firstWeightIndices[i] == nodeIndex)
						{
							initWeightsVerts.Add(chunkVert);
						}
						else if(lastWeightIndices[i] == nodeIndex)
						{
							endWeightsVerts.Add(chunkVert);
						}
						else
						{
							continueWeightsVerts.Add(chunkVert);
						}
					}

					if(initWeightsVerts.Count > 0)
					{
						chunks.Add(
							new VertexChunk(
								VertexChunkType.NormalAttributes,
								WeightStatus.Start, 0,
								initWeightsVerts.ToArray()));
					}

					if(continueWeightsVerts.Count > 0)
					{
						chunks.Add(
							new VertexChunk(
								VertexChunkType.NormalAttributes,
								WeightStatus.Middle, 0,
								continueWeightsVerts.ToArray()));
					}

					if(endWeightsVerts.Count > 0)
					{
						chunks.Add(
							new VertexChunk(
								VertexChunkType.NormalAttributes,
								WeightStatus.End, 0,
								endWeightsVerts.ToArray()));
					}

					vertexChunks.Add((nodeIndex, chunks.ToArray()));
				}

				// mapping the indices for the polygons
				ushort[] indexMap = new ushort[wba.Vertices.Length];
				for(int i = 0; i < singleWeights.Count; i++)
				{
					indexMap[singleWeights[i].index] = (ushort)i;
				}

				for(int i = 0; i < multiWeights.Count; i++)
				{
					indexMap[multiWeights[i].index] = (ushort)(i + multiWeightOffset);
				}

				// assemble the polygon chunks
				List<PolyChunk> polyChunks = new();
				for(int i = 0; i < wba.TriangleSets.Length; i++)
				{
					// mapping the triangles to the chunk format
					BufferCorner[] bufferCorners = wba.TriangleSets[i];
					ChunkCorner[] corners = new ChunkCorner[bufferCorners.Length];
					for(int j = 0; j < bufferCorners.Length; j++)
					{
						BufferCorner bc = bufferCorners[j];
						corners[j] = ChunkCorner.DefaultValues;
						corners[j].Index = indexMap[bc.VertexIndex];
						corners[j].Texcoord = bc.Texcoord;
					}

					polyChunks.AddRange(CreateStripChunk(corners, wba.Materials[i], wba.WriteSpecular));
				}

				// assemble the attaches
				List<int> nodeAttachIndices = new();
				List<ChunkAttach> attaches = new();

				for(int i = 0; i < vertexChunks.Count - 1; i++)
				{
					(int nodeIndex, VertexChunk[] chunks) = vertexChunks[i];
					nodeAttachIndices.Add(nodeIndex);
					attaches.Add(new(chunks, null));
				}

				(int lastNodeindex, VertexChunk[] lastVertexChunk) = vertexChunks[^1];
				nodeAttachIndices.Add(lastNodeindex);
				attaches.Add(new(lastVertexChunk, polyChunks.ToArray()));

				return new(
					wba.Label ?? "CHUNK_" + StringExtensions.GenerateIdentifier(),
					wba.Vertices.Length,
					true,
					nodeAttachIndices.ToArray(),
					attaches.ToArray());
			}

			protected override ChunkResult ConvertWeighted(WeightedMesh wba, bool optimize)
			{
				bool binaryWeighted = wba.HasColors;
				if(binaryWeighted && !wba.ForceVertexColors)
				{
					foreach(WeightedVertex vertex in wba.Vertices)
					{
						if(vertex.IsWeighted())
						{
							binaryWeighted = false;
							break;
						}
					}
				}

				if(binaryWeighted)
				{
					return ConvertWeightedBinaryColored(wba);
				}
				else
				{
					return ConvertWeighted(wba);
				}
			}

			protected override ChunkResult ConvertWeightless(WeightedMesh wba, bool optimize)
			{
				ChunkVertex[] vertices;
				ChunkCorner[][] cornerSets = new ChunkCorner[wba.TriangleSets.Length][];

				VertexChunkType type;
				if(wba.HasColors)
				{
					type = VertexChunkType.Diffuse;
					List<ChunkVertex> colorVertices = new();
					for(int i = 0; i < wba.TriangleSets.Length; i++)
					{
						BufferCorner[] bufferCorners = wba.TriangleSets[i];
						ChunkCorner[] corners = new ChunkCorner[bufferCorners.Length];
						for(int j = 0; j < bufferCorners.Length; j++)
						{
							BufferCorner bc = bufferCorners[j];
							corners[j] = ChunkCorner.DefaultValues;
							corners[j].Index = (ushort)colorVertices.Count;
							corners[j].Texcoord = bc.Texcoord;

							WeightedVertex vertex = wba.Vertices[bc.VertexIndex];
							colorVertices.Add(new(vertex.Position, bc.Color, Color.ColorWhite));
						}

						cornerSets[i] = corners;
					}

					// first, get rid of all duplicate vertices
					if(colorVertices.TryCreateDistinctMap(out DistinctMap<ChunkVertex> distinctVerts))
					{
						for(int i = 0; i < cornerSets.Length; i++)
						{
							ChunkCorner[] corners = cornerSets[i];
							for(int j = 0; j < corners.Length; j++)
							{
								corners[j].Index = distinctVerts[corners[j].Index];
							}
						}
					}

					vertices = distinctVerts.ValueArray;
				}
				else
				{
					type = VertexChunkType.Normal;
					vertices = new ChunkVertex[wba.Vertices.Length];
					// converting the vertices 1:1, with normal information
					for(int i = 0; i < wba.Vertices.Length; i++)
					{
						WeightedVertex vert = wba.Vertices[i];
						Vector3 position = new(vert.Position.X, vert.Position.Y, vert.Position.Z);
						vertices[i] = new(position, vert.Normal);
					}

					for(int i = 0; i < wba.TriangleSets.Length; i++)
					{
						BufferCorner[] bufferCorners = wba.TriangleSets[i];
						ChunkCorner[] corners = new ChunkCorner[bufferCorners.Length];
						for(int j = 0; j < bufferCorners.Length; j++)
						{
							BufferCorner bc = bufferCorners[j];
							corners[j] = ChunkCorner.DefaultValues;
							corners[j].Index = bc.VertexIndex;
							corners[j].Texcoord = bc.Texcoord;
						}

						cornerSets[i] = corners;
					}
				}

				VertexChunk vtxChunk = new(type, WeightStatus.Start, 0, vertices);
				List<PolyChunk> polyChunks = new();
				for(int i = 0; i < cornerSets.Length; i++)
				{
					polyChunks.AddRange(CreateStripChunk(cornerSets[i], wba.Materials[i], wba.WriteSpecular));
				}

				return new(
					wba.Label ?? "CHUNK_" + StringExtensions.GenerateIdentifier(),
					vertices.Length,
					false,
					wba.RootIndices.ToArray(),
					new[] {
					new ChunkAttach(
						new[] { vtxChunk },
						polyChunks.ToArray())
					});
			}

			private static PolyChunk[] CreateStripChunk(ChunkCorner[] corners, BufferMaterial material, bool writeSpecular)
			{
				ChunkCorner[][] stripCorners = TriangleStrippifier.Global.StrippifyNoDegen(corners, out bool[] reversed);
				ChunkStrip[] strips = new ChunkStrip[stripCorners.Length];

				for(int i = 0; i < strips.Length; i++)
				{
					strips[i] = new(stripCorners[i], reversed[i]);
				}

				bool hasUV = material.UseTexture && !material.NormalMapping;
				PolyChunkType stripType = hasUV ? PolyChunkType.Strip_Tex : PolyChunkType.Strip_Blank;

				StripChunk stripchunk = new(stripType, strips, 0)
				{
					FlatShading = material.Flat,
					IgnoreAmbient = material.NoAmbient,
					IgnoreLight = material.NoLighting,
					IgnoreSpecular = material.NoSpecular,
					EnvironmentMapping = material.NormalMapping,
					UseAlpha = material.UseAlpha,
					DoubleSide = !material.BackfaceCulling
				};

				TextureChunk textureChunk = new()
				{
					ClampU = material.ClampU,
					ClampV = material.ClampV,
					MirrorU = material.MirrorU,
					MirrorV = material.MirrorV,
					FilterMode = material.TextureFiltering,
					SuperSample = material.AnisotropicFiltering,
					TextureID = (ushort)material.TextureIndex
				};

				MaterialChunk materialChunk = new()
				{
					SourceAlpha = material.SourceBlendMode,
					DestinationAlpha = material.DestinationBlendmode,
					Diffuse = material.Diffuse,
					Ambient = material.Ambient,
				};

				if(writeSpecular)
				{
					materialChunk.Specular = material.Specular;
					materialChunk.SpecularExponent = (byte)material.SpecularExponent;
				}

				return new PolyChunk[] { materialChunk, textureChunk, stripchunk };
			}

			protected override void CorrectSpace(Attach attach, Matrix4x4 vertexMatrix, Matrix4x4 normalMatrix)
			{
				ChunkAttach chunkAttach = (ChunkAttach)attach;
				if(chunkAttach.VertexChunks == null)
				{
					return;
				}

				foreach(VertexChunk? vtxChunk in chunkAttach.VertexChunks)
				{
					if(vtxChunk == null)
					{
						continue;
					}

					for(int j = 0; j < vtxChunk.Vertices.Length; j++)
					{
						ChunkVertex vertex = vtxChunk.Vertices[j];

						vertex.Position = Vector3.Transform(vertex.Position, vertexMatrix);
						vertex.Normal = Vector3.TransformNormal(vertex.Normal, normalMatrix);

						vtxChunk.Vertices[j] = vertex;
					}
				}
			}

			protected override ChunkResult WeightedClone(string label, int vertexCount, int[] attachIndices, Attach[] attaches)
			{
				return new(
					label,
					vertexCount,
					true,
					attachIndices,
					attaches.Cast<ChunkAttach>().ToArray());
			}

			protected override Attach CombineAttaches(List<Attach> attaches, string label)
			{
				List<VertexChunk?> vertexChunks = new();
				List<PolyChunk?> polyChunks = new();

				foreach(ChunkAttach atc in attaches.Cast<ChunkAttach>())
				{
					if(atc.VertexChunks != null)
					{
						vertexChunks.AddRange(atc.VertexChunks);
					}

					if(atc.PolyChunks != null)
					{
						polyChunks.AddRange(atc.PolyChunks);
					}
				}

				return new ChunkAttach(vertexChunks.ToArray(), polyChunks.ToArray()) { Label = label };
			}
		}

		public static void ConvertWeightedToChunk(Node model, WeightedMesh[] meshData, bool optimize)
		{
			new OffsettableChunkConverter().Convert(model, meshData, optimize);
		}

		#region Convert to Buffer

		private static BufferCorner[] ConvertStripChunk(StripChunk chunk, ChunkVertex[] vertexCache)
		{
			bool hasColor = chunk.HasColors;

			BufferCorner[][] bufferStrips = new BufferCorner[chunk.Strips.Length][];
			bool[] reversed = new bool[bufferStrips.Length];

			for(int i = 0; i < chunk.Strips.Length; i++)
			{
				ChunkStrip strip = chunk.Strips[i];
				BufferCorner[] bufferStrip = new BufferCorner[strip.Corners.Length];

				for(int j = 0; j < strip.Corners.Length; j++)
				{
					ChunkCorner corner = strip.Corners[j];

					Color color = hasColor
						? corner.Color
						: vertexCache[corner.Index].Diffuse;

					bufferStrip[j] = new(corner.Index, color, corner.Texcoord);
				}

				bufferStrips[i] = bufferStrip;
				reversed[i] = strip.Reversed;
			}

			return TriangleStrippifier.JoinStrips(bufferStrips, reversed);
		}

		public static void BufferChunkModel(Node model, bool optimize)
		{
			BufferMaterial material = BufferMaterial.DefaultValues;

			ChunkVertex[] vertexCache = new ChunkVertex[0x10000];
			Dictionary<ChunkAttach, PolyChunk?[]> activeChunks = GetActivePolyChunks(model);

			foreach(ChunkAttach atc in model.GetTreeAttachEnumerable().OfType<ChunkAttach>())
			{
				List<BufferMesh> meshes = new();

				BufferVertex[]? vertices = null;
				bool continueWeight = false;
				bool hasVertexNormals = false;
				bool hasVertexColors = false;
				ushort vertexWriteOffset = 0;

				if(atc.VertexChunks != null)
				{
					for(int i = 0; i < atc.VertexChunks.Length; i++)
					{
						VertexChunk? cnk = atc.VertexChunks[i];

						if(cnk == null)
						{
							continue;
						}

						List<BufferVertex> vertexList = new();
						if(!cnk.HasWeight)
						{
							for(int j = 0; j < cnk.Vertices.Length; j++)
							{
								ChunkVertex vtx = cnk.Vertices[j];
								vertexCache[j + cnk.IndexOffset] = vtx;
								vertexList.Add(new BufferVertex(vtx.Position, vtx.Normal, (ushort)j));
							}
						}
						else
						{

							for(int j = 0; j < cnk.Vertices.Length; j++)
							{
								ChunkVertex vtx = cnk.Vertices[j];
								vertexCache[vtx.Index + cnk.IndexOffset] = vtx;
								vertexList.Add(new BufferVertex(vtx.Position, vtx.Normal, vtx.Index, vtx.Weight));
							}
						}

						vertices = vertexList.ToArray();
						continueWeight = cnk.WeightStatus != WeightStatus.Start;
						hasVertexNormals = cnk.HasNormals;
						hasVertexColors |= cnk.HasDiffuseColors;
						vertexWriteOffset = cnk.IndexOffset;

						// if not last
						if(i < atc.VertexChunks.Length - 1)
						{
							meshes.Add(new BufferMesh(vertices, continueWeight, hasVertexNormals, vertexWriteOffset));
						}
					}
				}

				if(activeChunks.TryGetValue(atc, out PolyChunk?[]? polyChunks))
				{
					foreach(PolyChunk? chunk in polyChunks)
					{
						switch(chunk)
						{
							case BlendAlphaChunk blendAlphaChunk:
								material.SourceBlendMode = blendAlphaChunk.SourceAlpha;
								material.DestinationBlendmode = blendAlphaChunk.DestinationAlpha;
								break;
							case MipmapDistanceMultiplierChunk mmdmChunk:
								material.MipmapDistanceMultiplier = mmdmChunk.MipmapDistanceMultiplier;
								break;
							case SpecularExponentChunk specularExponentChunk:
								material.SpecularExponent = specularExponentChunk.SpecularExponent;
								break;
							case TextureChunk textureChunk:
								material.TextureIndex = textureChunk.TextureID;
								material.MirrorU = textureChunk.MirrorU;
								material.MirrorV = textureChunk.MirrorV;
								material.ClampU = textureChunk.ClampU;
								material.ClampV = textureChunk.ClampV;
								material.AnisotropicFiltering = textureChunk.SuperSample;
								material.TextureFiltering = textureChunk.FilterMode;
								break;
							case MaterialChunk materialChunk:
								material.SourceBlendMode = materialChunk.SourceAlpha;
								material.DestinationBlendmode = materialChunk.DestinationAlpha;

								if(materialChunk.Diffuse.HasValue)
								{
									material.Diffuse = materialChunk.Diffuse.Value;
								}

								if(materialChunk.Ambient.HasValue)
								{
									material.Ambient = materialChunk.Ambient.Value;
								}

								if(materialChunk.Specular.HasValue)
								{
									material.Specular = materialChunk.Specular.Value;
									material.SpecularExponent = materialChunk.SpecularExponent;
								}

								break;
							case StripChunk stripChunk:
								material.Flat = stripChunk.FlatShading;
								material.NoAmbient = stripChunk.IgnoreAmbient;
								material.NoLighting = stripChunk.IgnoreLight;
								material.NoSpecular = stripChunk.IgnoreSpecular;
								material.NormalMapping = stripChunk.EnvironmentMapping;
								material.UseTexture = stripChunk.TexcoordCount > 0 || stripChunk.EnvironmentMapping;
								material.UseAlpha = stripChunk.UseAlpha;
								material.BackfaceCulling = !stripChunk.DoubleSide;

								BufferCorner[] corners = ConvertStripChunk(stripChunk, vertexCache);

								bool hasColor = stripChunk.HasColors || hasVertexColors;

								if(corners.Length > 0)
								{
									if(vertices != null)
									{
										meshes.Add(new BufferMesh(vertices, material, corners, null, true, continueWeight, hasVertexNormals, hasColor, vertexWriteOffset, 0));
										vertices = null;
									}
									else
									{
										meshes.Add(new BufferMesh(material, corners, null, true, hasColor, 0));
									}
								}

								break;
							default:
								break;
						}
					}

				}

				if(vertices != null)
				{
					meshes.Add(new BufferMesh(vertices, continueWeight, hasVertexNormals, vertexWriteOffset));
				}

				atc.MeshData = optimize ? BufferMesh.Optimize(meshes) : meshes.ToArray();
			}
		}

		#endregion

		public static Dictionary<ChunkAttach, PolyChunk?[]> GetActivePolyChunks(Node model)
		{
			Dictionary<ChunkAttach, PolyChunk?[]> result = new();
			List<PolyChunk?>[] polyChunkCache = Array.Empty<List<PolyChunk?>>();

			foreach(ChunkAttach attach in model.GetTreeAttachEnumerable().OfType<ChunkAttach>())
			{
				if(attach.PolyChunks == null)
				{
					continue;
				}

				List<PolyChunk?> active = new();

				int cacheID = -1;
				foreach(PolyChunk? polyChunk in attach.PolyChunks)
				{
					switch(polyChunk)
					{
						case CacheListChunk cache:
							cacheID = cache.List;

							if(polyChunkCache.Length <= cacheID)
							{
								Array.Resize(ref polyChunkCache, cacheID + 1);
							}

							polyChunkCache[cacheID] = new List<PolyChunk?>();
							break;
						case DrawListChunk draw:
							active.AddRange(polyChunkCache[draw.List]);
							break;
						default:
							if(cacheID > -1)
							{
								polyChunkCache[cacheID].Add(polyChunk);
							}
							else
							{
								active.Add(polyChunk);
							}

							break;
					}

				}


				if(active.Count > 0)
				{
					result.Add(attach, active.ToArray());
				}
			}

			return result;
		}
	}
}
