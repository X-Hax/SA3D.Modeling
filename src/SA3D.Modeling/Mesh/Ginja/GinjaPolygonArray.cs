using Amicitia.IO.Binary;
using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Ginja.Enums;
using SA3D.Modeling.Mesh.Ginja.Structs;
using System.Collections.Generic;

namespace SA3D.Modeling.Mesh.Ginja
{
	/// <summary>
	/// Ginja polygon array
	/// </summary>
	public class GinjaPolygonArray : LabeledArray<GinjaPolygon>, IBinarySerializable<(GinjaIndexFormat format, int size)>
	{
		private const string _labelPrefix = "polygons_";

		/// <inheritdoc/>
		public override string LabelPrefix => _labelPrefix;

		/// <summary>
		/// Creates a new, empty polygon array
		/// </summary>
		public GinjaPolygonArray() : base(_labelPrefix.GenerateIdentifier()) { }

		/// <summary>
		/// Creates a new, empty polygon array
		/// </summary>
		public GinjaPolygonArray(int size) : base(_labelPrefix.GenerateIdentifier(), size) { }

		/// <summary>
		/// Creates a new polygon array with preexisting polygons
		/// </summary>
		public GinjaPolygonArray(IEnumerable<GinjaPolygon> polygons) : base(_labelPrefix.GenerateIdentifier(), [.. polygons]) { }


		internal static int[] GetIndexSizes(GinjaIndexFormat format)
		{
			int[] result = new int[13];
			uint attributes = (uint)format;
			for(uint i = 0, b = 1; b <= 0x2000; i++, b <<= 2)
			{
				if((attributes & (b << 1)) == 0)
				{
					continue;
				}

				result[i] = (attributes & b) == 0 ? 1 : 2;
			}

			return result;
		}

		unsafe void IBinarySerializable<(GinjaIndexFormat format, int size)>.Read(BinaryObjectReader reader, (GinjaIndexFormat format, int size) context)
		{
			List<GinjaPolygon> polygons = [];
			long end_pos = reader.Position + context.size;

			using EndiannessToken endianness = reader.WithEndian(Endianness.Big);
			int[] indexSizes = GetIndexSizes(context.format);

			while(reader.Position < end_pos)
			{
				GinjaPolyType type = (GinjaPolyType)reader.ReadByte();
				if(type == default)
				{
					break;
				}

				uint cornerCount = reader.ReadUInt16();
				GinjaPolygon polygon = new(type, new GinjaCorner[cornerCount]);

				fixed(GinjaCorner* corner = &polygon.Corners[0])
				{
					ushort* current = (ushort*)corner;
					for(int i = 0; i < cornerCount; i++)
					{
						foreach(int number in indexSizes)
						{
							if(number == 1)
							{
								*current = reader.ReadByte();
							}
							else if(number == 2)
							{
								*current = reader.ReadUInt16();
							}

							current++;
						}
					}
				}

				polygons.Add(polygon);
			}

			Array = [.. polygons];
		}

		unsafe void IBinarySerializable<(GinjaIndexFormat format, int size)>.Write(BinaryObjectWriter writer, (GinjaIndexFormat format, int size) context)
		{
			using EndiannessToken endianness = writer.WithEndian(Endianness.Big);
			int[] indexSize = GetIndexSizes(context.format);

			foreach(GinjaPolygon polygon in this)
			{
				writer.WriteByte((byte)polygon.Type);
				writer.WriteUInt16((ushort)polygon.Corners.Length);

				fixed(GinjaCorner* corner = &polygon.Corners[0])
				{
					ushort* current = (ushort*)corner;
					for(int i = 0; i < polygon.Corners.Length; i++)
					{
						foreach(int number in indexSize)
						{
							if(number == 1)
							{
								writer.WriteByte((byte)*current);
							}
							else if(number == 2)
							{
								writer.WriteUInt16(*current);
							}

							current++;
						}
					}
				}
			}
		}
	}
}
