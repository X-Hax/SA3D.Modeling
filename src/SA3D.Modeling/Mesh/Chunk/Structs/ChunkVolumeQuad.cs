using SA3D.Common.IO;
using System;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	/// <summary>
	/// Quad polygon for volume chunks.
	/// </summary>
	public struct ChunkVolumeQuad : IChunkVolumePolygon
	{
		/// <inheritdoc/>
		public readonly int NumIndices => 4;


		/// <summary>
		/// First vertex index.
		/// </summary>
		public ushort Index1 { get; set; }

		/// <summary>
		/// Second vertex index.
		/// </summary>
		public ushort Index2 { get; set; }

		/// <summary>
		/// Third vertex index.
		/// </summary>
		public ushort Index3 { get; set; }

		/// <summary>
		/// Fourth vertex index.
		/// </summary>
		public ushort Index4 { get; set; }


		/// <summary>
		/// First polygon attribute.
		/// </summary>
		public ushort Attribute1 { get; set; }

		/// <summary>
		/// Second polygon attribute.
		/// </summary>
		public ushort Attribute2 { get; set; }

		/// <summary>
		/// Third polygon attribute.
		/// </summary>
		public ushort Attribute3 { get; set; }


		/// <inheritdoc/>
		public ushort this[int index]
		{
			readonly get => index switch
			{
				0 => Index1,
				1 => Index2,
				2 => Index3,
				4 => Index4,
				_ => throw new IndexOutOfRangeException(),
			};
			set
			{
				switch(index)
				{
					case 0:
						Index1 = value;
						break;
					case 1:
						Index2 = value;
						break;
					case 2:
						Index3 = value;
						break;
					case 3:
						Index4 = value;
						break;
					default:
						throw new IndexOutOfRangeException();
				}
			}
		}


		/// <summary>
		/// Creates a new chunk volume quad.
		/// </summary>
		/// <param name="index1">First vertex index.</param>
		/// <param name="index2">Second vertex index.</param>
		/// <param name="index3">Third vertex index.</param>
		/// <param name="index4">Third vertex index.</param>
		/// <param name="attribute1">First polygon attribute.</param>
		/// <param name="attribute2">Second polygon attribute.</param>
		/// <param name="attribute3">Third polygon attribute.</param>
		public ChunkVolumeQuad(ushort index1, ushort index2, ushort index3, ushort index4, ushort attribute1, ushort attribute2, ushort attribute3)
		{
			Index1 = index1;
			Index2 = index2;
			Index3 = index3;
			Index4 = index4;
			Attribute1 = attribute1;
			Attribute2 = attribute2;
			Attribute3 = attribute3;
		}

		/// <summary>
		/// Creates a new chunk volume quad.
		/// </summary>
		/// <param name="index1">First vertex index.</param>
		/// <param name="index2">Second vertex index.</param>
		/// <param name="index3">Third vertex index.</param>
		/// <param name="index4">Third vertex index.</param>
		public ChunkVolumeQuad(ushort index1, ushort index2, ushort index3, ushort index4) : this()
		{
			Index1 = index1;
			Index2 = index2;
			Index3 = index3;
			Index4 = index4;
		}


		/// <inheritdoc/>
		public readonly ushort Size(int polygonAttributeCount)
		{
			return (ushort)(8u + (polygonAttributeCount * 2u));
		}

		/// <inheritdoc/>
		public readonly void Write(EndianStackWriter writer, int polygonAttributeCount)
		{
			writer.WriteUShort(Index1);
			writer.WriteUShort(Index2);
			writer.WriteUShort(Index3);
			writer.WriteUShort(Index4);

			if(polygonAttributeCount > 0)
			{
				writer.WriteUShort(Attribute1);
				if(polygonAttributeCount > 1)
				{
					writer.WriteUShort(Attribute2);
					if(polygonAttributeCount > 0)
					{
						writer.WriteUShort(Attribute3);
					}
				}
			}
		}

		/// <summary>
		/// Reads a chunk volume quad off an endian stack reader. Advances the address by the number of bytes read.
		/// </summary>
		/// <param name="reader">Reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="polygonAttributeCount">Number of attributes to read for the quad.</param>
		/// <returns>The quad that was read.</returns>
		public static ChunkVolumeQuad Read(EndianStackReader reader, ref uint address, int polygonAttributeCount)
		{
			ChunkVolumeQuad result = new(
				reader.ReadUShort(address),
				reader.ReadUShort(address + 2),
				reader.ReadUShort(address + 4),
				reader.ReadUShort(address + 6));

			address += 8;

			if(polygonAttributeCount > 0)
			{
				result.Attribute1 = reader.ReadUShort(address);
				address += 2;

				if(polygonAttributeCount > 1)
				{
					result.Attribute2 = reader.ReadUShort(address);
					address += 2;

					if(polygonAttributeCount > 2)
					{
						result.Attribute3 = reader.ReadUShort(address);
						address += 2;
					}
				}
			}

			return result;
		}


		readonly object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a clone of the quad.
		/// </summary>
		/// <returns>The clonsed quad.</returns>
		public readonly ChunkVolumeQuad Clone()
		{
			return this;
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Quad - {{ {Index1}, {Index2}, {Index3}, {Index4} }}";
		}
	}
}
