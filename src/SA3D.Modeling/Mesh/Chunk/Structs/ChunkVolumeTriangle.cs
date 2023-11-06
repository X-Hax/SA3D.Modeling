using SA3D.Common.IO;
using System;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	/// <summary>
	/// Triangle polygon for volume chunks.
	/// </summary>
	public struct ChunkVolumeTriangle : IChunkVolumePolygon
	{
		/// <inheritdoc/>
		public readonly int NumIndices => 3;


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
					default:
						throw new IndexOutOfRangeException();
				}
			}
		}


		/// <summary>
		/// Creates a new chunk volume triangle.
		/// </summary>
		/// <param name="index1">First vertex index.</param>
		/// <param name="index2">Second vertex index.</param>
		/// <param name="index3">Third vertex index.</param>
		/// <param name="attribute1">First polygon attribute.</param>
		/// <param name="attribute2">Second polygon attribute.</param>
		/// <param name="attribute3">Third polygon attribute.</param>
		public ChunkVolumeTriangle(ushort index1, ushort index2, ushort index3, ushort attribute1, ushort attribute2, ushort attribute3)
		{
			Index1 = index1;
			Index2 = index2;
			Index3 = index3;
			Attribute1 = attribute1;
			Attribute2 = attribute2;
			Attribute3 = attribute3;
		}

		/// <summary>
		/// Creates a new chunk volume triangle.
		/// </summary>
		/// <param name="index1">First vertex index.</param>
		/// <param name="index2">Second vertex index.</param>
		/// <param name="index3">Third vertex index.</param>
		public ChunkVolumeTriangle(ushort index1, ushort index2, ushort index3) : this()
		{
			Index1 = index1;
			Index2 = index2;
			Index3 = index3;
		}


		/// <inheritdoc/>
		public readonly ushort Size(int polygonAttributeCount)
		{
			return (ushort)(6u + (polygonAttributeCount * 2u));
		}

		/// <inheritdoc/>
		public readonly void Write(EndianStackWriter writer, int polygonAttributeCount)
		{
			writer.WriteUShort(Index1);
			writer.WriteUShort(Index2);
			writer.WriteUShort(Index3);

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
		/// Reads a chunk volume triangle off an endian stack reader. Advances the address by the number of bytes read.
		/// </summary>
		/// <param name="reader">Reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="polygonAttributeCount">Number of attributes to read for the triangle.</param>
		/// <returns>The triangle that was read.</returns>
		public static ChunkVolumeTriangle Read(EndianStackReader reader, ref uint address, int polygonAttributeCount)
		{
			ChunkVolumeTriangle result = new(
				reader.ReadUShort(address),
				reader.ReadUShort(address + 2),
				reader.ReadUShort(address + 4));

			address += 6;

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
		/// Creates a clone of the triangle.
		/// </summary>
		/// <returns>The clonsed triangle.</returns>
		public readonly ChunkVolumeTriangle Clone()
		{
			return this;
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Triangle - {{ {Index1}, {Index2}, {Index3} }}";
		}
	}
}
