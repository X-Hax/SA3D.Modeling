using SA3D.Common.IO;
using SA3D.Modeling.Structs;
using System;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Buffer
{
	/// <summary>
	/// A single corner in a triangle
	/// </summary>
	public struct BufferCorner : IEquatable<BufferCorner>
	{
		/// <summary>
		/// Size of a buffer corner in bytes.
		/// </summary>
		public const uint StructSize = 14;

		/// <summary>
		/// Size of a buffer corner without color in bytes.
		/// </summary>
		public const uint StructSizeNoColor = 10;

		/// <summary>
		/// Vertex cache index to use.
		/// </summary>
		public ushort VertexIndex { get; set; }

		/// <summary>
		/// Color.
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		/// Coordinates for texture rendering.
		/// </summary>
		public Vector2 Texcoord { get; set; }


		/// <summary>
		/// Creates a new buffer corner.
		/// </summary>
		/// <param name="vertexIndex">Buffer array index of the vertex</param>
		/// <param name="color">Color.</param>
		/// <param name="texcoord">Coordinates for texture rendering.</param>
		public BufferCorner(ushort vertexIndex, Color color, Vector2 texcoord)
		{
			VertexIndex = vertexIndex;
			Color = color;
			Texcoord = texcoord;
		}

		/// <summary>
		/// Creates a new white buffer corner with no texture coordinates.
		/// </summary>
		/// <param name="vertexIndex">Buffer array index of the vertex</param>
		public BufferCorner(ushort vertexIndex)
		{
			VertexIndex = vertexIndex;
			Color = BufferMesh.DefaultColor;
			Texcoord = Vector2.Zero;
		}


		/// <summary>
		/// Writes the buffer corner to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="writeColor">Whether to write the corners color too.</param>
		public readonly void Write(EndianStackWriter writer, bool writeColor)
		{
			writer.WriteUShort(VertexIndex);
			writer.WriteVector2(Texcoord);
			if(writeColor)
			{
				writer.WriteColor(Color, ColorIOType.RGBA8);
			}
		}

		/// <summary>
		/// Reads a buffer corner off an endian stack reader. Advances the address by the number of bytes read.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="hasColor">Whether the corner contains a color.</param>
		/// <returns>The corner that was read.</returns>
		public static BufferCorner Read(EndianStackReader reader, ref uint address, bool hasColor)
		{
			ushort index = reader.ReadUShort(address);
			address += 2;
			Vector2 texcoord = reader.ReadVector2(ref address);
			Color col = hasColor ? reader.ReadColor(ref address, ColorIOType.RGBA8) : BufferMesh.DefaultColor;

			return new BufferCorner(index, col, texcoord);
		}


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is BufferCorner corner &&
				   VertexIndex == corner.VertexIndex &&
				   Color == corner.Color &&
				   Texcoord == corner.Texcoord;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return System.HashCode.Combine(VertexIndex, Color, Texcoord);
		}

		/// <inheritdoc/>
		readonly bool IEquatable<BufferCorner>.Equals(BufferCorner other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two corners for equality.
		/// </summary>
		/// <param name="l">Lefthand corners.</param>
		/// <param name="r">Righthand corners.</param>
		/// <returns>Whether the two corners are equal.</returns>
		public static bool operator ==(BufferCorner l, BufferCorner r)
		{
			return l.Equals(r);
		}

		/// <summary>
		/// Compares two corners for inequality.
		/// </summary>
		/// <param name="l">Lefthand corners.</param>
		/// <param name="r">Righthand corners.</param>
		/// <returns>Whether the two corners are inequal.</returns>
		public static bool operator !=(BufferCorner l, BufferCorner r)
		{
			return !l.Equals(r);
		}


		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{VertexIndex}: \t{Color}; \t{Texcoord.DebugString()}";
		}
	}
}
