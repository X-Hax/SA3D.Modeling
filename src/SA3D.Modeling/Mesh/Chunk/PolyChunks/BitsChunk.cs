using SA3D.Common.Ascii;
using SA3D.Modeling.ObjectData;

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

		/// <inheritdoc/>
		public override void Write(AsciiWriter writer, ModelAsciiContext context)
		{
			base.Write(writer, context);
			writer.WriteLine();
		}

	}
}
