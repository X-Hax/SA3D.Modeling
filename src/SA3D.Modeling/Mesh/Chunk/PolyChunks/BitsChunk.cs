namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Base class for poly chunks with no body.
	/// </summary>
	public abstract class BitsChunk : PolyChunk
	{
		/// <inheritdoc/>
		protected override bool AlignWithFour => false;

		/// <summary>
		/// Base constructor for bits chunks.
		/// </summary>
		/// <param name="type"></param>
		protected BitsChunk(PolyChunkType type) : base(type) { }

	}
}
