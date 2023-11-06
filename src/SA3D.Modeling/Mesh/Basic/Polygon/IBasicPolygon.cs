using SA3D.Common.IO;
using System;
using System.Collections.Generic;

namespace SA3D.Modeling.Mesh.Basic.Polygon
{
	/// <summary>
	/// BASIC polygon interface.
	/// </summary>
	public interface IBasicPolygon : ICloneable, IEnumerable<ushort>
	{
		/// <summary>
		/// Size of the primitive in bytes.
		/// </summary>
		public uint Size { get; }

		/// <summary>
		/// Number of indices in the polygon.
		/// </summary>
		public int NumIndices { get; }

		/// <summary>
		/// Access and set vertex indices of the polygon.
		/// </summary>
		/// <param name="index">The index of the corner.</param>
		/// <returns>The vertex index.</returns>
		public ushort this[int index] { get; set; }


		/// <summary>
		/// Reads a primitive off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which the primitive is located.</param>
		/// <param name="type">Type of primitive to read.</param>
		/// <returns>The read primitive</returns>
		/// <exception cref="ArgumentException"></exception>
		public static IBasicPolygon Read(EndianStackReader reader, ref uint address, BasicPolygonType type)
		{
			return type switch
			{
				BasicPolygonType.Triangles => BasicTriangle.Read(reader, ref address),
				BasicPolygonType.Quads => BasicQuad.Read(reader, ref address),
				BasicPolygonType.NPoly or BasicPolygonType.TriangleStrips => BasicMultiPolygon.Read(reader, ref address),
				_ => throw new ArgumentException("Unknown poly type!", nameof(type)),
			};
		}

		/// <summary>
		/// Writes the polygon to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public void Write(EndianStackWriter writer);
	}
}
