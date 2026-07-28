namespace SA3D.Modeling.Mesh.Ginja.Enums
{
	/// <summary>
	/// Output slot to which calculated texture coordinates should be written to.
	/// </summary>
	public enum GinjaTexCoordID : byte
	{
		/// <summary>
		/// First slot.
		/// </summary>
		TexCoord0 = 0,

		/// <summary>
		/// First slot.
		/// </summary>
		TexCoord1 = 1,

		/// <summary>
		/// Second slot.
		/// </summary>
		TexCoord2 = 2,

		/// <summary>
		/// Third slot.
		/// </summary>
		TexCoord3 = 3,

		/// <summary>
		/// Fourth slot.
		/// </summary>
		TexCoord4 = 4,

		/// <summary>
		/// Fifth slot.
		/// </summary>
		TexCoord5 = 5,

		/// <summary>
		/// Sixth slot.
		/// </summary>
		TexCoord6 = 6,

		/// <summary>
		/// Seventh slot.
		/// </summary>
		TexCoord7 = 7,

		/// <summary>
		/// Maximum available slot.
		/// </summary>
		TexCoordMax = 8,

		/// <summary>
		/// No slot.
		/// </summary>
		TexCoordNull = 0xFF,
	}
}
