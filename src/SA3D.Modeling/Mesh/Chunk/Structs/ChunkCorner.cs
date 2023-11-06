using SA3D.Modeling.Structs;
using System;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	/// <summary>
	/// A single polygon corner for chunk models.
	/// </summary>
	public struct ChunkCorner : IEquatable<ChunkCorner>
	{
		/// <summary>
		/// Vertex Cache index.
		/// </summary>
		public ushort Index { get; set; }

		/// <summary>
		/// Texture coordinates.
		/// </summary>
		public Vector2 Texcoord { get; set; }

		/// <summary>
		/// Second set of texture coordinates.
		/// </summary>
		public Vector2 Texcoord2 { get; set; }

		/// <summary>
		/// Normalized direction.
		/// </summary>
		public Vector3 Normal { get; set; }

		/// <summary>
		/// Color.
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		/// First set of attributes for the triangle that this corner closes.
		/// </summary>
		public ushort Attributes1 { get; set; }

		/// <summary>
		/// Second set of attributes for the triangle that this corner closes.
		/// </summary>
		public ushort Attributes2 { get; set; }

		/// <summary>
		/// Third set of attributes for the triangle that this corner closes.
		/// </summary>
		public ushort Attributes3 { get; set; }


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is ChunkCorner corner &&
				   Index == corner.Index &&
				   Texcoord.Equals(corner.Texcoord) &&
				   Normal.Equals(corner.Normal) &&
				   Color.Equals(corner.Color) &&
				   Attributes1 == corner.Attributes1 &&
				   Attributes2 == corner.Attributes2 &&
				   Attributes3 == corner.Attributes3;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(Index, Texcoord, Normal, Color, Attributes1, Attributes2, Attributes3);
		}

		readonly bool IEquatable<ChunkCorner>.Equals(ChunkCorner other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two chunk corners for equality.
		/// </summary>
		/// <param name="left">Lefthand corner.</param>
		/// <param name="right">Righthand corner.</param>
		/// <returns>Wether the corners are equal.</returns>
		public static bool operator ==(ChunkCorner left, ChunkCorner right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two chunk corners for inequality.
		/// </summary>
		/// <param name="left">Lefthand corner.</param>
		/// <param name="right">Righthand corner.</param>
		/// <returns>Wether the corners are inequal.</returns>
		public static bool operator !=(ChunkCorner left, ChunkCorner right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{Index} : {Texcoord.DebugString()}, {Color}";
		}
	}
}
