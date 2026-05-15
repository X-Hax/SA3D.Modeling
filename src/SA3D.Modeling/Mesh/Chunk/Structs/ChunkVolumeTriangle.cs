using Amicitia.IO.Binary;
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
		public void Read(BinaryObjectReader reader, int polygonAttributeCount)
		{
			Index1 = reader.ReadUInt16();
			Index2 = reader.ReadUInt16();
			Index3 = reader.ReadUInt16();

			if(polygonAttributeCount > 0)
			{
				Attribute1 = reader.ReadUInt16();

				if(polygonAttributeCount > 1)
				{
					Attribute2 = reader.ReadUInt16();

					if(polygonAttributeCount > 2)
					{
						Attribute3 = reader.ReadUInt16();
					}
				}
			}
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer, int polygonAttributeCount)
		{
			writer.WriteUInt16(Index1);
			writer.WriteUInt16(Index2);
			writer.WriteUInt16(Index3);

			if(polygonAttributeCount > 0)
			{
				writer.WriteUInt16(Attribute1);

				if(polygonAttributeCount > 1)
				{
					writer.WriteUInt16(Attribute2);

					if(polygonAttributeCount > 0)
					{
						writer.WriteUInt16(Attribute3);
					}
				}
			}
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
