using SA3D.Common.IO;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Base class for polygon chunks with a size header.
	/// </summary>
	public abstract class SizedChunk : PolyChunk
	{
		/// <summary>
		/// Amount of shorts in the chunk
		/// </summary>
		public abstract ushort Size { get; }

		/// <inheritdoc/>
		public sealed override uint ByteSize => (Size * 2u) + 4u;

		/// <summary>
		/// Base constructor for sized chunks.
		/// </summary>
		/// <param name="type"></param>
		public SizedChunk(PolyChunkType type) : base(type) { }

		/// <inheritdoc/>
		protected override void InternalWrite(EndianStackWriter writer)
		{
			writer.WriteUShort(Size);
		}
	}
}
