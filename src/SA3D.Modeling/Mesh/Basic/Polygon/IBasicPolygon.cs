using Amicitia.IO.Binary;
using System;
using System.Collections.Generic;

namespace SA3D.Modeling.Mesh.Basic.Polygon
{
	/// <summary>
	/// BASIC interface for storing polygon information. Read/Write are only responsible for writing vertex index structures
	/// </summary>
	public interface IBasicPolygon : ICloneable, IEnumerable<ushort>, IBinarySerializable
	{
		/// <summary>
		/// Size of the primitive (only vertex indices) in bytes.
		/// </summary>
		public uint Size { get; }

		/// <summary>
		/// Number of vertices in the polygon.
		/// </summary>
		public int NumIndices { get; }

		/// <summary>
		/// Access and set polygon corners of the polygon.
		/// </summary>
		/// <param name="index">The index of the corner.</param>
		/// <returns>The vertex index.</returns>
		public ushort this[int index] { get; set; }


		/// <summary>
		/// Reads a primitive off an endian stack reader.
		/// </summary>
		/// <param name="type">Type of primitive to read.</param>
		/// <returns>The read primitive</returns>
		/// <exception cref="ArgumentException"></exception>
		public static Func<BinaryObjectReader, IBasicPolygon> GetReader(BasicPolygonType type)
		{
			return type switch
			{
				BasicPolygonType.Triangles => (reader) => reader.ReadObject<BasicTriangle>(),
				BasicPolygonType.Quads => (reader) => reader.ReadObject<BasicQuad>(),
				BasicPolygonType.NPoly or BasicPolygonType.TriangleStrips => (reader) => reader.ReadObject<BasicMultiPolygon>(),
				_ => throw new ArgumentException("Unknown poly type!", nameof(type)),
			};
		}
	}
}
