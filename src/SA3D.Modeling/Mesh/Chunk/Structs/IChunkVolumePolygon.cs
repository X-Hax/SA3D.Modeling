using SA3D.Common.IO;
using System;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	/// <summary>
	/// Chunk volume polygon interface.
	/// </summary>
	public interface IChunkVolumePolygon : ICloneable
	{
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
		/// Calculates the size of the polygon in bytes.
		/// </summary>
		/// <param name="polygonAttributeCount">Number of attributes for every polygon.</param>
		/// <returns>The size of the polygon in bytes</returns>
		public ushort Size(int polygonAttributeCount);

		/// <summary>
		/// Write the polygon to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="polygonAttributeCount">Number of attributes for every polygon to write.</param>
		public abstract void Write(EndianStackWriter writer, int polygonAttributeCount);
	}
}
