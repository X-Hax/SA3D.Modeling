

using Amicitia.IO.Binary;
using SA3D.Modeling.Structs;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Polychunk with unknown usage.
	/// </summary>
	public class MaterialBumpChunk : SizedChunk
	{
		/// <inheritdoc/>
		public override ushort Size => 6;

		/// <summary>
		/// "direction" vector
		/// </summary>
		public Vector3 Dir { get; set; }

		/// <summary>
		/// "up" vector
		/// </summary>
		public Vector3 Up { get; set; }


		/// <summary>
		/// Creates a new material bump chunk.
		/// </summary>
		public MaterialBumpChunk() : base(PolyChunkType.Material_Bump) { }

		/// <inheritdoc/>
		public override void Read(BinaryObjectReader reader)
		{
			base.Read(reader);

			Dir = reader.ReadVector3(FloatIOType.NormalizedShort);
			Up = reader.ReadVector3(FloatIOType.NormalizedShort);
		}

		/// <inheritdoc/>
		protected override void WriteData(BinaryObjectWriter writer)
		{
			base.WriteData(writer);

			writer.WriteVector3(Dir, FloatIOType.NormalizedShort);
			writer.WriteVector3(Up, FloatIOType.NormalizedShort);
		}
	}
}
