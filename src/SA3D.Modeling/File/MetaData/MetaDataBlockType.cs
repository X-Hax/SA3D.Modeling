namespace SA3D.Modeling.File
{
	/// <summary>
	/// Meta data type
	/// </summary>
	public enum MetaBlockType : uint
	{
		/// <summary>
		/// Data labels.
		/// </summary>
		Label = 0x4C42414C,

		/// <summary>
		/// List of animation files paths.
		/// </summary>
		Animation = 0x4D494E41,

		/// <summary>
		/// List of morph animation file paths.
		/// </summary>
		Morph = 0x46524F4D,

		/// <summary>
		/// Author of the file.
		/// </summary>
		Author = 0x48545541,

		/// <summary>
		/// Tool used to create the file.
		/// </summary>
		Tool = 0x4C4F4F54,

		/// <summary>
		/// Description given to the file.
		/// </summary>
		Description = 0x43534544,

		/// <summary>
		/// Texture info.
		/// </summary>
		Texture = 0x584554,

		/// <summary>
		/// Name of the action that the data belongs to.
		/// </summary>
		ActionName = 0x4143544E,

		/// <summary>
		/// Name of the object that the data belongs to.
		/// </summary>
		ObjectName = 0x4F424A4E,

		/// <summary>
		/// BASIC weights
		/// </summary>
		Weight = 0x54484757,

		/// <summary>
		/// End marker.
		/// </summary>
		End = 0x444E45
	}
}
