using SA3D.Common.IO;
using System;

namespace SA3D.Modeling.File.Structs
{
	/// <summary>
	/// Metadata weight.
	/// </summary>
	public readonly struct MetaWeight : IEquatable<MetaWeight>
	{
		/// <summary>
		/// Size of the structure in bytes.
		/// </summary>
		public const uint StructSize = 12;


		/// <summary>
		/// Pointer to the node that is weighted to.
		/// </summary>
		public uint NodePointer { get; }

		/// <summary>
		/// Vertex index to the draw position and normal from.
		/// </summary>
		public uint VertexIndex { get; }

		/// <summary>
		/// Influence of the weight.
		/// </summary>
		public float Weight { get; }


		/// <summary>
		/// Creates a new meta weight.
		/// </summary>
		/// <param name="nodePointer">Pointer to the node that is weighted to.</param>
		/// <param name="vertexIndex">Vertex cache index.</param>
		/// <param name="weight">Weight.</param>
		public MetaWeight(uint nodePointer, uint vertexIndex, float weight)
		{
			NodePointer = nodePointer;
			VertexIndex = vertexIndex;
			Weight = weight;
		}


		/// <summary>
		/// Writes the meta weight to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public void Write(EndianStackWriter writer)
		{
			writer.WriteUInt(NodePointer);
			writer.WriteUInt(VertexIndex);
			writer.WriteFloat(Weight);
		}

		/// <summary>
		/// Reads a meta weight off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <returns>The read meta weight</returns>
		public static MetaWeight Read(EndianStackReader reader, uint address)
		{
			return new(
				reader.ReadUInt(address),
				reader.ReadUInt(address + 4),
				reader.ReadFloat(address + 8)
			);
		}


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is MetaWeight weight &&
				   NodePointer == weight.NodePointer &&
				   VertexIndex == weight.VertexIndex &&
				   Weight == weight.Weight;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(NodePointer, VertexIndex, Weight);
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
			return $"{NodePointer:X8} - {VertexIndex} - {Weight:F4}";
		}
	}
}
