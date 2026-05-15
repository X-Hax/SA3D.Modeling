namespace SA3D.Modeling.Mesh.Ginja.Enums
{
	/// <summary>
	/// Ginja color channel IDs
	/// </summary>
	public enum GinjaColorChannelID : byte
	{
		/// <summary>
		/// Color 0
		/// </summary>
		Color0 = 0,

		/// <summary>
		/// Color 1
		/// </summary>
		Color1 = 1,

		/// <summary>
		/// Alpha 0
		/// </summary>
		Alpha0 = 2,

		/// <summary>
		/// Alpha 0
		/// </summary>
		Alpha1 = 3,

		/// <summary>
		/// Combined output of <see cref="Color0"/> and <see cref="Alpha0"/>
		/// </summary>
		Color0A0 = 4,

		/// <summary>
		/// Combined output of <see cref="Color1"/> and <see cref="Alpha1"/>
		/// </summary>
		Color1A1 = 5,

		/// <summary>
		/// Color Zero
		/// </summary>
		ColorZero = 6,

		/// <summary>
		/// Alpha Bump
		/// </summary>
		AlphaBump = 7,

		/// <summary>
		/// Alpha Bump N
		/// </summary>
		AlphaBumpN = 8,

		/// <summary>
		/// Null
		/// </summary>
		ColorNull = 0xFF
	}
}
