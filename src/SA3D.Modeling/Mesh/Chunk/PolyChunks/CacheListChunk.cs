namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Caches the succeeding polygon chunks of the same attach into specified index.
	/// </summary>
	public class CacheListChunk : BitsChunk
	{
		/// <summary>
		/// Cache ID.
		/// </summary>
		public byte List
		{
			get => Attributes;
			set => Attributes = value;
		}

		/// <summary>
		/// Creates a new cache list chunk.
		/// </summary>
		public CacheListChunk() : base(PolyChunkType.CacheList) { }

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Cache list - {List}";
		}
	}
}
