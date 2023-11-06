using System;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Adjusts the mipmap distance of the following strip chunks
	/// </summary>
	public class MipmapDistanceMultiplierChunk : BitsChunk
	{
		/// <summary>
		/// The mipmap distance multiplier <br/>
		/// Ranges from 0 to 3.75f in increments of 0.25
		/// </summary>
		public float MipmapDistanceMultiplier
		{
			get => (Attributes & 0xF) * 0.25f;
			set => Attributes = (byte)((Attributes & 0xF0) | (byte)Math.Max(0, Math.Min(0xF, Math.Round(value / 0.25, MidpointRounding.AwayFromZero))));
		}

		/// <summary>
		/// Creates a new mipmap distance multiplier chunk.
		/// </summary>
		public MipmapDistanceMultiplierChunk() : base(PolyChunkType.MipmapDistanceMultiplier) { }

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"MMDM - {MipmapDistanceMultiplier}";
		}
	}
}
