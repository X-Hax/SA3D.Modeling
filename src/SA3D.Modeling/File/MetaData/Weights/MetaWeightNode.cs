using SA3D.Common.IO;
using System;
using System.Collections.Generic;

namespace SA3D.Modeling.File.Structs
{
	/// <summary>
	/// Node with weight influence info.
	/// </summary>
	public readonly struct MetaWeightNode : IEquatable<MetaWeightNode>
	{
		/// <summary>
		/// Address of the node being weighted.
		/// </summary>
		public uint NodePointer { get; }

		/// <summary>
		/// Weight influences.
		/// </summary>
		public MetaWeightVertex[] VertexWeights { get; }


		/// <summary>
		/// Creates a new meta weight node.
		/// </summary>
		/// <param name="nodePointer">Address of the node being weighted.</param>
		/// <param name="vertexWeights">Weight influences.</param>
		public MetaWeightNode(uint nodePointer, MetaWeightVertex[] vertexWeights)
		{
			NodePointer = nodePointer;
			VertexWeights = vertexWeights;
		}


		/// <summary>
		/// Writes the meta weight node to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public void Write(EndianStackWriter writer)
		{
			writer.WriteUInt(NodePointer);
			writer.WriteInt(VertexWeights.Length);

			foreach(MetaWeightVertex vertex in VertexWeights)
			{
				vertex.Write(writer);
			}
		}

		/// <summary>
		/// Reads a meta weight node off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <returns>The read meta weight node</returns>
		public static MetaWeightNode Read(EndianStackReader reader, ref uint address)
		{
			uint nodePointer = reader.ReadUInt(address);
			int vertexCount = reader.ReadInt(address + 4);

			address += 8;
			MetaWeightVertex[] vertices = new MetaWeightVertex[vertexCount];
			for(int i = 0; i < vertexCount; i++)
			{
				vertices[i] = MetaWeightVertex.Read(reader, ref address);
			}

			return new(nodePointer, vertices);
		}


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is MetaWeightNode node &&
				   NodePointer == node.NodePointer &&
				   EqualityComparer<MetaWeightVertex[]>.Default.Equals(VertexWeights, node.VertexWeights);
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(NodePointer, VertexWeights);
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
			return $"{NodePointer:X8} - {VertexWeights.Length}";
		}
	}
}
