using Amicitia.IO.Binary;
using SA3D.Common.IO;
using System;
using System.Collections.Generic;

namespace SA3D.Modeling.File.MetaData.Weights
{
	/// <summary>
	/// Vertex with weights
	/// </summary>
	public struct MetaWeightVertex : IEquatable<MetaWeightVertex>, IBinarySerializable
	{
		/// <summary>
		/// Index to the vertex that the weights influence.
		/// </summary>
		public uint DestinationVertexIndex { get; set; }

		/// <summary>
		/// Weights for the vertex.
		/// </summary>
		public MetaWeight[] Weights { get; set; }


		/// <summary>
		/// Creates a new meta weight vertex.
		/// </summary>
		/// <param name="destinationVertexIndex">Index to the vertex that the weights influence.</param>
		/// <param name="weights">Weights for the vertex.</param>
		public MetaWeightVertex(uint destinationVertexIndex, MetaWeight[] weights)
		{
			DestinationVertexIndex = destinationVertexIndex;
			Weights = weights;
		}


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			DestinationVertexIndex = reader.ReadUInt32();
			int weightCount = reader.ReadInt32();
			Weights = reader.ReadObjectArray<MetaWeight>(weightCount);
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer)
		{
			writer.WriteUInt32(DestinationVertexIndex);
			writer.WriteInt32(Weights.Length);
			writer.WriteObjectArray(Weights);
		}


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is MetaWeightVertex vertex &&
				   DestinationVertexIndex == vertex.DestinationVertexIndex &&
				   EqualityComparer<MetaWeight[]>.Default.Equals(Weights, vertex.Weights);
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(DestinationVertexIndex, Weights);
		}

		readonly bool IEquatable<MetaWeightVertex>.Equals(MetaWeightVertex other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two meta weight vertices for equality.
		/// </summary>
		/// <param name="left">Lefthand meta weight vertex.</param>
		/// <param name="right">Righthand meta weight vertex.</param>
		/// <returns>Whether the two meta weight vertices are equal.</returns>
		public static bool operator ==(MetaWeightVertex left, MetaWeightVertex right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two meta weight vertices for inequality.
		/// </summary>
		/// <param name="left">Lefthand meta weight vertex.</param>
		/// <param name="right">Righthand meta weight vertex.</param>
		/// <returns>Whether the two meta weight vertices are inequal.</returns>
		public static bool operator !=(MetaWeightVertex left, MetaWeightVertex right)
		{
			return !(left == right);
		}


		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{DestinationVertexIndex} - {Weights.Length}";
		}
	}
}
