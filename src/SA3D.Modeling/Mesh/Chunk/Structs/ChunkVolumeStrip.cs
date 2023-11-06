using SA3D.Common.IO;
using System;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	/// <summary>
	/// Triangle strip polygon for volume chunks.
	/// </summary>
	public struct ChunkVolumeStrip : IChunkVolumePolygon
	{
		/// <summary>
		/// Vertex indices.
		/// </summary>
		public ushort[] Indices { get; }

		/// <inheritdoc/>
		public readonly int NumIndices => Indices.Length;

		/// <summary>
		/// Triangle attributes for each triangle. [triangle index, attribute index]
		/// </summary>
		public ushort[,] TriangleAttributes { get; }

		/// <summary>
		/// Whether the triangles use reversed culling direction.
		/// </summary>
		public bool Reversed { get; set; }

		/// <inheritdoc/>
		public readonly ushort this[int index]
		{
			get => Indices[index];
			set => Indices[index] = value;
		}

		private ChunkVolumeStrip(ushort[] indices, ushort[,] triangleAttributes, bool reversed)
		{
			Indices = indices;
			TriangleAttributes = triangleAttributes;
			Reversed = reversed;
		}

		/// <summary>
		/// Creates a new empty chunk volume strip.
		/// </summary>
		/// <param name="size">Number of vertex indices.</param>
		/// <param name="reversed">Whether the triangles use reversed culling direction.</param>
		public ChunkVolumeStrip(int size, bool reversed)
		{
			Indices = new ushort[size];
			TriangleAttributes = new ushort[size - 2, 3];
			Reversed = reversed;
		}

		/// <summary>
		/// Creates a new empty chunk volume strip.
		/// </summary>
		/// <param name="indices">Vertex indices to use.</param>
		/// <param name="reversed">Whether the triangles use reversed culling direction.</param>
		public ChunkVolumeStrip(ushort[] indices, bool reversed)
		{
			Indices = indices;
			TriangleAttributes = new ushort[Indices.Length - 2, 3];
			Reversed = reversed;
		}

		/// <inheritdoc/>
		public readonly ushort Size(int polygonAttributeCount)
		{
			return (ushort)(2u + (2 * (Indices.Length + (TriangleAttributes.Length * polygonAttributeCount))));
		}

		/// <inheritdoc/>
		public readonly void Write(EndianStackWriter writer, int polygonAttributeCount)
		{
			short count = (short)Math.Min(Indices.Length, short.MaxValue);
			writer.WriteShort(Reversed ? (short)-count : count);

			writer.WriteUShort(Indices[0]);
			writer.WriteUShort(Indices[1]);
			for(int i = 2; i < count; i++)
			{
				writer.WriteUShort(Indices[i]);

				for(int j = 0; j < polygonAttributeCount; j++)
				{
					writer.WriteUShort(TriangleAttributes[i - 2, j]);
				}
			}
		}

		/// <summary>
		/// Reads a chunk volume strip off an endian stack reader. Advances the address by the number of bytes read.
		/// </summary>
		/// <param name="reader">Reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="polygonAttributeCount">Number of attributes to read for each triangle in the strip.</param>
		/// <returns>The strip that was read.</returns>
		public static ChunkVolumeStrip Read(EndianStackReader reader, ref uint address, int polygonAttributeCount)
		{
			short header = reader.ReadShort(address);
			ChunkVolumeStrip result = new(Math.Abs(header), header < 0);
			address += 2;

			result.Indices[0] = reader.ReadUShort(address);
			result.Indices[1] = reader.ReadUShort(address += 2);

			for(int i = 2; i < result.Indices.Length; i++)
			{
				result.Indices[i] = reader.ReadUShort(address += 2);

				for(int j = 0; j < polygonAttributeCount; j++)
				{
					result.TriangleAttributes[i - 2, j] = reader.ReadUShort(address += 2);
				}
			}

			return result;
		}

		readonly object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Returns a deep clone of the chunk volume strip.
		/// </summary>
		/// <returns>The cloned strip.</returns>
		public readonly ChunkVolumeStrip Clone()
		{
			return new(
				(ushort[])Indices.Clone(),
				(ushort[,])TriangleAttributes.Clone(),
				Reversed);
		}
	}
}
