namespace SA3D.Modeling.Mesh.Gamecube.Enums
{
	/// <summary>
	/// Output channel to which calculated texture coordinates should be written to.
	/// </summary>
	public enum GCTexCoordID : byte
	{
		/// <summary>
		/// First channel.
		/// </summary>
		TexCoord0 = 0,

		/// <summary>
		/// First channel.
		/// </summary>
		TexCoord1 = 1,

		/// <summary>
		/// Second channel.
		/// </summary>
		TexCoord2 = 2,

		/// <summary>
		/// Third channel.
		/// </summary>
		TexCoord3 = 3,

		/// <summary>
		/// Fourth channel.
		/// </summary>
		TexCoord4 = 4,

		/// <summary>
		/// Fifth channel.
		/// </summary>
		TexCoord5 = 5,

		/// <summary>
		/// Sixth channel.
		/// </summary>
		TexCoord6 = 6,

		/// <summary>
		/// Seventh channel.
		/// </summary>
		TexCoord7 = 7,

		/// <summary>
		/// Maximum available channel.
		/// </summary>
		TexCoordMax = 8,

		/// <summary>
		/// No channel.
		/// </summary>
		TexCoordNull = 0xFF,
	}
}
