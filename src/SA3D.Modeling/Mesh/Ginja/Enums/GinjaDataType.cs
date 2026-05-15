namespace SA3D.Modeling.Mesh.Gamecube.Enums
{
	/// <summary>
	/// The component type of the data.
	/// </summary>
	public enum GCDataType : byte
	{
		/// <summary>
		/// Equal to <see cref="byte"/>.
		/// </summary>
		Unsigned8 = 0,

		/// <summary>
		/// Equal to <see cref="sbyte"/>.
		/// </summary>
		Signed8 = 1,

		/// <summary>
		/// Equal to <see cref="ushort"/>.
		/// </summary>
		Unsigned16 = 2,

		/// <summary>
		/// Equal to <see cref="short"/>.
		/// </summary>
		Signed16 = 3,

		/// <summary>
		/// Equal to <see cref="float"/>.
		/// </summary>
		Float32 = 4,

		/// <summary>
		/// RGB565 struct.
		/// </summary>
		RGB565 = 5,

		/// <summary>
		/// RGB8 struct.
		/// </summary>
		RGB8 = 6,

		/// <summary>
		/// RGBX8 struct.
		/// </summary>
		RGBX8 = 7,

		/// <summary>
		/// RGBA4 struct.
		/// </summary>
		RGBA4 = 8,

		/// <summary>
		/// RGBA6 struct.
		/// </summary>
		RGBA6 = 9,

		/// <summary>
		/// RGBA8 struct.
		/// </summary>
		RGBA8 = 10
	}
}
