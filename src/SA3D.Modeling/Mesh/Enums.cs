namespace SA3D.Modeling.Mesh
{
	/// <summary>
	/// The different available Attach formats.
	/// </summary>
	public enum AttachFormat
	{
		/// <summary>
		/// Buffer format; Exclusive to this library.
		/// </summary>
		Buffer,

		/// <summary>
		/// BASIC format.
		/// </summary>
		BASIC,

		/// <summary>
		/// CHUNK format.
		/// </summary>
		CHUNK,

		/// <summary>
		/// GC format.
		/// </summary>
		GC
	}

	/// <summary>
	/// Transparency blending
	/// </summary>
	public enum BlendMode : byte
	{
		/// <summary>
		/// Blend factor of (0, 0, 0, 0).
		/// <br/> D3DBLEND_ZERO
		/// </summary>
		Zero = 0,

		/// <summary>
		/// Blend factor of (1, 1, 1, 1).
		/// <br/> D3DBLEND_ONE
		/// </summary>
		One = 1,

		/// <summary>
		/// Blend factor of (Rs; Gs; Bs; As).
		/// <br/> D3DBLEND_SRCCOLOR
		/// </summary>
		Other = 2,

		/// <summary>
		/// Blend factor of (1 - Rs, 1- Gs, 1- Bs, 1- As).
		/// <br/> D3DBLEND_INVSRCCOLOR
		/// </summary>
		OtherInverted = 3,

		/// <summary>
		/// Blend factor of (As, As, As, As).
		/// <br/> D3DBLEND_SRCALPHA
		/// </summary>
		SrcAlpha = 4,

		/// <summary>
		/// Blend factor of (1 - As, 1 - As, 1 - As, 1 - As).
		/// <br/> D3DBLEND_INVSRCALPHA
		/// </summary>
		SrcAlphaInverted = 5,

		/// <summary>
		/// Blend factor of (Ad, Ad, Ad, Ad).
		/// <br/> D3DBLEND_DESTALPHA
		/// </summary>
		DstAlpha = 6,

		/// <summary>
		/// Blend factor of (1 - Ad, 1 - Ad, 1 - Ad, 1 - Ad).
		/// <br/> D3DBLEND_INVDESTALPHA
		/// </summary>
		DstAlphaInverted = 7
	}

	/// <summary>
	/// Texture filtering modes.
	/// </summary>
	public enum FilterMode
	{
		/// <summary>
		/// Samples the nearest pixel only.
		/// </summary>
		Nearest = 0,

		/// <summary>
		/// Linearly interpolates between the pixels.
		/// </summary>
		Bilinear = 1,

		/// <summary>
		/// Linearly interpolates between the pixels and mitpmaps.
		/// </summary>
		Trilinear = 2,

		/// <summary>
		/// Mix between bilinear and trilinear (?).
		/// </summary>
		Blend = 3,
	}
}
