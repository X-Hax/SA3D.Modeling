using SA3D.Common;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Mesh.Gamecube;
using SA3D.Modeling.Mesh.Gamecube.Enums;
using SA3D.Modeling.Mesh.Gamecube.Parameters;
using SA3D.Modeling.Mesh.Weighted;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Converters
{
	/// <summary>
	/// Provides buffer conversion methods for GC
	/// </summary>
	internal static class GCConverter
	{
		public static void ConvertWeightedToGC(Node model, WeightedMesh[] meshData, bool optimize)
		{
			Node[] nodes = model.GetTreeNodes();
			GCAttach[] attaches = new GCAttach[nodes.Length];
			List<GCAttach> rootlessAttaches = new();

			meshData = WeightedMesh.MergeAtRoots(meshData);

			foreach(WeightedMesh weightedMesh in meshData)
			{
				Vector3[] positionData = new Vector3[weightedMesh.Vertices.Length];
				Vector3[] normalData = new Vector3[positionData.Length];

				for(int i = 0; i < positionData.Length; i++)
				{
					WeightedVertex vtx = weightedMesh.Vertices[i];

					positionData[i] = vtx.Position;
					normalData[i] = vtx.Normal;
				}

				// getting the corner information
				int cornerCount = 0;
				for(int i = 0; i < weightedMesh.TriangleSets.Length; i++)
				{
					cornerCount += weightedMesh.TriangleSets[i].Length;
				}

				float uvFac = 1 << byte.Clamp(weightedMesh.TexcoordPrecisionLevel, 0, 7);
				Vector2[] texcoordData = new Vector2[cornerCount];
				Color[] colorData = new Color[cornerCount];
				GCCorner[][] polygonData = new GCCorner[weightedMesh.TriangleSets.Length][];

				ushort cornerIndex = 0;
				for(int i = 0; i < polygonData.Length; i++)
				{
					BufferCorner[] bufferCorners = weightedMesh.TriangleSets[i];
					GCCorner[] meshCorners = new GCCorner[bufferCorners.Length];
					for(int j = 0; j < bufferCorners.Length; j++)
					{
						BufferCorner bcorner = bufferCorners[j];

						texcoordData[cornerIndex] = bcorner.Texcoord * uvFac;
						colorData[cornerIndex] = bcorner.Color;

						meshCorners[j] = new GCCorner()
						{
							PositionIndex = bcorner.VertexIndex,
							NormalIndex = bcorner.VertexIndex,
							TexCoord0Index = cornerIndex,
							Color0Index = cornerIndex
						};

						cornerIndex++;
					}

					// correcting the culling order
					for(int j = 0; j < meshCorners.Length; j += 3)
					{
						(meshCorners[j], meshCorners[j + 1]) = (meshCorners[j + 1], meshCorners[j]);
					}

					polygonData[i] = meshCorners;
				}

				bool hasUVs = texcoordData.Any(x => x != default);

				// Puttin together the vertex sets
				GCVertexSet[] vertexData = new GCVertexSet[2 + (hasUVs ? 1 : 0)];

				vertexData[0] = GCVertexSet.CreatePositionSet(positionData);

				vertexData[1] = weightedMesh.HasColors
					? GCVertexSet.CreateColor0Set(colorData)
					: GCVertexSet.CreateNormalSet(normalData);

				if(hasUVs)
				{
					vertexData[2] = GCVertexSet.CreateTexcoord0Set(texcoordData);
				}

				// stitching polygons together
				GCMesh ProcessBufferMesh(GCCorner[] corners, BufferMaterial material, ref BufferMaterial activeMaterial, ref bool first)
				{
					// generating parameter info
					List<IGCParameter> parameters = new();

					if(first)
					{
						GCIndexFormat indexFormat = default;

						foreach(GCVertexSet set in vertexData)
						{
							GCVertexFormatParameter param = new()
							{
								VertexType = set.Type,
								VertexStructType = set.StructType,
								VertexDataType = set.DataType
							};

							if(param.VertexType is >= GCVertexType.TexCoord0 and <= GCVertexType.TexCoord7)
							{
								param.Attributes = byte.Min(7, weightedMesh.TexcoordPrecisionLevel);
							}

							parameters.Add(param);

							uint flag = 1u << ((int)set.Type * 2);

							// Mark as use vertex data
							indexFormat |= (GCIndexFormat)(flag << 1);

							if(set.DataLength > 256)
							{
								// mark as large index
								indexFormat |= (GCIndexFormat)flag;
							}
						}

						parameters.Add(new GCIndexFormatParameter() { IndexFormat = indexFormat });

						activeMaterial = material;

						parameters.Add(new GCLightingParameter()
						{
							LightingAttributes = weightedMesh.HasColors
								? GCLightingParameter.DefaultColorParam
								: GCLightingParameter.DefaultNormalParam,
							ShadowStencil = material.GCShadowStencil
						});

					}

					if(first || activeMaterial.GCShadowStencil != material.GCShadowStencil)
					{
						parameters.Add(new GCLightingParameter()
						{
							LightingAttributes = weightedMesh.HasColors
								? GCLightingParameter.DefaultColorParam
								: GCLightingParameter.DefaultNormalParam,
							ShadowStencil = material.GCShadowStencil
						});
					}

					if(first
						|| activeMaterial.UseAlpha != material.UseAlpha
						|| activeMaterial.SourceBlendMode != material.SourceBlendMode
						|| activeMaterial.DestinationBlendmode != material.DestinationBlendmode)
					{
						parameters.Add(new GCBlendAlphaParameter()
						{
							SourceAlpha = material.SourceBlendMode,
							DestinationAlpha = material.DestinationBlendmode,
							UseAlpha = material.UseAlpha
						});
					}

					if(first || activeMaterial.Ambient != material.Ambient)
					{
						parameters.Add(new GCAmbientColorParameter()
						{
							AmbientColor = material.Ambient
						});
					}

					if(first
						|| activeMaterial.TextureIndex != material.TextureIndex
						|| activeMaterial.MirrorU != material.MirrorU
						|| activeMaterial.MirrorV != material.MirrorV
						|| activeMaterial.ClampU != material.ClampU
						|| activeMaterial.ClampV != material.ClampV)
					{
						GCTextureParameter texParam = new()
						{
							TextureID = (ushort)material.TextureIndex
						};

						if(!material.ClampU)
						{
							texParam.Tiling |= GCTileMode.RepeatU;
						}

						if(!material.ClampV)
						{
							texParam.Tiling |= GCTileMode.RepeatV;
						}

						if(material.MirrorU)
						{
							texParam.Tiling |= GCTileMode.MirrorU;
						}

						if(material.MirrorV)
						{
							texParam.Tiling |= GCTileMode.MirrorV;
						}

						parameters.Add(texParam);
					}

					if(first)
					{
						parameters.Add(GCUnknownParameter.DefaultValues);
					}

					if(first
						|| activeMaterial.GCTexCoordID != material.GCTexCoordID
						|| activeMaterial.GCTexCoordType != material.GCTexCoordType
						|| activeMaterial.GCTexCoordSource != material.GCTexCoordSource
						|| activeMaterial.GCMatrixID != material.GCMatrixID)
					{
						parameters.Add(new GCTexCoordParameter()
						{
							TexCoordID = material.GCTexCoordID,
							TexCoordType = material.GCTexCoordType,
							TexCoordSource = material.GCTexCoordSource,
							MatrixID = material.GCMatrixID
						});
					}

					activeMaterial = material;
					first = false;

					List<GCPolygon> polygons = new();

					// note: a single triangle polygon can only carry 0xFFFF corners, so about 22k tris
					if(corners.Length > 0xFFFF)
					{
						int remainingLength = corners.Length;
						int offset = 0;
						while(remainingLength > 0)
						{
							GCCorner[] finalCorners = new GCCorner[Math.Max(0xFFFF, remainingLength)];
							Array.Copy(corners, offset, finalCorners, 0, finalCorners.Length);
							offset += finalCorners.Length;
							remainingLength -= finalCorners.Length;

							GCPolygon triangle = new(GCPolyType.Triangles, finalCorners);
							polygons.Add(triangle);
						}
					}
					else
					{
						polygons.Add(new(GCPolyType.Triangles, corners));
					}

					return new GCMesh(parameters.ToArray(), polygons.ToArray());
				}

				List<int> opaqueMeshIndices = new();
				List<int> translucentMeshIndices = new();

				for(int i = 0; i < weightedMesh.Materials.Length; i++)
				{
					(weightedMesh.Materials[i].UseAlpha ? translucentMeshIndices : opaqueMeshIndices).Add(i);
				}

				GCMesh[] ProcessBufferMeshes(List<int> meshIndices)
				{
					BufferMaterial currentMaterial = default;
					List<GCMesh> result = new();
					bool first = true;
					foreach(int index in meshIndices)
					{
						result.Add(
							ProcessBufferMesh(
								polygonData[index],
								weightedMesh.Materials[index],
								ref currentMaterial,
								ref first));
					}

					return result.ToArray();
				}

				GCMesh[] opaqueMeshes = ProcessBufferMeshes(opaqueMeshIndices);
				GCMesh[] translucentMeshes = ProcessBufferMeshes(translucentMeshIndices);

				GCAttach result = new(vertexData, opaqueMeshes, translucentMeshes)
				{
					Label = weightedMesh.Label ?? "GC_" + StringExtensions.GenerateIdentifier()
				};

				if(optimize)
				{
					result.OptimizeVertexData();
					result.OptimizePolygons();
				}

				result.RecalculateBounds();

				foreach(int index in weightedMesh.RootIndices)
				{
					attaches[index] = result;
				}
			}

			model.ClearAttachesFromTree();
			model.ClearWeldingsFromTree();

			// Linking the attaches to the nodes
			for(int i = 0; i < nodes.Length; i++)
			{
				nodes[i].Attach = attaches[i];
			}
		}

		public static BufferMesh[] ConvertGCToBuffer(GCAttach attach, bool optimize)
		{
			List<BufferMesh> meshes = new();

			DistinctMap<Vector3>? positions = null;
			DistinctMap<Vector3>? normals = null;
			DistinctMap<Color>? colors = null;
			DistinctMap<Vector2>? uvs = null;


			if(attach.VertexData.TryGetValue(GCVertexType.Position, out GCVertexSet? tmp))
			{
				if(!optimize || !tmp.Vector3Data.TryCreateDistinctMap(out positions))
				{
					positions = new(tmp.Vector3Data, null);
				}
			}

			if(attach.VertexData.TryGetValue(GCVertexType.Normal, out tmp))
			{
				if(!optimize || !tmp.Vector3Data.TryCreateDistinctMap(out normals))
				{
					normals = new(tmp.Vector3Data, null);
				}
			}

			if(attach.VertexData.TryGetValue(GCVertexType.TexCoord0, out tmp))
			{
				if(!optimize || !tmp.Vector2Data.TryCreateDistinctMap(out uvs))
				{
					uvs = new(tmp.Vector2Data, null);
				}
			}

			if(attach.VertexData.TryGetValue(GCVertexType.Color0, out tmp))
			{
				if(!optimize || !tmp.ColorData.TryCreateDistinctMap(out colors))
				{
					colors = new(tmp.ColorData, null);
				}
			}

			if(positions == null)
			{
				throw new NullReferenceException("Mandatory positions dont exit");
			}


			float uvFac = 1;

			List<BufferVertex> bufferVertices = new();

			// if there are no normals, then we can already initialize the entire thing with all positions
			Func<ushort, ushort, ushort> getVertexIndex;
			if(normals == null)
			{
				for(ushort i = 0; i < positions.Values.Count; i++)
				{
					bufferVertices.Add(new BufferVertex(positions.Values[i], i));
				}

				getVertexIndex = (pos, nrm) => positions[pos];
			}
			else
			{
				Dictionary<uint, ushort> vertexIndices = new();
				getVertexIndex = (pos, nrm) =>
				{
					pos = positions[pos];
					nrm = normals[nrm];

					uint posnrmIndex = pos | ((uint)nrm << 16);

					if(!vertexIndices.TryGetValue(posnrmIndex, out ushort vtxIndex))
					{
						vtxIndex = (ushort)bufferVertices.Count;
						bufferVertices.Add(new(positions.Values[pos], normals.Values[nrm], vtxIndex));
						vertexIndices.Add(posnrmIndex, vtxIndex);
					}

					return vtxIndex;
				};
			}

			BufferMaterial material;

			BufferMesh ProcessMesh(GCMesh m)
			{
				// setting the material properties according to the parameters
				foreach(IGCParameter param in m.Parameters)
				{
					switch(param)
					{
						case GCVertexFormatParameter vertexFormatParam:
							if(vertexFormatParam.VertexType == GCVertexType.TexCoord0
								&& (vertexFormatParam.Attributes & 0xF0) == 0)
							{
								uvFac = 1 << (vertexFormatParam.Attributes & 0x7);
								if((vertexFormatParam.Attributes & 0x8) > 0)
								{
									uvFac = 1 / uvFac;
								}
							}

							break;
						case GCBlendAlphaParameter blendAlphaParam:
							material.SourceBlendMode = blendAlphaParam.SourceAlpha;
							material.DestinationBlendmode = blendAlphaParam.DestinationAlpha;
							material.UseAlpha = blendAlphaParam.UseAlpha;
							break;

						case GCAmbientColorParameter ambientColorParam:
							material.Ambient = ambientColorParam.AmbientColor;
							break;

						case GCDiffuseColorParameter diffuseColorParam:
							material.Ambient = diffuseColorParam.DiffuseColor;
							break;

						case GCSpecularColorParameter specularColorParam:
							material.Ambient = specularColorParam.SpecularColor;
							break;

						case GCTextureParameter textureParam:
							material.UseTexture = true;
							material.TextureIndex = textureParam.TextureID;
							material.ClampU = !textureParam.Tiling.HasFlag(GCTileMode.RepeatU);
							material.ClampV = !textureParam.Tiling.HasFlag(GCTileMode.RepeatV);
							material.MirrorU = textureParam.Tiling.HasFlag(GCTileMode.MirrorU);
							material.MirrorV = textureParam.Tiling.HasFlag(GCTileMode.MirrorV);
							break;

						case GCTexCoordParameter texcoordParam:
							material.NormalMapping =
								texcoordParam.TexCoordSource ==
								GCTexCoordSource.Normal;

							material.GCMatrixID = texcoordParam.MatrixID;
							material.GCTexCoordID = texcoordParam.TexCoordID;
							material.GCTexCoordSource = texcoordParam.TexCoordSource;
							material.GCTexCoordType = texcoordParam.TexCoordType;
							break;

						default:
							break;
					}
				}

				// filtering out the double loops
				List<BufferCorner> corners = new();
				List<uint> trianglelist = new();

				foreach(GCPolygon p in m.Polygons)
				{
					// inverted culling is done manually in the gc strips, so we have to account for that
					bool rev = p.Type != GCPolyType.TriangleStrip || p.Corners[0].PositionIndex != p.Corners[1].PositionIndex;
					int offset = rev ? 0 : 1;
					uint[] indices = new uint[p.Corners.Length - offset];

					for(int i = offset; i < p.Corners.Length; i++)
					{
						GCCorner c = p.Corners[i];
						indices[i - offset] = (uint)corners.Count;
						ushort vertexIndex = getVertexIndex(c.PositionIndex, c.NormalIndex);

						Vector2 uv = uvs?.GetValue(c.TexCoord0Index) ?? default;
						uv *= uvFac;
						Color color = colors?.GetValue(c.Color0Index) ?? Color.ColorWhite;
						corners.Add(new BufferCorner(vertexIndex, color, uv));
					}

					// converting indices to triangles
					if(p.Type == GCPolyType.Triangles)
					{
						for(int i = 0; i < indices.Length; i += 3)
						{
							(indices[i + 1], indices[i]) = (indices[i], indices[i + 1]);
						}

						trianglelist.AddRange(indices);

					}
					else if(p.Type == GCPolyType.TriangleStrip)
					{
						uint[] newIndices = new uint[(indices.Length - 2) * 3];
						for(int i = 2, triangleIndex = 0; i < indices.Length; i++, triangleIndex += 3)
						{
							if(!rev)
							{
								newIndices[triangleIndex] = indices[i - 2];
								newIndices[triangleIndex + 1] = indices[i - 1];
							}
							else
							{
								newIndices[triangleIndex] = indices[i - 1];
								newIndices[triangleIndex + 1] = indices[i - 2];
							}

							newIndices[triangleIndex + 2] = indices[i];
							rev = !rev;
						}

						trianglelist.AddRange(newIndices);
					}
					else
					{
						throw new Exception($"Primitive type {p.Type} not a valid triangle format");
					}
				}

				BufferMesh mesh = new(material, corners.ToArray(), trianglelist.ToArray(), false, colors != null, 0);

				if(optimize)
				{
					mesh.OptimizePolygons();
				}

				return mesh;
			}

			material = new(BufferMaterial.DefaultValues)
			{
				NoLighting = colors != null
			};

			foreach(GCMesh m in attach.OpaqueMeshes)
			{
				meshes.Add(ProcessMesh(m));
			}

			material = new(BufferMaterial.DefaultValues)
			{
				NoLighting = colors != null
			};

			foreach(GCMesh m in attach.TransparentMeshes)
			{
				meshes.Add(ProcessMesh(m));
			}

			// inject the vertex information into the first mesh
			BufferMesh vtxMesh = meshes[0];

			meshes[0] = new(
				bufferVertices.ToArray(),
				vtxMesh.Material,
				vtxMesh.Corners,
				vtxMesh.IndexList,
				vtxMesh.Strippified,
				false,
				normals != null,
				vtxMesh.HasColors,
				0, 0);

			return BufferMesh.CompressLayout(meshes);
		}

		public static void BufferGCModel(Node model, bool optimize)
		{
			foreach(GCAttach attach in model.GetTreeAttachEnumerable().OfType<GCAttach>())
			{
				attach.MeshData = ConvertGCToBuffer(attach, optimize);
			}
		}
	}
}
