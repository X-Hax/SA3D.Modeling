using SA3D.Common.IO;
using SA3D.Modeling.Structs;
using System;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Buffer
{
	/// <summary>
	/// A single point in space with a direction and weight.
	/// </summary>
	public struct BufferVertex : IEquatable<BufferVertex>
	{
		/// <summary>
		/// Position in 3D space.
		/// </summary>
		public Vector3 Position { get; set; }

		/// <summary>
		/// Normalized direction that the vertex if facing in.
		/// </summary>
		public Vector3 Normal { get; set; }

		/// <summary>
		/// Index in the vertex cache that this vertex occupies.
		/// </summary>
		public ushort Index { get; set; }

		/// <summary>
		/// Influence of the assigned node on the vertices position and direction.
		/// </summary>
		public float Weight { get; set; }


		/// <summary>
		/// Creates a new buffer vertex with default normal and full weight.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="index">Index in the buffer array that this vertex occupies.</param>
		public BufferVertex(Vector3 position, ushort index)
		{
			Position = position;
			Normal = BufferMesh.DefaultNormal;
			Index = index;
			Weight = 1;
		}

		/// <summary>
		/// Creates a new buffer vertex with full weight.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="normal">Normalized direction that the vertex if facing in.</param>
		/// <param name="index">Index in the buffer array that this vertex occupies.</param>
		public BufferVertex(Vector3 position, Vector3 normal, ushort index)
		{
			Position = position;
			Normal = normal;
			Index = index;
			Weight = 1;
		}

		/// <summary>
		/// Creates a new buffer vertex.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="normal">Normalized direction that the vertex if facing in.</param>
		/// <param name="index">Index in the buffer array that this vertex occupies.</param>
		/// <param name="weight">Influence of the assigned node on the vertices position and direction.</param>
		public BufferVertex(Vector3 position, Vector3 normal, ushort index, float weight)
		{
			Position = position;
			Normal = normal;
			Index = index;
			Weight = weight;
		}



		/// <summary>
		/// Writes the vertex to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="writeNormal">Whether the normal should be written too.</param>
		public readonly void Write(EndianStackWriter writer, bool writeNormal)
		{
			writer.WriteUShort(Index);
			writer.WriteUShort((ushort)(Weight * ushort.MaxValue));

			writer.WriteVector3(Position);
			if(writeNormal)
			{
				writer.WriteVector3(Normal);
			}
		}

		/// <summary>
		/// Reads a buffer vertex off an endian stack reader. Advances the address by the number of bytes read.
		/// </summary>
		/// <param name="reader">Byte source</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="hasNormal">Whether the vertex contains a normal.</param>
		/// <returns>The vertex that was read</returns>
		public static BufferVertex Read(EndianStackReader reader, ref uint address, bool hasNormal)
		{
			const float WeightFactor = 1f / ushort.MaxValue;

			BufferVertex result = default;

			result.Index = reader.ReadUShort(address);
			result.Weight = reader.ReadUShort(address + 2) * WeightFactor;
			address += 4;
			result.Position = reader.ReadVector3(ref address);
			result.Normal = hasNormal ? reader.ReadVector3(ref address) : BufferMesh.DefaultNormal;

			return result;
		}


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is BufferVertex vertex &&
				   Position.Equals(vertex.Position) &&
				   Normal.Equals(vertex.Normal) &&
				   Index == vertex.Index &&
				   Weight == vertex.Weight;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(Position, Normal, Index, Weight);
		}

		/// <inheritdoc/>
		public readonly bool Equals(BufferVertex other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two vertices for equality.
		/// </summary>
		/// <param name="left">Lefthand vertex.</param>
		/// <param name="right">Righthand vertex.</param>
		/// <returns>Whether the two vertices are equal.</returns>
		public static bool operator ==(BufferVertex left, BufferVertex right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two vertices for inequality.
		/// </summary>
		/// <param name="left">Lefthand vertex.</param>
		/// <param name="right">Righthand vertex.</param>
		/// <returns>Whether the two vertices are inequal.</returns>
		public static bool operator !=(BufferVertex left, BufferVertex right)
		{
			return !(left == right);
		}


		/// <summary>
		/// Adds the position and normal of two vertices together. 
		/// <br/> Index is taken from the lefthand vertex,
		/// <br/> Weight is always 1.
		/// </summary>
		/// <param name="l">Lefthand vertex.</param>
		/// <param name="r">Righthand vertex.</param>
		/// <returns></returns>
		public static BufferVertex operator +(BufferVertex l, BufferVertex r)
		{
			return new BufferVertex()
			{
				Position = l.Position + r.Position,
				Normal = l.Normal + r.Normal,
				Index = l.Index,
				Weight = 1
			};
		}

		/// <summary>
		/// Multiplies position and normal of a vertex by a value.
		/// </summary>
		/// <param name="l">Vertex to multiply.</param>
		/// <param name="r">Value to multiply by.</param>
		/// <returns></returns>
		public static BufferVertex operator *(BufferVertex l, float r)
		{
			return new BufferVertex()
			{
				Position = l.Position * r,
				Normal = l.Normal * r,
				Index = l.Index,
				Weight = l.Weight
			};
		}

		/// <summary>
		/// Multiplies position and normal of a vertex by a value.
		/// </summary>
		/// <param name="l">Value to multiply by.</param>
		/// <param name="r">Vertex to multiply.</param>
		public static BufferVertex operator *(float l, BufferVertex r)
		{
			return r * l;
		}


		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return Weight == 1.0f
				? $"{Index}: {Position.DebugString()}; {Normal.DebugString()}"
				: $"{Index}: {Position.DebugString()}; {Normal.DebugString()}; {Weight}";
		}
	}
}
