using System;
using System.Collections.Generic;

namespace SA3D.Modeling.ObjectData.Structs
{
	/// <summary>
	/// Vertex weld influence.
	/// </summary>
	public readonly struct Weld : IEquatable<Weld>
	{
		/// <summary>
		/// Node that influences the vertex.
		/// </summary>
		public Node SourceNode { get; }

		/// <summary>
		/// Vertex index to the draw position and normal from.
		/// </summary>
		public uint VertexIndex { get; }

		/// <summary>
		/// Influence of the weld.
		/// </summary>
		public float Weight { get; }


		/// <summary>
		/// Creates a new weld.
		/// </summary>
		/// <param name="sourceNode">Node that influences the vertex.</param>
		/// <param name="vertexIndex">Vertex index to the draw position and normal from.</param>
		/// <param name="weight">Influence of the weld.</param>
		public Weld(Node sourceNode, uint vertexIndex, float weight)
		{
			SourceNode = sourceNode;
			VertexIndex = vertexIndex;
			Weight = weight;
		}


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is Weld weld &&
				   EqualityComparer<Node>.Default.Equals(SourceNode, weld.SourceNode) &&
				   VertexIndex == weld.VertexIndex &&
				   Weight == weld.Weight;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(SourceNode, VertexIndex, Weight);
		}

		/// <inheritdoc/>
		readonly bool IEquatable<Weld>.Equals(Weld other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two welds for equality.
		/// </summary>
		/// <param name="left">Lefthand weld.</param>
		/// <param name="right">Righthand weld.</param>
		/// <returns>Whether the two welds are equal.</returns>
		public static bool operator ==(Weld left, Weld right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two welds for inequality.
		/// </summary>
		/// <param name="left">Lefthand weld.</param>
		/// <param name="right">Righthand weld.</param>
		/// <returns>Whether the two welds are inequal.</returns>
		public static bool operator !=(Weld left, Weld right)
		{
			return !(left == right);
		}
	}
}
