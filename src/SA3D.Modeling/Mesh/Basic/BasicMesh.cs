using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Basic.Polygon;
using SA3D.Modeling.Structs;
using System;
using System.Linq;
using System.Numerics;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.Mesh.Basic
{
	/// <summary>
	/// BASIC format mesh structure for holding polygon information.
	/// </summary>
	public class BasicMesh : ICloneable
	{
		/// <summary>
		/// Number of bytes the structure occupies.
		/// </summary>
		public const uint StructSize = 24;

		/// <summary>
		/// Number of bytes the structure occupies. (SADX)
		/// </summary>
		public const uint StructSizeDX = 28;

		private ILabeledArray<Vector3>? _normals;
		private ILabeledArray<Color>? _colors;
		private ILabeledArray<Vector2>? _texcoords;

		/// <summary>
		/// Index indicating which material to use from <see cref="BasicAttach.Materials"/>.
		/// </summary>
		public ushort MaterialIndex { get; set; }

		/// <summary>
		/// Indicating how polygons are stored.
		/// </summary>
		public BasicPolygonType PolygonType { get; }

		/// <summary>
		/// Polygons of the mesh.
		/// </summary>
		public LabeledReadOnlyArray<IBasicPolygon> Polygons { get; }

		/// <summary>
		/// The amount of corners/loops in the polygons. Determines the lengths of the other arrays.
		/// </summary>
		public int PolygonCornerCount { get; }

		/// <summary>
		/// Polygon attributes. (Unused)
		/// </summary>
		public uint PolyAttributes { get; set; }

		/// <summary>
		/// Per corner custom polygon normals.
		/// </summary>
		public ILabeledArray<Vector3>? Normals
		{
			get => _normals;
			set
			{
				if(value != null && value.Length != PolygonCornerCount)
				{
					throw new ArgumentException($"New array has a length of {value.Length}, while {PolygonCornerCount} is expected");
				}

				_normals = value;
			}
		}

		/// <summary>
		/// Per corner polygon colors.
		/// </summary>
		public ILabeledArray<Color>? Colors
		{
			get => _colors;
			set
			{
				if(value != null && value.Length != PolygonCornerCount)
				{
					throw new ArgumentException($"New array has a length of {value.Length}, while {PolygonCornerCount} is expected");
				}

				_colors = value;
			}
		}

		/// <summary>
		/// Per corner polygon texture coordinates.
		/// </summary>
		public ILabeledArray<Vector2>? Texcoords
		{
			get => _texcoords;
			set
			{
				if(value != null && value.Length != PolygonCornerCount)
				{
					throw new ArgumentException($"New array has a length of {value.Length}, while {PolygonCornerCount} is expected");
				}

				_texcoords = value;
			}
		}

		private BasicMesh(
			ILabeledArray<Vector3>? normals,
			ILabeledArray<Color>? colors,
			ILabeledArray<Vector2>? texcoords,
			ushort materialIndex,
			BasicPolygonType polygonType,
			LabeledReadOnlyArray<IBasicPolygon> polygons,
			int polygonCornerCount,
			uint polyAttributes)
		{
			_normals = normals;
			_colors = colors;
			_texcoords = texcoords;
			MaterialIndex = materialIndex;
			PolygonType = polygonType;
			Polygons = polygons;
			PolygonCornerCount = polygonCornerCount;
			PolyAttributes = polyAttributes;
		}

		/// <summary>
		/// Creates a new basic mesh from preexisting data.
		/// </summary>
		/// <param name="materialID">Index indicating which material to use.</param>
		/// <param name="polyType">Indicating how polygons are stored.</param>
		/// <param name="polys">Polygons of the mesh.</param>
		/// <param name="normals">Per corner custom polygon normals.</param>
		/// <param name="colors">Per corner polygon colors.</param>
		/// <param name="texcoords">Per corner polygon texture coordinates.</param>
		public BasicMesh(
			ushort materialID,
			BasicPolygonType polyType,
			LabeledReadOnlyArray<IBasicPolygon> polys,
			ILabeledArray<Vector3>? normals,
			ILabeledArray<Color>? colors,
			ILabeledArray<Vector2>? texcoords)
		{
			MaterialIndex = materialID;
			PolygonType = polyType;
			Polygons = polys;

			foreach(IBasicPolygon p in Polygons)
			{
				PolygonCornerCount += p.NumIndices;
			}

			if(normals != null && normals.Length != PolygonCornerCount)
			{
				throw new ArgumentException($"Polygon corner count ({PolygonCornerCount}) and normal count ({normals.Length}) dont match up!", nameof(normals));
			}

			if(colors != null && colors.Length != PolygonCornerCount)
			{
				throw new ArgumentException($"Polygon corner count ({PolygonCornerCount}) and colors count ({colors.Length}) dont match up!", nameof(colors));
			}

			if(texcoords != null && texcoords.Length != PolygonCornerCount)
			{
				throw new ArgumentException($"Polygon corner count ({PolygonCornerCount}) and texcoord count ({texcoords.Length}) dont match up!", nameof(texcoords));
			}

			Normals = normals;
			Colors = colors;
			Texcoords = texcoords;
		}

		/// <summary>
		/// Creates a new (empty) mesh based on polygon data.
		/// </summary>
		/// <param name="polygonType">Indicating how polygons are stored.</param>
		/// <param name="polygons">Polygons to use.</param>
		/// <param name="materialIndex">Index indicating which material to use.</param>
		/// <param name="hasNormal">Whether the mesh contains custom normals</param>
		/// <param name="hasColor">Whether the model contains colors.</param>
		/// <param name="hasTexcoords">Whether the model contains texture coordinate.</param>
		public BasicMesh(
			BasicPolygonType polygonType,
			IBasicPolygon[] polygons,
			ushort materialIndex,
			bool hasNormal,
			bool hasColor,
			bool hasTexcoords)
		{
			PolygonType = polygonType;
			MaterialIndex = materialIndex;

			string identifier = GenerateIdentifier();
			Polygons = new LabeledReadOnlyArray<IBasicPolygon>("poly_" + identifier, polygons);

			foreach(IBasicPolygon p in polygons)
			{
				PolygonCornerCount += p.NumIndices;
			}

			if(hasNormal)
			{
				Normals = new LabeledArray<Vector3>("polynormal_" + identifier, PolygonCornerCount);
			}

			if(hasColor)
			{
				Colors = new LabeledArray<Color>("vcolor_" + identifier, PolygonCornerCount);
			}

			if(hasTexcoords)
			{
				Texcoords = new LabeledArray<Vector2>("uv_" + identifier, PolygonCornerCount);
			}
		}


		/// <summary>
		/// Reads a basic mesh off an endian strack reader.
		/// </summary>
		/// <param name="reader">Reader to read from.</param>
		/// <param name="address">Address at which the mesh is located.</param>
		/// <param name="lut">Pointer references to use.</param>
		/// <returns>The read mesh.</returns>
		public static BasicMesh Read(EndianStackReader reader, uint address, PointerLUT lut)
		{
			ushort header = reader.ReadUShort(address);
			ushort materialID = (ushort)(header & 0x3FFFu);
			BasicPolygonType polyType = (BasicPolygonType)(header >> 14);
			uint polyAttributes = reader.ReadUInt(address + 8);

			//==================================================================

			ushort polyCount = reader.ReadUShort(address + 2);
			uint polyAddr = reader.ReadPointer(address + 4);

			IBasicPolygon onReadPolys(EndianStackReader reader, ref uint address)
			{
				return IBasicPolygon.Read(reader, ref address, polyType);
			}

			LabeledReadOnlyArray<IBasicPolygon> polys = reader.ReadLabeledReadOnlyArray(polyAddr, polyCount, onReadPolys, "poly_", lut);

			int cornerCount = polys.Sum(x => x.NumIndices);

			//==================================================================

			LabeledArray<T>? ReadArray<T>(uint offset, string prefix, uint valueSize, EndianIOExtensions.ReadValueDelegate<T> readValue)
			{
				LabeledArray<T>? result = null;

				if(reader.TryReadPointer(address + offset, out uint pointer))
				{
					result = reader.ReadLabeledArray<T>(pointer, (uint)cornerCount, valueSize, readValue, prefix, lut);
				}

				return result;
			}

			LabeledArray<Vector3>? normals /****/ = ReadArray(0x0C, "polynormal_", 12, (r, p) => r.ReadVector3(p));
			LabeledArray<Color>? colors /*******/ = ReadArray(0x10, "vcolor_", /**/ 4, (r, p) => r.ReadColor(p, ColorIOType.ARGB8_32));
			LabeledArray<Vector2>? texcoords /**/ = ReadArray(0x14, "polynormal_", 08, (r, p) => r.ReadVector2(p, FloatIOType.Short) / 255f);

			//==================================================================

			return new BasicMesh(
				normals,
				colors,
				texcoords,
				materialID,
				polyType,
				polys,
				cornerCount,
				polyAttributes);
		}

		/// <summary>
		/// Writes the different data arrays to a stream
		/// </summary>
		/// <param name="writer">Output stream</param>
		/// <param name="lut"></param>
		public void WriteData(EndianStackWriter writer, PointerLUT lut)
		{
			_ = lut.GetAddAddress(Polygons, (array) =>
			{
				uint result = writer.PointerPosition;

				foreach(IBasicPolygon p in array)
				{
					p.Write(writer);
				}

				writer.Align(4);

				return result;
			});

			_ = writer.WriteCollectionWithLUT(Normals, (w, v) => w.WriteVector3(v), lut);
			_ = writer.WriteCollectionWithLUT(Colors, (w, c) => w.WriteColor(c, ColorIOType.ARGB8_32), lut);
			_ = writer.WriteCollectionWithLUT(Texcoords, (w, v) => w.WriteVector2(v * 255f, FloatIOType.Short), lut);
		}

		/// <summary>
		/// Writes the meshset to a stream
		/// </summary>
		/// <param name="writer">Ouput stream</param>
		/// <param name="DX">Whether the mesh should be written for SADX</param>
		/// <param name="lut"></param>
		public void WriteMeshset(EndianStackWriter writer, bool DX, PointerLUT lut)
		{
			uint normalsAddress = 0;
			uint colorsAddress = 0;
			uint texcoordAddress = 0;

			if(!lut.All.TryGetAddress(Polygons, out uint polyAddress)
				|| (Normals != null && !lut.All.TryGetAddress(Normals, out normalsAddress))
				|| (Colors != null && !lut.All.TryGetAddress(Colors, out colorsAddress))
				|| (Texcoords != null && !lut.All.TryGetAddress(Texcoords, out texcoordAddress)))
			{
				throw new NullReferenceException("Data has not been written yet");
			}

			ushort header = MaterialIndex;
			header |= (ushort)((uint)PolygonType << 14);

			writer.WriteUShort(header);
			writer.WriteUShort((ushort)Polygons.Length);
			writer.WriteUInt(polyAddress);
			writer.WriteUInt(PolyAttributes);
			writer.WriteUInt(normalsAddress);
			writer.WriteUInt(colorsAddress);
			writer.WriteUInt(texcoordAddress);

			if(DX)
			{
				writer.WriteEmpty(4);
			}
		}


		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the mesh.
		/// </summary>
		/// <returns>The clone.</returns>
		public BasicMesh Clone()
		{
			return new BasicMesh(
				Normals?.Clone(),
				Colors?.Clone(),
				Texcoords?.Clone(),
				MaterialIndex,
				PolygonType,
				Polygons,
				PolygonCornerCount,
				PolyAttributes);
		}

	}
}
