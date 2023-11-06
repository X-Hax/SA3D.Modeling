using SA3D.Common.IO;
using SA3D.Modeling.Mesh.Gamecube.Enums;
using System;
using System.Collections.Generic;

namespace SA3D.Modeling.Mesh.Gamecube
{
	/// <summary>
	/// A collection of corners forming polygons
	/// </summary>
	public readonly struct GCPolygon : ICloneable
	{
		/// <summary>
		/// The way in which polygons are being stored.
		/// </summary>
		public GCPolyType Type { get; }

		/// <summary>
		/// Corners making up the polygons.
		/// </summary>
		public GCCorner[] Corners { get; }

		/// <summary>
		/// Create a new empty Primitive
		/// </summary>
		/// <param name="type">The type of primitive.</param>
		/// <param name="corners">Corners making up the polygons.</param>
		public GCPolygon(GCPolyType type, GCCorner[] corners)
		{
			Type = type;
			Corners = corners;
		}

		private unsafe delegate void ReadIndex(EndianStackReader reader, ushort* destination, ref uint address);


		/// <summary>
		/// Reads a gamecube polygon off an endian stack reader. Advances the pointer by the number of bytes read.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="indexFormat">Polygon index formatting.</param>
		public static unsafe GCPolygon Read(EndianStackReader reader, ref uint address, GCIndexFormat indexFormat)
		{
			reader.PushBigEndian(true);

			GCPolyType type = (GCPolyType)reader[address];
			ushort vtxCount = reader.ReadUShort(address + 1);

			static void Read8(EndianStackReader reader, ushort* destination, ref uint address)
			{
				*destination = reader[address];
				address++;
			}

			static void Read16(EndianStackReader reader, ushort* destination, ref uint address)
			{
				*destination = reader.ReadUShort(address);
				address += 2;
			}

			List<(ReadIndex read, uint fieldOffset)> readList = new();

			uint attributes = (uint)indexFormat;
			for(uint i = 1, outOffset = 0; i <= 0x2000; i <<= 2, outOffset++)
			{
				if((attributes & (i << 1)) == 0)
				{
					continue;
				}

				if((attributes & i) == 0)
				{
					readList.Add((Read8, outOffset));
				}
				else
				{
					readList.Add((Read16, outOffset));
				}
			}

			address += 3;

			(ReadIndex read, uint fieldOffset)[] readArray = readList.ToArray();
			GCCorner[] corners = new GCCorner[vtxCount];

			fixed(GCCorner* corner = &corners[0])
			{
				GCCorner* current = corner;
				for(int i = 0; i < vtxCount; i++, current++)
				{
					for(int j = 0; j < readArray.Length; j++)
					{
						(ReadIndex read, uint fieldOffset) = readArray[j];
						read(reader, ((ushort*)current) + fieldOffset, ref address);
					}
				}
			}

			reader.PopEndian();
			return new GCPolygon(type, corners);
		}

		/// <summary>
		/// Writes the gamecube polygon to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="indexFormat">Polygon index formatting.</param>
		public readonly unsafe void Write(EndianStackWriter writer, GCIndexFormat indexFormat)
		{
			writer.PushBigEndian(true);

			writer.WriteByte((byte)Type);
			writer.WriteUShort((ushort)Corners.Length);

			static void Write8(EndianStackWriter writer, ushort value)
			{
				writer.WriteByte((byte)value);
			}

			static void Write16(EndianStackWriter writer, ushort value)
			{
				writer.WriteUShort(value);
			}

			List<(Action<EndianStackWriter, ushort> write, uint fieldOffset)> writeList = new();

			uint attributes = (uint)indexFormat;
			for(uint i = 1, outOffset = 0; i <= 0x2000; i <<= 2, outOffset++)
			{
				if((attributes & (i << 1)) == 0)
				{
					continue;
				}

				if((attributes & i) == 0)
				{
					writeList.Add((Write8, outOffset));
				}
				else
				{
					writeList.Add((Write16, outOffset));
				}
			}

			(Action<EndianStackWriter, ushort> write, uint fieldOffset)[] writeArray = writeList.ToArray();

			fixed(GCCorner* corner = &Corners[0])
			{
				GCCorner* current = corner;
				for(int i = 0; i < Corners.Length; i++, current++)
				{
					for(int j = 0; j < writeArray.Length; j++)
					{
						(Action<EndianStackWriter, ushort> write, uint fieldOffset) = writeArray[j];
						write(writer, *(((ushort*)current) + fieldOffset));
					}
				}
			}

			writer.PopEndian();
		}


		readonly object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the polygon.
		/// </summary>
		/// <returns>The cloned polygon</returns>
		public readonly GCPolygon Clone()
		{
			return new(Type, (GCCorner[])Corners.Clone());
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{Type}: {Corners.Length}";
		}
	}
}
