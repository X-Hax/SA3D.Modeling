using SA3D.Common.IO;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Base class for poly chunks with no body.
	/// </summary>
	public abstract class BitsChunk : PolyChunk
	{
		/// <inheritdoc/>
		public override uint ByteSize => 2;

		/// <summary>
		/// Base constructor for bits chunks.
		/// </summary>
		/// <param name="type"></param>
		protected BitsChunk(PolyChunkType type) : base(type) { }

		/// <inheritdoc/>
		protected override void InternalWrite(EndianStackWriter writer) { }
	}
}
