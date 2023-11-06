using SA3D.Modeling.Mesh.Gamecube.Enums;

namespace SA3D.Modeling.Mesh.Gamecube.Parameters
{
	/// <summary>
	/// Texture information for the geometry
	/// </summary>
	public struct GCTextureParameter : IGCParameter
	{
		/// <inheritdoc/>
		public readonly GCParameterType Type => GCParameterType.Texture;

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
		public GCTileMode Tiling
		{
			readonly get => (GCTileMode)(Data >> 16);
			set => Data = (Data & 0xFFFF) | ((uint)value << 16);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Texture: {TextureID} - {(uint)Tiling}";
		}
	}
}
