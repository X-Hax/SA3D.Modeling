using SA3D.Modeling.Mesh.Ginja.Enums;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Holds lighting information
	/// </summary>
	public struct GinjaStripFlagsParameter : IGinjaParameter
	{
		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.StripFlags;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Number of output channels to be used.
		/// <br/> Ranges from 0 - 2
		/// </summary>
		public byte ChannelCount
		{
			readonly get => (byte)(Data & 0x3);
			set => Data = (Data & ~0x3u) | byte.Clamp(value, 0, 2);
		}

		/// <summary>
		/// Number of <see cref="GinjaTexGenParameter"/> used.
		/// <br/> Ranges from 0 - 15
		/// </summary>
		public byte TexGenCount
		{
			readonly get => (byte)((Data >> 4) & 0xF);
			set => Data = (Data & ~0xF0u) | (uint)(byte.Clamp(value, 0, 15) << 4);
		}

		/// <summary>
		/// Enables fullbright (no diffuse lighting &amp; ambient light set to white. Priority over <see cref="IgnoreAmbient"/>)
		/// </summary>
		public bool IgnoreLight
		{
			readonly get => GetFlag(0x100u);
			set => SetFlag(0x100, value);
		}

		/// <summary>
		/// Ignores specular lighting.
		/// </summary>
		public bool IgnoreSpecular
		{
			readonly get => GetFlag(0x200);
			set => SetFlag(0x200, value);
		}

		/// <summary>
		/// Ignores ambient lighting.
		/// </summary>
		public bool IgnoreAmbient
		{
			readonly get => GetFlag(0x400);
			set => SetFlag(0x400, value);
		}

		/// <summary>
		/// Use vertex colors for diffuse color, instead of from <see cref="GinjaDiffuseColorParameter"/>
		/// </summary>
		public bool UseVertexColorForDiffuse
		{
			readonly get => GetFlag(0x800);
			set => SetFlag(0x800, value);
		}

		/// <summary>
		/// Use vertex colors for ambient color, instead of from <see cref="GinjaAmbientColorParameter"/>
		/// </summary>
		public bool UseVertexColorForAmbient
		{
			readonly get => GetFlag(0x1000);
			set => SetFlag(0x1000, value);
		}

		/// <summary>
		/// Enables transparency
		/// </summary>
		public bool UseAlpha
		{
			readonly get => GetFlag(0x2000);
			set => SetFlag(0x2000, value);
		}

		/// <summary>
		/// Disables punchthrough rendering
		/// </summary>
		public bool NoPunchThrough
		{
			readonly get => GetFlag(0x4000);
			set => SetFlag(0x4000, value);
		}

		/// <summary>
		/// Disables backface culling.
		/// </summary>
		public bool DoubleSided
		{
			readonly get => GetFlag(0x8000);
			set => SetFlag(0x8000, value);
		}

		/// <summary>
		/// Number of TevStages used.
		/// <br/> Ranges from 0 - 15
		/// </summary>
		public byte TevStageCount
		{
			readonly get => (byte)((Data >> 16) & 0xF);
			set => Data = (Data & ~0xF0000u) | (uint)(byte.Clamp(value, 0, 15) << 16);
		}


		private readonly bool GetFlag(uint mask)
		{
			return (Data & mask) != 0;
		}

		private void SetFlag(uint mask, bool value)
		{
			if(value)
			{
				Data |= mask;
			}
			else
			{
				Data &= ~mask;
			}
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			string flagString =
				(IgnoreLight ? "X" : "-")
				+ (IgnoreSpecular ? 'X' : '-')
				+ (IgnoreAmbient ? 'X' : '-')
				+ (UseVertexColorForDiffuse ? 'X' : '-')
				+ "_"
				+ (UseVertexColorForAmbient ? 'X' : '-')
				+ (UseAlpha ? 'X' : '-')
				+ (NoPunchThrough ? 'X' : '-')
				+ (DoubleSided ? 'X' : '-');

			return $"Strip flags: {ChannelCount} - {TexGenCount} - {TevStageCount} - {flagString}";
		}
	}
}
