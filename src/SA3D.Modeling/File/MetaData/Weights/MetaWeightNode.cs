using Amicitia.IO.Binary;
using SA3D.Common.IO;
using System;
using System.Collections.Generic;

namespace SA3D.Modeling.File.MetaData.Weights
{
	/// <summary>
	/// Node with weight influence info.
	/// </summary>
	public struct MetaWeightNode : IEquatable<MetaWeightNode>, IBinarySerializable
	{
		/// <summary>
		/// Address of the node being weighted.
		/// </summary>
		public long NodeOffset { get; set; }

		/// <summary>
		/// Weight influences.
		/// </summary>
		public MetaWeightVertex[] VertexWeights { get; set; }


		/// <summary>
		/// Creates a new meta weight node.
		/// </summary>
		/// <param name="nodeOffset">Address of the node being weighted.</param>
		/// <param name="vertexWeights">Weight influences.</param>
		public MetaWeightNode(long nodeOffset, MetaWeightVertex[] vertexWeights)
		{
			NodeOffset = nodeOffset;
			VertexWeights = vertexWeights;
		}


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			NodeOffset = reader.ReadOffsetValue();
			int vertexCount = reader.ReadInt32();
			VertexWeights = reader.ReadObjectArray<MetaWeightVertex>(vertexCount);
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer)
		{
			writer.WriteOffsetValue(NodeOffset);
			writer.WriteInt32(VertexWeights.Length);
			writer.WriteObjectArray(VertexWeights);
		}


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is MetaWeightNode node &&
				   NodeOffset == node.NodeOffset &&
				   EqualityComparer<MetaWeightVertex[]>.Default.Equals(VertexWeights, node.VertexWeights);
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(NodeOffset, VertexWeights);
		}

		/// <inheritdoc/>
		readonly bool IEquatable<MetaWeightNode>.Equals(MetaWeightNode other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two meta weight nodes for equality.
		/// </summary>
		/// <param name="left">Lefthand meta weight node.</param>
		/// <param name="right">Righthand meta weight node.</param>
		/// <returns>Whether the two meta weight nodes are equal.</returns>
		public static bool operator ==(MetaWeightNode left, MetaWeightNode right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two meta weight nodes for inequality.
		/// </summary>
		/// <param name="left">Lefthand meta weight node.</param>
		/// <param name="right">Righthand meta weight node.</param>
		/// <returns>Whether the two meta weight nodes are inequal.</returns>
		public static bool operator !=(MetaWeightNode left, MetaWeightNode right)
		{
			return !(left == right);
		}


		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{NodeOffset:X8} - {VertexWeights.Length}";
		}
	}
}
