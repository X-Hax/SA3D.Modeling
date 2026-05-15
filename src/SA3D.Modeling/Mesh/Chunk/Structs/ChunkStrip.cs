using System;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	/// <summary>
	/// Triangle string structure for strip chunks.
	/// </summary>
	public struct ChunkStrip : ICloneable
	{
		/// <summary>
		/// Maximum allowed size of a (collection of) strip chunk(s)
		/// </summary>
		public const uint MaxByteSize = (ushort.MaxValue * 2) - 2;

		/// <summary>
		/// Triangle corners. 
		/// <br/> The first two corners are only used for their index.
		/// </summary>
		public ChunkCorner[] Corners { get; private set; }

		/// <summary>
		/// Whether to inverse the culling direction of the triangles.
		/// </summary>
		public bool Reversed { get; private set; }


		/// <summary>
		/// Creates a new strip.
		/// </summary>
		/// <param name="corners">Triangle corners.</param>
		/// <param name="reverse">Whether to inverse the culling direction of the triangles</param>
		public ChunkStrip(ChunkCorner[] corners, bool reverse)
		{
			Reversed = reverse;
			Corners = corners;
		}


		readonly object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the strip.
		/// </summary>
		/// <returns>The cloned strip.</returns>
		public readonly ChunkStrip Clone()
		{
			return new((ChunkCorner[])Corners.Clone(), Reversed);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{Reversed} : {Corners.Length}";
		}
	}
}
