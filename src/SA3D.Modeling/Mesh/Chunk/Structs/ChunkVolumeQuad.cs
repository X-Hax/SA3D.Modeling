using Amicitia.IO.Binary;
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
		public void Read(BinaryObjectReader reader, int polygonAttributeCount)
		{
			Index1 = reader.ReadUInt16();
			Index2 = reader.ReadUInt16();
			Index3 = reader.ReadUInt16();
			Index4 = reader.ReadUInt16();

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
			writer.WriteUInt16(Index4);

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
