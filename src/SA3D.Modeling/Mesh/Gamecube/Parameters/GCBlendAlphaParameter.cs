using SA3D.Modeling.Mesh.Gamecube.Enums;

namespace SA3D.Modeling.Mesh.Gamecube.Parameters
{
	/// <summary>
	/// The blending information for the surface of the geometry
	/// </summary>
	public struct GCBlendAlphaParameter : IGCParameter
	{
		/// <summary>
		/// Blend alpha parameter with default values.
		/// </summary>
		public static readonly GCBlendAlphaParameter DefaultBlendParameter
			= new() { SourceAlpha = BlendMode.SrcAlpha, DestinationAlpha = BlendMode.SrcAlphaInverted };

		/// <inheritdoc/>
		public readonly GCParameterType Type => GCParameterType.BlendAlpha;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Source pixel blendmode.
		/// </summary>
		public BlendMode SourceAlpha
		{
			readonly get => (BlendMode)((Data >> 11) & 7);
			set => Data = (Data & 0xFFFFC7FF) | (((uint)value & 7) << 11);
		}

		/// <summary>
		/// Destination pixel blendmode.
		/// </summary>
		public BlendMode DestinationAlpha
		{
			readonly get => (BlendMode)((Data >> 8) & 7);
			set => Data = (Data & 0xFFFFF8FF) | (((uint)value & 7) << 8);
		}

		/// <summary>
		/// Whether to use blending.
		/// </summary>
		public bool UseAlpha
		{
			readonly get => (Data & 0x4000u) != 0;
			set => Data = (Data & ~0x4000u) | (value ? 0x4000u : 0);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Blendalpha: {UseAlpha} / {SourceAlpha} -> {DestinationAlpha}";
		}
	}
}
