namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Draws the polygon chunks cached by a specific index.
	/// </summary>
	public class DrawListChunk : BitsChunk
	{
		/// <summary>
		/// Cache ID
		/// </summary>
		public byte List
		{
			get => Attributes;
			set => Attributes = value;
		}

		/// <summary>
		/// Creates a new draw list chunk.
		/// </summary>
		public DrawListChunk() : base(PolyChunkType.DrawList) { }

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Draw List - {List}";
		}
	}
}
