namespace SA3D.Modeling.ObjectData.Enums
{
	/// <summary>
	/// Landtable attributes.
	/// </summary>
	public enum LandtableAttributes : uint
	{
		/// <summary>
		/// Enables geometry motions.
		/// </summary>
		EnableMotions = 0x1,

		/// <summary>
		/// Utilizes specified texture list.
		/// </summary>
		LoadTexlist = 0x2,

		/// <summary>
		/// Specifies a custom draw distance.
		/// </summary>
		CustomDrawDistance = 0x4,

		/// <summary>
		/// Loads texture from file.
		/// </summary>
		LoadTextureFile = 0x8,
	}
}
