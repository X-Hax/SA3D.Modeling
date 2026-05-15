using Amicitia.IO.Binary;
using SA3D.Common.IO;
using System;

namespace SA3D.Modeling.File.MetaData.Weights
{
	/// <summary>
	/// Metadata weight.
	/// </summary>
	public struct MetaWeight : IEquatable<MetaWeight>, IBinarySerializable
	{
		/// <summary>
		/// Size of the structure in bytes.
		/// </summary>
		public const uint StructSize = 12;


		/// <summary>
		/// Pointer to the node that is weighted to.
		/// </summary>
		public long NodeOffset { get; set; }

		/// <summary>
		/// Vertex index to the draw position and normal from.
		/// </summary>
		public uint VertexIndex { get; set; }

		/// <summary>
		/// Influence of the weight.
		/// </summary>
		public float Weight { get; set; }


		/// <summary>
		/// Creates a new meta weight.
		/// </summary>
		/// <param name="nodePointer">Pointer to the node that is weighted to.</param>
		/// <param name="vertexIndex">Vertex cache index.</param>
		/// <param name="weight">Weight.</param>
		public MetaWeight(long nodePointer, uint vertexIndex, float weight)
		{
			NodeOffset = nodePointer;
			VertexIndex = vertexIndex;
			Weight = weight;
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			NodeOffset = reader.ReadOffsetValue();
			VertexIndex = reader.ReadUInt32();
			Weight = reader.ReadSingle();
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer)
		{
			writer.WriteOffsetValue(NodeOffset);
			writer.WriteUInt32(VertexIndex);
			writer.WriteSingle(Weight);
		}

		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is MetaWeight weight &&
				   NodeOffset == weight.NodeOffset &&
				   VertexIndex == weight.VertexIndex &&
				   Weight == weight.Weight;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(NodeOffset, VertexIndex, Weight);
		}

		/// <inheritdoc/>
		readonly bool IEquatable<MetaWeight>.Equals(MetaWeight other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two meta weights for equality.
		/// </summary>
		/// <param name="left">Lefthand meta weight.</param>
		/// <param name="right">Righthand meta weight.</param>
		/// <returns>Whether the two meta weights are equal.</returns>
		public static bool operator ==(MetaWeight left, MetaWeight right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two meta weights for inequality.
		/// </summary>
		/// <param name="left">Lefthand meta weight.</param>
		/// <param name="right">Righthand meta weight.</param>
		/// <returns>Whether the two meta weights are inequal.</returns>
		public static bool operator !=(MetaWeight left, MetaWeight right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{NodeOffset:X8} - {VertexIndex} - {Weight:F4}";
		}
	}
}
