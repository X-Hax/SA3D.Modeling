using SA3D.Modeling.Mesh.Gamecube.Enums;
using System;
using System.Runtime.InteropServices;

namespace SA3D.Modeling.Mesh.Gamecube
{
	/// <summary>
	/// A single corner of a polygon, called loop
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct GCCorner : IEquatable<GCCorner>
	{
		/// <summary>
		/// The index to <see cref="GCVertexType.PositionMatrixID"/>.
		/// </summary>
		public ushort PositionMatrixIDIndex { get; set; }

		/// <summary>
		/// The index to <see cref="GCVertexType.Position"/>.
		/// </summary>
		public ushort PositionIndex { get; set; }

		/// <summary>
		/// The index to <see cref="GCVertexType.Normal"/>.
		/// </summary>
		public ushort NormalIndex { get; set; }

		/// <summary>
		/// The index to <see cref="GCVertexType.Color0"/>.
		/// </summary>
		public ushort Color0Index { get; set; }

		/// <summary>
		/// The index to <see cref="GCVertexType.Color1"/>.
		/// </summary>
		public ushort Color1Index { get; set; }

		/// <summary>
		/// The index to <see cref="GCVertexType.TexCoord0"/>.
		/// </summary>
		public ushort TexCoord0Index { get; set; }

		/// <summary>
		/// The index to <see cref="GCVertexType.TexCoord1"/>.
		/// </summary>
		public ushort TexCoord1Index { get; set; }

		/// <summary>
		/// The index to <see cref="GCVertexType.TexCoord2"/>.
		/// </summary>
		public ushort TexCoord2Index { get; set; }

		/// <summary>
		/// The index to <see cref="GCVertexType.TexCoord3"/>.
		/// </summary>
		public ushort TexCoord3Index { get; set; }

		/// <summary>
		/// The index to <see cref="GCVertexType.TexCoord4"/>.
		/// </summary>
		public ushort TexCoord4Index { get; set; }

		/// <summary>
		/// The index to <see cref="GCVertexType.TexCoord5"/>.
		/// </summary>
		public ushort TexCoord5Index { get; set; }

		/// <summary>
		/// The index to <see cref="GCVertexType.TexCoord6"/>.
		/// </summary>
		public ushort TexCoord6Index { get; set; }

		/// <summary>
		/// The index to <see cref="GCVertexType.TexCoord7"/>.
		/// </summary>
		public ushort TexCoord7Index { get; set; }


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is GCCorner corner &&
				   PositionMatrixIDIndex == corner.PositionMatrixIDIndex &&
				   PositionIndex == corner.PositionIndex &&
				   NormalIndex == corner.NormalIndex &&
				   Color0Index == corner.Color0Index &&
				   Color1Index == corner.Color1Index &&
				   TexCoord0Index == corner.TexCoord0Index &&
				   TexCoord1Index == corner.TexCoord1Index &&
				   TexCoord2Index == corner.TexCoord2Index &&
				   TexCoord3Index == corner.TexCoord3Index &&
				   TexCoord4Index == corner.TexCoord4Index &&
				   TexCoord5Index == corner.TexCoord5Index &&
				   TexCoord6Index == corner.TexCoord6Index &&
				   TexCoord7Index == corner.TexCoord7Index;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			HashCode hash = new();
			hash.Add(PositionMatrixIDIndex);
			hash.Add(PositionIndex);
			hash.Add(NormalIndex);
			hash.Add(Color0Index);
			hash.Add(Color1Index);
			hash.Add(TexCoord0Index);
			hash.Add(TexCoord1Index);
			hash.Add(TexCoord2Index);
			hash.Add(TexCoord3Index);
			hash.Add(TexCoord4Index);
			hash.Add(TexCoord5Index);
			hash.Add(TexCoord6Index);
			hash.Add(TexCoord7Index);
			return hash.ToHashCode();
		}

		readonly bool IEquatable<GCCorner>.Equals(GCCorner other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two GC corners for equality.
		/// </summary>
		/// <param name="left">Lefthand corner.</param>
		/// <param name="right">Righthand corner.</param>
		/// <returns>Whether the two corners are equal.</returns>
		public static bool operator ==(GCCorner left, GCCorner right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two GC corners for inequality.
		/// </summary>
		/// <param name="left">Lefthand corner.</param>
		/// <param name="right">Righthand corner.</param>
		/// <returns>Whether the two corners are inequal.</returns>
		public static bool operator !=(GCCorner left, GCCorner right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"({PositionIndex}, {NormalIndex}, {Color0Index}, {TexCoord0Index})";
		}
	}
}
