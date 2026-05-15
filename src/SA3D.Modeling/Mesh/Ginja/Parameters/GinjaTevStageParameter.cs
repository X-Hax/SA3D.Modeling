using SA3D.Modeling.Mesh.Ginja.Enums;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Unknown but not unused.
	/// </summary>
	public struct GinjaTevStageParameter : IGinjaParameter
	{
		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.TevStage;

		/// <inheritdoc/>
		public uint Data { get; set; }


		/// <summary>
		/// Tev stage to use
		/// </summary>
		public GinjaTevStageID TevStage
		{
			readonly get => (GinjaTevStageID)((Data >> 12) & 0xF);
			set => Data = (Data & ~0xFu) | byte.Clamp((byte)value, 0, 0xF);
		}

		/// <summary>
		/// Texcoord id to use
		/// </summary>
		public GinjaTexCoordID TexCoord
		{
			readonly get
			{
				byte value = (byte)((Data >> 8) & 0xF);
				return value < 8 ? (GinjaTexCoordID)value : GinjaTexCoordID.TexCoordNull;
			}
			set
			{
				byte val = value is >= GinjaTexCoordID.TexCoordMax ? (byte)GinjaTexCoordID.TexCoordNull : (byte)value;
				Data = (Data & ~0xF00u) | ((uint)val << 8);
			}
		}

		/// <summary>
		/// Texmap to use. Setting this to <see cref="GinjaTexMapID.TexMapNull"/> will clear the tev stage instead of "modulo-ing" to it.
		/// </summary>
		public GinjaTexMapID TexMap
		{
			readonly get
			{
				byte value = (byte)((Data >> 4) & 0xF);
				return value < 8 ? (GinjaTexMapID)value : GinjaTexMapID.TexMapNull;
			}
			set
			{
				byte val = value is >= GinjaTexMapID.TexMapMax ? (byte)GinjaTexMapID.TexMapNull : (byte)value;
				Data = (Data & ~0xF0u) | ((uint)val << 4);
			}
		}

		/// <summary>
		/// Color channel to write to.
		/// </summary>
		public GinjaColorChannelID ColorChannel
		{
			readonly get => (GinjaColorChannelID)(Data & 0xF);
			set => Data = (Data & ~0xFu) | byte.Clamp((byte)value, 0, 0xF);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Unknown: {TevStage} - {TexCoord} - {TexMap} - {ColorChannel}";
		}
	}
}
