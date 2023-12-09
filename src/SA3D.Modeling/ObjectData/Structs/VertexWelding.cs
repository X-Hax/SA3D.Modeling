using System;
using System.Collections.Generic;

namespace SA3D.Modeling.ObjectData.Structs
{
	/// <summary>
	/// Vertex welding info. Used for Deforming meshes with no native weight support.
	/// </summary>
	public readonly struct VertexWelding : IEquatable<VertexWelding>
	{
		/// <summary>
		/// Index of the vertex being influenced.
		/// </summary>
		public uint DestinationVertexIndex { get; }

		/// <summary>
		/// Welds influencing the vertex.
		/// </summary>
		public Weld[] Welds { get; }


		/// <summary>
		/// Creates new vertex welding info.
		/// </summary>
		/// <param name="destinationVertexIndex">Index of the vertex being influenced.</param>
		/// <param name="welds">Welds influencing the vertex.</param>
		public VertexWelding(uint destinationVertexIndex, Weld[] welds)
		{
			DestinationVertexIndex = destinationVertexIndex;
			Welds = welds;
		}


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is VertexWelding weld &&
				   DestinationVertexIndex == weld.DestinationVertexIndex &&
				   EqualityComparer<Weld[]>.Default.Equals(Welds, weld.Welds);
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(DestinationVertexIndex, Welds);
		}

		/// <inheritdoc/>
		readonly bool IEquatable<VertexWelding>.Equals(VertexWelding other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two sets of vertex welding info for equality.
		/// </summary>
		/// <param name="left">Lefthand vertex welding info.</param>
		/// <param name="right">Righthand vertex welding info.</param>
		/// <returns>Whether the sets of vertex welding info are equal.</returns>
		public static bool operator ==(VertexWelding left, VertexWelding right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two sets of vertex welding info for inequality.
		/// </summary>
		/// <param name="left">Lefthand vertex welding info.</param>
		/// <param name="right">Righthand vertex welding info.</param>
		/// <returns>Whether the sets of vertex welding info are inequal.</returns>
		public static bool operator !=(VertexWelding left, VertexWelding right)
		{
			return !(left == right);
		}


		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{DestinationVertexIndex} - {Welds.Length}";
		}
	}
}
