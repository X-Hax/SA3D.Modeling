using Amicitia.IO.Binary;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Basic.Polygon;
using SA3D.Modeling.Structs;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Numerics;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.Mesh.Basic
{
	/// <summary>
	/// BASIC format mesh structure for holding polygon information.
	/// </summary>
	public class BasicMeshSet : ICloneable, IBinarySerializable<IOContext>
	{
		/// <summary>
		/// Default label prefix for <see cref="Polygons"/>
		/// </summary>
		public const string PolygonLabelPrefix = "polygons_";

		/// <summary>
		/// Default label prefix for <see cref="Normals"/>
		/// </summary>
		public const string NormalsLabelPrefix = "polygon_normals_";

		/// <summary>
		/// Default label prefix for <see cref="Colors"/>
		/// </summary>
		public const string ColorsLabelPrefix = "polygon_colors_";

		/// <summary>
		/// Default label prefix for <see cref="TextureCoordinates"/>
		/// </summary>
		public const string TextureCoordinatesLabelPrefix = "polygon_texcoords_";


		/// <summary>
		/// Number of bytes the structure occupies.
		/// </summary>
		public const uint StructSize = 24;

		/// <summary>
		/// Number of bytes the structure occupies. (SADX)
		/// </summary>
		public const uint StructSizeDX = 28;

		/// <summary>
		/// Index indicating which material to use from <see cref="BasicMesh.Materials"/>.
		/// </summary>
		public ushort MaterialIndex { get; set; }

		/// <summary>
		/// Polygon attributes (unused)
		/// </summary>
		public uint PolygonAttributes { get; set; }

		/// <summary>
		/// Indicating how polygons are stored.
		/// </summary>
		public BasicPolygonType PolygonType { get; set; }

		/// <summary>
		/// Polygons of the mesh.
		/// </summary>
		public LabeledArray<IBasicPolygon> Polygons { get; set; }

		/// <summary>
		/// Total number of corners in <see cref="Polygons"/>. Also the expected array length for <see cref="Normals"/>, <see cref="Colors"/> and <see cref="TextureCoordinates"/>
		/// </summary>
		public int PolygonCornerCount => Polygons.Sum(x => x.NumIndices);

		/// <summary>
		/// Polygon corner normals
		/// </summary>
		public LabeledArray<Vector3>? Normals { get; set; }

		/// <summary>
		/// Polygon corner colors
		/// </summary>
		public LabeledArray<Color>? Colors { get; set; }

		/// <summary>
		/// Polygon corner texture coordinates
		/// </summary>
		public LabeledArray<Vector2>? TextureCoordinates { get; set; }


		/// <summary>
		/// Creates a new, blank basic mesh
		/// </summary>
		public BasicMeshSet()
		{
			Polygons = new LabeledArray<IBasicPolygon>(PolygonLabelPrefix.GenerateIdentifier(), 0);
		}

		/// <summary>
		/// Checks whether polygon data is valid and throws an <see cref="InvalidDataException"/> if not.
		/// </summary>
		/// <exception cref="InvalidDataException"></exception>
		public void VerifyPolygonData()
		{
			Type expectedPolygonType = PolygonType switch
			{
				BasicPolygonType.Triangles => typeof(BasicTriangle),
				BasicPolygonType.Quads => typeof(BasicQuad),
				BasicPolygonType.NPoly or BasicPolygonType.TriangleStrips => typeof(BasicTriangle),
				_ => throw new InvalidDataException($"Invalid polygon type \"{PolygonType}\"!"),
			};

			if(Polygons.Any(x => x.GetType() != expectedPolygonType))
			{
				throw new InvalidDataException($"Not all polygons are of the expected type {expectedPolygonType}!");
			}

			if(Normals == null && Colors == null && TextureCoordinates == null)
			{
				return;
			}

			int cornerCount = PolygonCornerCount;

			if(Normals != null && Normals.Length != cornerCount)
			{
				throw new InvalidDataException($"Mesh has {cornerCount} corners, but {Normals.Length} normals!");
			}

			if(Colors != null && Colors.Length != cornerCount)
			{
				throw new InvalidDataException($"Mesh has {cornerCount} corners, but {Colors.Length} colors!");
			}

			if(TextureCoordinates != null && TextureCoordinates.Length != cornerCount)
			{
				throw new InvalidDataException($"Mesh has {cornerCount} corners, but {TextureCoordinates.Length} texture coordinates!");
			}
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, IOContext context)
		{
			ushort header = reader.ReadUInt16();
			MaterialIndex = (ushort)(header & 0x3FFFu);
			PolygonType = (BasicPolygonType)(header >> 14);

			ushort polyCount = reader.ReadUInt16();

			Polygons = reader.ReadLabeledObjectArrayOffset(IBasicPolygon.GetReader(PolygonType), polyCount, "poly_", context.PointerLUT)
				?? throw reader.ReadNullReference(nameof(BasicMeshSet), nameof(Polygon));

			PolygonAttributes = reader.ReadUInt32();

			int cornerCount = PolygonCornerCount;

			Normals = reader.ReadLabeledObjectArrayOffset(StructBinaryHelper.ReadVector3, cornerCount, NormalsLabelPrefix, context.PointerLUT);
			Colors = reader.ReadLabeledObjectArrayOffset(r => r.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_32), cornerCount, ColorsLabelPrefix, context.PointerLUT);
			TextureCoordinates = reader.ReadLabeledObjectArrayOffset(FloatIOType.Short.GetVector2Reader(), cornerCount, TextureCoordinatesLabelPrefix, context.PointerLUT);
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, IOContext context)
		{
			VerifyPolygonData();

			ushort header = (ushort)((MaterialIndex & 0x3FFFu) | (ushort)((int)PolygonType << 14));
			writer.WriteUInt16(header);
			writer.WriteUInt16((ushort)Polygons.Length);
			writer.WriteObjectArrayOffset(Polygons, context.PointerLUT);
			writer.WriteUInt32(PolygonAttributes);
			writer.WriteObjectArrayOffset(StructBinaryHelper.WriteVector3, Normals, context.PointerLUT);
			writer.WriteObjectArrayOffset((w, v) => w.WriteObject(v, ColorIOType.ARGB8_32), Colors, context.PointerLUT);
			writer.WriteObjectArrayOffset(FloatIOType.Short.GetVector2Writer(), TextureCoordinates, context.PointerLUT);
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the mesh.
		/// </summary>
		/// <returns>The clone.</returns>
		public BasicMeshSet Clone()
		{
			return new()
			{
				MaterialIndex = MaterialIndex,
				PolygonAttributes = PolygonAttributes,
				PolygonType = PolygonType,
				Polygons = new LabeledArray<IBasicPolygon>(Polygons.Label, [.. Polygons.Select(x => (IBasicPolygon)x.Clone())]),
				Normals = Normals?.Clone(),
				Colors = Colors?.Clone(),
				TextureCoordinates = TextureCoordinates?.Clone()
			};
		}
	}
}
