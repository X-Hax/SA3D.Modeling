using SA3D.Common.Ascii;
using SA3D.Modeling.ObjectData.Structs;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Base class for poly chunks with no body.
	/// </summary>
	public abstract class BitsChunk : PolyChunk
	{
		/// <inheritdoc/>
		public override bool AlignWithFour => false;

		/// <summary>
		/// Base constructor for bits chunks.
		/// </summary>
		/// <param name="type"></param>
		protected BitsChunk(PolyChunkType type) : base(type) { }

		/// <inheritdoc/>
		protected override void Write(AsciiWriter writer, ModelAsciiIOContext context)
		{
			base.Write(writer, context);
			writer.WriteLine();
		}

	}
}
