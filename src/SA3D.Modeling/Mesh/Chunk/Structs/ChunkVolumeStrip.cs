using Amicitia.IO.Binary;
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
		public ushort[] Indices { get; set; }

		/// <inheritdoc/>
		public readonly int NumIndices => Indices.Length;

		/// <summary>
		/// Triangle attributes for each triangle. [triangle index, attribute index]
		/// </summary>
		public ushort[,] TriangleAttributes { get; set; }

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


		/// <summary>
		/// Verifies this strips polygon data
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public readonly void VerifyPolygonData()
		{
			if(Indices.Length < 3)
			{
				throw new InvalidOperationException("Volume strips require at least 3 indices!");
			}

			if(TriangleAttributes.Length != Indices.Length - 2)
			{
				throw new InvalidOperationException("Triangle attributes on volume strips are required to have 2 less than the length of the same polygons indices!");
			}

			if(TriangleAttributes.GetLength(1) != 3)
			{
				throw new InvalidOperationException("Triangle attributes on volume strips need to be 3 deep!");
			}
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, int polygonAttributeCount)
		{
			short header = reader.ReadInt16();
			Reversed = header < 0;
			Indices = new ushort[Math.Abs(header)];
			TriangleAttributes = new ushort[Indices.Length - 2, 3];

			Indices[0] = reader.ReadUInt16();
			Indices[1] = reader.ReadUInt16();

			for(int i = 2; i < Indices.Length; i++)
			{
				Indices[i] = reader.ReadUInt16();

				for(int j = 0; j < polygonAttributeCount; j++)
				{
					TriangleAttributes[i - 2, j] = reader.ReadUInt16();
				}
			}
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer, int polygonAttributeCount)
		{
			VerifyPolygonData();

			short count = (short)Math.Min(Indices.Length, short.MaxValue);
			writer.WriteInt16(Reversed ? (short)-count : count);

			writer.WriteUInt16(Indices[0]);
			writer.WriteUInt16(Indices[1]);
			for(int i = 2; i < count; i++)
			{
				writer.WriteUInt16(Indices[i]);

				for(int j = 0; j < polygonAttributeCount; j++)
				{
					writer.WriteUInt16(TriangleAttributes[i - 2, j]);
				}
			}
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
