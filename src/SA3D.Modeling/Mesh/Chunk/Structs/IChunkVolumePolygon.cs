using Amicitia.IO.Binary;
using J113D.Json;
using System;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	/// <summary>
	/// Chunk volume polygon interface.
	/// </summary>
	[JsonConverter(typeof(InterfaceJsonSerializer<IChunkVolumePolygon>))]
	public interface IChunkVolumePolygon : ICloneable, IBinarySerializable<int>
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
	}
}
