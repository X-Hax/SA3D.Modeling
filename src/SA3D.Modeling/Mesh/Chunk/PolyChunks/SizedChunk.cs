using Amicitia.IO.Binary;

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
		protected override bool AlignWithFour => true;

		/// <summary>
		/// Base constructor for sized chunks.
		/// </summary>
		/// <param name="type"></param>
		public SizedChunk(PolyChunkType type) : base(type) { }

		/// <inheritdoc/>
		public override void Read(BinaryObjectReader reader)
		{
			base.Read(reader);
			reader.Skip(sizeof(ushort));
		}

		/// <inheritdoc/>
		protected override void WriteData(BinaryObjectWriter writer)
		{
			base.WriteData(writer);
			writer.WriteUInt16(Size);
		}
	}
}
