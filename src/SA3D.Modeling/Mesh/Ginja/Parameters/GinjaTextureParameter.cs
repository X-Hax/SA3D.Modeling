using SA3D.Modeling.Mesh.Ginja.Enums;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Texture information for the geometry
	/// </summary>
	public struct GinjaTextureParameter : IGinjaParameter
	{
		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.Texture;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Texture index.
		/// </summary>
		public ushort TextureID
		{
			readonly get => (ushort)(Data & 0xFFFF);
			set => Data = (Data & 0xFFFF0000) | value;
		}

		/// <summary>
		/// Texture tiling properties.
		/// </summary>
		public GinjaTileMode Tiling
		{
			readonly get => (GinjaTileMode)(Data >> 16);
			set => Data = (Data & 0xFFFF) | ((uint)value << 16);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Texture: {TextureID} - {(uint)Tiling}";
		}
	}
}
