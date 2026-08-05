using Amicitia.IO.Binary;
using SA3D.Common.Ascii;
using SA3D.Modeling.ObjectData.Structs;

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
		public override bool AlignWithFour => true;

		/// <summary>
		/// Base constructor for sized chunks.
		/// </summary>
		/// <param name="type"></param>
		public SizedChunk(PolyChunkType type) : base(type) { }

		/// <inheritdoc/>
		protected override void Read(BinaryObjectReader reader)
		{
			base.Read(reader);
			reader.Skip(sizeof(ushort));
		}

		/// <inheritdoc/>
		protected override void Write(BinaryObjectWriter writer)
		{
			base.Write(writer);
			writer.WriteUInt16(Size);
		}

		/// <inheritdoc/>
		protected override void Write(AsciiWriter writer, ModelAsciiIOContext context)
		{
			base.Write(writer, context);
			writer.Write($" {Size},");
		}
	}
}
