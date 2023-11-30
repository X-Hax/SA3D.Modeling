using SA3D.Common.IO;
using System;
using System.Collections.Generic;

namespace SA3D.Modeling.File.Structs
{
	/// <summary>
	/// Vertex with weights
	/// </summary>
	public readonly struct MetaWeightVertex : IEquatable<MetaWeightVertex>
	{
		/// <summary>
		/// Index to the vertex that the weights influence.
		/// </summary>
		public uint DestinationVertexIndex { get; }

		/// <summary>
		/// Weights for the vertex.
		/// </summary>
		public MetaWeight[] Weights { get; }


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


		/// <summary>
		/// Writes the meta weight vertex to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public void Write(EndianStackWriter writer)
		{
			writer.WriteUInt(DestinationVertexIndex);
			writer.WriteInt(Weights.Length);

			foreach(MetaWeight weight in Weights)
			{
				weight.Write(writer);
			}
		}

		/// <summary>
		/// Reads a meta weight vertex off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <returns>The read meta weight vertex</returns>
		public static MetaWeightVertex Read(EndianStackReader reader, ref uint address)
		{
			uint destinationVertexIndex = reader.ReadUInt(address);
			int weightCount = reader.ReadInt(address + 4);

			address += 8;
			MetaWeight[] weights = new MetaWeight[weightCount];
			for(int i = 0; i < weightCount; i++)
			{
				weights[i] = MetaWeight.Read(reader, address);
				address += MetaWeight.StructSize;
			}

			return new(destinationVertexIndex, weights);
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
