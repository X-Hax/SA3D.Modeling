using SA3D.Common;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Basic;
using SA3D.Modeling.Mesh.Basic.Polygon;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Mesh.Weighted;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Strippify;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Converters
{
	/// <summary>
	/// Provides buffer conversion methods for BASIC
	/// </summary>
	internal static class BasicConverter
	{

		#region Convert To Basic

		private static BasicMaterial ConvertToBasicMaterial(BufferMaterial mat)
		{
			return new(BasicMaterial.DefaultValues)
			{
				DiffuseColor = mat.Diffuse,
				SpecularColor = mat.Specular,
				SpecularExponent = mat.SpecularExponent,
				TextureID = mat.TextureIndex,
				FilterMode = mat.TextureFiltering,
				MipmapDistanceMultiplier = mat.MipmapDistanceMultiplier,
				SuperSample = mat.AnisotropicFiltering,
				ClampU = mat.ClampU,
				ClampV = mat.ClampV,
				MirrorU = mat.MirrorU,
				MirrorV = mat.MirrorV,
				UseAlpha = mat.UseAlpha,
				SourceAlpha = mat.SourceBlendMode,
				DestinationAlpha = mat.DestinationBlendmode,
				DoubleSided = !mat.BackfaceCulling,

				IgnoreLighting = mat.NoLighting,
				IgnoreSpecular = mat.NoSpecular,
				FlatShading = mat.Flat,
				UseTexture = mat.UseTexture,
				EnvironmentMap = mat.NormalMapping
			};
		}

		private static void ConvertToBasicStrips(
			BufferCorner[][] strips,
			bool[] reversedStrips,
			out IBasicPolygon[] polygons,
			out Vector2[] texcoords,
			out Color[] colors)
		{
			polygons = new IBasicPolygon[strips.Length];

			int cornerCount = strips.Sum(x => x.Length);
			texcoords = new Vector2[cornerCount];
			colors = new Color[cornerCount];

			int absoluteIndex = 0;

			for(int i = 0; i < strips.Length; i++)
			{
				BufferCorner[] strip = strips[i];

				BasicMultiPolygon polygon = new((ushort)strip.Length, reversedStrips[i]);

				for(int j = 0; j < strip.Length; j++, absoluteIndex++)
				{
					BufferCorner corner = strip[j];

					polygon.Indices[j] = corner.VertexIndex;
					colors[absoluteIndex] = corner.Color;
					texcoords[absoluteIndex] = corner.Texcoord;
				}

				polygons[i] = polygon;
			}
		}

		private static void ConvertToBasicTriangles(
			BufferCorner[] corners,
			out IBasicPolygon[] polygons,
			out Vector2[] texcoords,
			out Color[] colors)
		{
			polygons = new IBasicPolygon[corners.Length / 3];
			texcoords = new Vector2[corners.Length];
			colors = new Color[corners.Length];

			int absoluteIndex = 0;

			for(int i = 0; i < polygons.Length; i++)
			{
				BufferCorner corner1 = corners[absoluteIndex];
				colors[absoluteIndex] = corner1.Color;
				texcoords[absoluteIndex] = corner1.Texcoord;
				absoluteIndex++;

				BufferCorner corner2 = corners[absoluteIndex];
				colors[absoluteIndex] = corner2.Color;
				texcoords[absoluteIndex] = corner2.Texcoord;
				absoluteIndex++;

				BufferCorner corner3 = corners[absoluteIndex];
				colors[absoluteIndex] = corner3.Color;
				texcoords[absoluteIndex] = corner3.Texcoord;
				absoluteIndex++;

				polygons[i] = new BasicTriangle(corner1.VertexIndex, corner2.VertexIndex, corner3.VertexIndex);
			}
		}

		private static BasicMesh ConvertToBasicMesh(BufferCorner[] bCorners, bool hasColors, int index, string identifier)
		{
			BufferCorner[][] strips = TriangleStrippifier.Global.StrippifyNoDegen(bCorners, out bool[] reversed);

			int triangleByteLength = bCorners.Length * 2;
			int stripByteLength = (strips.Length + strips.Sum(x => x.Length)) * 2;

			BasicPolygonType type;
			IBasicPolygon[] polygons;
			Vector2[] texcoords;
			Color[] colors;

			if(stripByteLength < triangleByteLength)
			{
				type = BasicPolygonType.TriangleStrips;
				ConvertToBasicStrips(strips, reversed, out polygons, out texcoords, out colors);
			}
			else
			{
				type = BasicPolygonType.Triangles;
				ConvertToBasicTriangles(bCorners, out polygons, out texcoords, out colors);
			}

			bool hasTexcoords = texcoords.Any(x => x != default);

			BasicMesh basicmesh = new(type, polygons, (ushort)index, false, hasColors, hasTexcoords);

			if(hasColors)
			{
				basicmesh.Colors = new LabeledArray<Color>("vcolor_" + index + "_" + identifier, colors);
			}

			if(hasTexcoords)
			{
				basicmesh.Texcoords = new LabeledArray<Vector2>("uv_" + index + "_" + identifier, texcoords);
			}

			return basicmesh;
		}

		private static BasicAttach OptimizeBasicVertices(BasicAttach attach)
		{
			PositionNormal[] vertices = new PositionNormal[attach.Positions.Length];

			for(int i = 0; i < vertices.Length; i++)
			{
				vertices[i] = new(attach.Positions[i], attach.Normals[i]);
			}

			if(!DistinctMap<PositionNormal>.TryCreateDistinctMap(vertices, out DistinctMap<PositionNormal> distinctMap))
			{
				return attach;
			}

			LabeledArray<Vector3> positions = new(attach.Positions.Label, distinctMap.Values.Count);
			LabeledArray<Vector3> normals = new(attach.Normals.Label, positions.Length);

			for(int i = 0; i < positions.Length; i++)
			{
				PositionNormal pn = distinctMap.Values[i];
				positions[i] = pn.position;
				normals[i] = pn.normal;
			}

			foreach(BasicMesh mesh in attach.Meshes)
			{
				foreach(IBasicPolygon polygon in mesh.Polygons)
				{
					for(int i = 0; i < polygon.NumIndices; i++)
					{
						polygon[i] = distinctMap[polygon[i]];
					}
				}
			}

			return new BasicAttach(positions, normals, attach.Meshes, attach.Materials)
			{
				MeshBounds = attach.MeshBounds
			};
		}

		public static BasicAttach CreateBasicAttach(WeightedVertex[] vertices, BufferCorner[][] triangleSets, BufferMaterial[] bMaterials, bool hasColors, string label)
		{
			Vector3[] positions = new Vector3[vertices.Length];
			Vector3[] normals = new Vector3[positions.Length];
			string identifier = StringExtensions.GenerateIdentifier();

			for(int i = 0; i < positions.Length; i++)
			{
				WeightedVertex vtx = vertices[i];

				positions[i] = vtx.Position;
				normals[i] = vtx.Normal;
			}

			BasicMesh[] meshes = new BasicMesh[triangleSets.Length];
			BasicMaterial[] materials = new BasicMaterial[meshes.Length];

			for(int i = 0; i < meshes.Length; i++)
			{
				meshes[i] = ConvertToBasicMesh(triangleSets[i], hasColors, i, identifier);
				materials[i] = ConvertToBasicMaterial(bMaterials[i]);
			}

			BasicAttach result = new(positions, normals, meshes, materials)
			{
				Label = label
			};

			result.RecalculateBounds();

			return result;
		}

		public static void ConvertWeightedToBasic(
			Node model,
			WeightedMesh[] meshData,
			bool optimize)
		{
			if(meshData.Any(x => x.IsWeighted))
			{
				ToWeldedBasicConverter.ConvertWeightedToWeldedBasic(model, meshData, optimize);
				return;
			}

			Node[] nodes = model.GetTreeNodes();
			BasicAttach?[] attaches = new BasicAttach[nodes.Length];

			meshData = WeightedMesh.MergeAtRoots(meshData);

			foreach(WeightedMesh weightedMesh in meshData)
			{
				BasicAttach result = CreateBasicAttach(
					weightedMesh.Vertices,
					weightedMesh.TriangleSets,
					weightedMesh.Materials,
					weightedMesh.HasColors,
					weightedMesh.Label ?? "BASIC_" + StringExtensions.GenerateIdentifier());

				if(optimize)
				{
					result = OptimizeBasicVertices(result);
				}

				if(weightedMesh.NoBounds)
				{
					result.MeshBounds = default;
				}

				foreach(int index in weightedMesh.RootIndices)
				{
					attaches[index] = result;
				}
			}

			model.ClearAttachesFromTree();
			model.ClearWeldingsFromTree();

			for(int i = 0; i < nodes.Length; i++)
			{
				nodes[i].Attach = attaches[i];
			}
		}

		#endregion

		#region Convert to Buffer

		public static BufferMaterial ConvertToBufferMaterial(BasicMaterial mat)
		{
			return new(BufferMaterial.DefaultValues)
			{
				Diffuse = mat.DiffuseColor,
				Specular = mat.SpecularColor,
				SpecularExponent = mat.SpecularExponent,
				TextureIndex = mat.TextureID,
				TextureFiltering = mat.FilterMode,
				MipmapDistanceMultiplier = mat.MipmapDistanceMultiplier,
				AnisotropicFiltering = mat.SuperSample,
				ClampU = mat.ClampU,
				ClampV = mat.ClampV,
				MirrorU = mat.MirrorU,
				MirrorV = mat.MirrorV,
				UseAlpha = mat.UseAlpha,
				SourceBlendMode = mat.SourceAlpha,
				DestinationBlendmode = mat.DestinationAlpha,
				BackfaceCulling = !mat.DoubleSided,

				NoLighting = mat.IgnoreLighting,
				NoSpecular = mat.IgnoreSpecular,
				Flat = mat.FlatShading,
				UseTexture = mat.UseTexture,
				NormalMapping = mat.EnvironmentMap
			};
		}

		public static void ConvertPolygons(BasicMesh mesh, out BufferCorner[] corners, out uint[]? indexList, out bool strippified)
		{
			strippified = mesh.PolygonType is BasicPolygonType.TriangleStrips or BasicPolygonType.NPoly;

			if(mesh.PolygonType is BasicPolygonType.NPoly or BasicPolygonType.TriangleStrips)
			{
				indexList = null;

				IEnumerable<BasicMultiPolygon> polys = mesh.Polygons.Cast<BasicMultiPolygon>();

				BufferCorner[][] strips = new BufferCorner[mesh.Polygons.Length][];
				bool[] reversed = new bool[strips.Length];

				int stripNum = 0;
				int absoluteIndex = 0;

				foreach(BasicMultiPolygon poly in polys)
				{
					BufferCorner[] strip = new BufferCorner[poly.Indices.Length];

					if(mesh.PolygonType == BasicPolygonType.NPoly)
					{
						strip[0] = new BufferCorner(
							poly.Indices[0],
							mesh.Colors?[absoluteIndex] ?? BufferMesh.DefaultColor,
							mesh.Texcoords?[absoluteIndex] ?? Vector2.Zero);

						ushort beginIndex = 1;
						ushort endIndex = (ushort)(poly.Indices.Length - 1);
						bool fromEnd = false;

						for(int i = 1; i < poly.Indices.Length; i++)
						{
							ushort stripIndex;

							if(fromEnd)
							{
								stripIndex = endIndex;
								endIndex--;
							}
							else
							{
								stripIndex = beginIndex;
								beginIndex++;
							}

							fromEnd = !fromEnd;

							strip[i] = new BufferCorner(
								poly.Indices[stripIndex],
								mesh.Colors?[absoluteIndex + stripIndex] ?? BufferMesh.DefaultColor,
								mesh.Texcoords?[absoluteIndex + stripIndex] ?? Vector2.Zero);
						}

						absoluteIndex += strip.Length;
					}
					else
					{
						for(int i = 0; i < strip.Length; i++, absoluteIndex++)
						{
							strip[i] = new BufferCorner(
								poly.Indices[i],
								mesh.Colors?[absoluteIndex] ?? BufferMesh.DefaultColor,
								mesh.Texcoords?[absoluteIndex] ?? Vector2.Zero);
						}
					}

					strips[stripNum] = strip;
					reversed[stripNum] = poly.Reversed;
					stripNum++;
				}

				corners = TriangleStrippifier.JoinStrips(strips, reversed);
			}
			else
			{
				int absoluteIndex = 0;
				corners = new BufferCorner[mesh.PolygonCornerCount];

				foreach(IBasicPolygon triangle in mesh.Polygons)
				{
					foreach(ushort index in triangle)
					{
						corners[absoluteIndex] = new BufferCorner(
							index,
							mesh.Colors?[absoluteIndex] ?? BufferMesh.DefaultColor,
							mesh.Texcoords?[absoluteIndex] ?? Vector2.Zero);
						absoluteIndex++;
					}
				}

				if(mesh.PolygonType == BasicPolygonType.Quads)
				{
					indexList = new uint[mesh.Polygons.Length * 6];

					for(uint i = 0, q = 0; i < corners.Length; i += 4, q += 6)
					{
						indexList[q] = i;
						indexList[q + 1] = i + 1;
						indexList[q + 2] = i + 2;

						indexList[q + 3] = i + 2;
						indexList[q + 4] = i + 1;
						indexList[q + 5] = i + 3;
					}
				}
				else
				{
					indexList = null;
				}
			}

		}

		public static BufferMesh[] ConvertBasicToBuffer(BasicAttach attach, bool optimize)
		{
			BufferVertex[] verts = new BufferVertex[attach.Positions.Length];
			for(ushort i = 0; i < verts.Length; i++)
			{
				verts[i] = new BufferVertex(attach.Positions[i], attach.Normals[i], i);
			}

			bool hasNormals = attach.Normals.Any(x => !x.Equals(BufferMesh.DefaultNormal));

			List<BufferMesh> meshes = [];
			foreach(BasicMesh mesh in attach.Meshes)
			{
				// creating the material
				BufferMaterial bMat = ConvertToBufferMaterial(
					mesh.MaterialIndex < attach.Materials.Length
					? attach.Materials[mesh.MaterialIndex]
					: BasicMaterial.DefaultValues);

				ConvertPolygons(mesh, out BufferCorner[] corners, out uint[]? indexList, out bool strippified);

				// first mesh includes vertex data
				BufferMesh bmesh = meshes.Count == 0
					? new(verts, bMat, corners, indexList, strippified, false, hasNormals, mesh.Colors != null, 0, 0)
					: new(bMat, corners, indexList, strippified, mesh.Colors != null, 0);

				if(optimize)
				{
					bmesh.OptimizePolygons();
				}

				meshes.Add(bmesh);
			}

			return BufferMesh.CompressLayout(meshes);
		}

		/// <summary>
		/// Generates Buffer meshes for all attaches in the model
		/// </summary>
		/// <param name="model">The tip of the model hierarchy to convert</param>
		/// <param name="optimize">Whether the buffer model should be optimized</param>
		public static void BufferBasicModel(Node model, bool optimize = true)
		{
			if(!FromWeldedBasicConverter.BufferWeldedBasicModel(model, optimize))
			{
				foreach(Attach atc in model.GetTreeAttaches())
				{
					atc.MeshData = ConvertBasicToBuffer((BasicAttach)atc, optimize);
				}
			}
		}

		#endregion
	}
}
