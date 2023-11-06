using SA3D.Modeling.Structs;
using System;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	/// <summary>
	/// Single vertex of a vertex chunk
	/// </summary>
	public struct ChunkVertex : IEquatable<ChunkVertex>
	{
		/// <summary>
		/// Position in 3D space.
		/// </summary>
		public Vector3 Position { get; set; }

		/// <summary>
		/// Normalized direction.
		/// </summary>
		public Vector3 Normal { get; set; }

		/// <summary>
		/// Diffuse Color.
		/// </summary>
		public Color Diffuse { get; set; }

		/// <summary>
		/// Specular color.
		/// </summary>
		public Color Specular { get; set; }

		/// <summary>
		/// Additonal Attributes.
		/// </summary>
		public uint Attributes { get; set; }

		/// <summary>
		/// Vertex cache index.
		/// </summary>
		public ushort Index
		{
			readonly get => (ushort)(Attributes & 0xFFFF);
			set => Attributes = (Attributes & ~0xFFFFu) | value;
		}

		/// <summary>
		/// Node influence.
		/// </summary>
		public float Weight
		{
			readonly get => (Attributes >> 16) / 255f;
			set => Attributes = (Attributes & 0xFFFFF) | ((uint)Math.Round(value * 255f) << 16);
		}


		/// <summary>
		/// Creates a chunk vertex with a normal.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="normal">Normalized direction.</param>
		public ChunkVertex(Vector3 position, Vector3 normal) : this()
		{
			Position = position;
			Normal = normal;
			Attributes = 0;
			Weight = 1;
		}

		/// <summary>
		/// Creates a chunk vertex with a normal and attributes.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="normal">Normalized direction.</param>
		/// <param name="attribs">Additional attributes.</param>
		public ChunkVertex(Vector3 position, Vector3 normal, uint attribs) : this()
		{
			Position = position;
			Normal = normal;
			Attributes = attribs;
			Weight = 1;
		}

		/// <summary>
		/// Creates a chunk vertex with a normal and weight info.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="normal">Normalized direction.</param>
		/// <param name="index">Vertex cache index.</param>
		/// <param name="weight">Node influence.</param>
		public ChunkVertex(Vector3 position, Vector3 normal, ushort index, float weight) : this()
		{
			Position = position;
			Normal = normal;
			Index = index;
			Weight = weight;
		}

		/// <summary>
		/// Creates a chunk with colors.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="diffuse">Diffuse color.</param>
		/// <param name="specular">Specular color.</param>
		public ChunkVertex(Vector3 position, Color diffuse, Color specular) : this()
		{
			Position = position;
			Diffuse = diffuse;
			Specular = specular;
		}


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is ChunkVertex vertex &&
				   Position.Equals(vertex.Position) &&
				   Normal.Equals(vertex.Normal) &&
				   Diffuse.Equals(vertex.Diffuse) &&
				   Specular.Equals(vertex.Specular) &&
				   Attributes == vertex.Attributes;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(Position, Normal, Diffuse, Specular, Attributes);
		}

		/// <inheritdoc/>
		readonly bool IEquatable<ChunkVertex>.Equals(ChunkVertex other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two chunk vertices for equality.
		/// </summary>
		/// <param name="left">Lefthand vertex.</param>
		/// <param name="right">Rigthand vertex.</param>
		/// <returns>Whether the vertices are equal.</returns>
		public static bool operator ==(ChunkVertex left, ChunkVertex right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two chunk vertices for inequality.
		/// </summary>
		/// <param name="left">Lefthand vertex.</param>
		/// <param name="right">Rigthand vertex.</param>
		/// <returns>Whether the vertices are inequal.</returns>
		public static bool operator !=(ChunkVertex left, ChunkVertex right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{Position.DebugString()}, {Normal.DebugString()} : {Index}, {Weight:F3}";
		}

	}
}
