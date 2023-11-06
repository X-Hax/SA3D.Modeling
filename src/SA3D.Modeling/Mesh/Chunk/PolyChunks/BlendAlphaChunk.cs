namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Sets the blendmode of the following strip chunks.
	/// </summary>
	public class BlendAlphaChunk : BitsChunk
	{
		/// <summary>
		/// Source blendmode.
		/// </summary>
		public BlendMode SourceAlpha
		{
			get => (BlendMode)((Attributes >> 3) & 7);
			set => Attributes = (byte)((Attributes & ~0x38) | ((byte)value << 3));
		}

		/// <summary>
		/// Destination blendmode.
		/// </summary>
		public BlendMode DestinationAlpha
		{
			get => (BlendMode)(Attributes & 7);
			set => Attributes = (byte)((Attributes & ~7) | (byte)value);
		}

		/// <summary>
		/// Creates a new blendalpha chunk.
		/// </summary>
		public BlendAlphaChunk() : base(PolyChunkType.BlendAlpha) { }

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"BlendAlpha - {SourceAlpha} -> {DestinationAlpha}";
		}
	}
}
