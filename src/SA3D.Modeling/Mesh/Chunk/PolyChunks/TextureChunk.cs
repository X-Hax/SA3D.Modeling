using SA3D.Common.IO;
using System;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Contains texture information.
	/// </summary>
	public class TextureChunk : PolyChunk
	{
		/// <inheritdoc/>
		public override uint ByteSize => 4;

		/// <summary>
		/// Whether the chunktype is <see cref="PolyChunkType.TextureID2"/>.
		/// </summary>
		public bool Second
		{
			get => Type == PolyChunkType.TextureID2;
			set => Type = value ? PolyChunkType.TextureID2 : PolyChunkType.TextureID;
		}

		/// <summary>
		/// The mipmap distance multiplier.
		/// <br/> Ranges from 0 to 3.75f in increments of 0.25.
		/// </summary>
		public float MipmapDistanceMultiplier
		{
			get => (Attributes & 0xF) * 0.25f;
			set => Attributes = (byte)((Attributes & 0xF0) | (byte)Math.Max(0, Math.Min(0xF, Math.Round(value / 0.25, MidpointRounding.AwayFromZero))));
		}

		/// <summary>
		/// Clamps texture corrdinates on the vertical axis between -1 and 1.
		/// </summary>
		public bool ClampV
		{
			get => (Attributes & 0x10) != 0;
			set => _ = value ? Attributes |= 0x10 : Attributes &= 0xEF;
		}

		/// <summary>
		/// Clamps texture corrdinates on the horizontal axis between -1 and 1.
		/// </summary>
		public bool ClampU
		{
			get => (Attributes & 0x20) != 0;
			set => _ = value ? Attributes |= 0x20 : Attributes &= 0xDF;
		}

		/// <summary>
		/// Mirrors the texture every second time the texture is repeated along the vertical axis.
		/// </summary>
		public bool MirrorV
		{
			get => (Attributes & 0x40) != 0;
			set => _ = value ? Attributes |= 0x40 : Attributes &= 0xBF;
		}

		/// <summary>
		/// Mirrors the texture every second time the texture is repeated along the horizontal axis.
		/// </summary>
		public bool MirrorU
		{
			get => (Attributes & 0x80) != 0;
			set => _ = value ? Attributes |= 0x80 : Attributes &= 0x7F;
		}


		/// <summary>
		/// Second set of data bytes.
		/// </summary>
		public ushort Data { get; private set; }

		/// <summary>
		/// Texture ID to use.
		/// </summary>
		public ushort TextureID
		{
			get => (ushort)(Data & 0x1FFFu);
			set => Data = (ushort)((Data & ~0x1FFF) | Math.Min(value, (ushort)0x1FFF));
		}

		/// <summary>
		/// Whether to use super sampling (anisotropic filtering).
		/// </summary>
		public bool SuperSample
		{
			get => (Data & 0x2000) != 0;
			set => _ = value ? Data |= 0x2000 : Data &= 0xDFFF;
		}

		/// <summary>
		/// Texture pixel filtering mode.
		/// </summary>
		public FilterMode FilterMode
		{
			get => (FilterMode)(Data >> 14);
			set => Data = (ushort)((Data & ~0xC000) | ((ushort)value << 14));
		}


		/// <summary>
		/// Creates a new texture chunk.
		/// </summary>
		/// <param name="second">Whether it is <see cref="PolyChunkType.TextureID2"/></param>
		public TextureChunk(bool second = false) : base(second ? PolyChunkType.TextureID2 : PolyChunkType.TextureID) { }


		internal static TextureChunk Read(EndianStackReader data, uint address)
		{
			ushort header = data.ReadUShort(address);
			PolyChunkType type = (PolyChunkType)(header & 0xFF);
			byte attribs = (byte)(header >> 8);
			ushort cnkData = data.ReadUShort(address + 2);

			return new TextureChunk(type == PolyChunkType.TextureID2)
			{
				Attributes = attribs,
				Data = cnkData
			};
		}

		/// <inheritdoc/>
		protected override void InternalWrite(EndianStackWriter writer)
		{
			writer.WriteUShort(Data);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Type} - {TextureID}";
		}
	}
}
