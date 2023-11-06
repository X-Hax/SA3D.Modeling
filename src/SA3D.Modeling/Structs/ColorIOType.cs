namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// Write/Read mode for colors.
	/// </summary>
	public enum ColorIOType
	{
		/// <summary>
		/// ARGB Color; Each channel takes a byte.
		/// </summary>
		ARGB8_32,

		/// <summary>
		/// ARGB Color, but written as two shorts (important for big endian ARGB).
		/// </summary>
		ARGB8_16,

		/// <summary>
		/// Color; Each channel uses 4 bits.
		/// </summary>
		ARGB4,

		/// <summary>
		/// Colors; Red and blue use 5 bits, green 6 bits.
		/// </summary>
		RGB565,

		/// <summary>
		/// BGRA Color; Each channel takes a byte.
		/// </summary>
		RGBA8,
	}

	/// <summary>
	/// Extension methods for <see cref="ColorIOType"/>
	/// </summary>
	public static class ColorIOTypeExtensions
	{
		/// <summary>
		/// Returns how many bytes the given color type takes up.
		/// </summary>
		/// <param name="type">Type to get the size of.</param>
		/// <returns>The value size.</returns>
		public static int GetByteSize(this ColorIOType type)
		{
			return type switch
			{
				ColorIOType.ARGB8_32 => 4,
				ColorIOType.ARGB8_16 => 4,
				ColorIOType.ARGB4 => 2,
				ColorIOType.RGB565 => 2,
				ColorIOType.RGBA8 => 4,
				_ => 0,
			};
		}

	}
}
