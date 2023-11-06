using System;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Sets the specular exponent of the following strip chunks
	/// </summary>
	public class SpecularExponentChunk : BitsChunk
	{
		/// <summary>
		/// Specular exponent <br/>
		/// Ranges from 0 to 16
		/// </summary>
		public byte SpecularExponent
		{
			get => (byte)(Attributes & 0x1F);
			set => Attributes = (byte)((Attributes & ~0x1F) | Math.Min(value, (byte)16));
		}

		/// <summary>
		/// Creates a new Specular exponent chunk.
		/// </summary>
		public SpecularExponentChunk() : base(PolyChunkType.SpecularExponent) { }

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Specular Exponent - {SpecularExponent}";
		}
	}
}
