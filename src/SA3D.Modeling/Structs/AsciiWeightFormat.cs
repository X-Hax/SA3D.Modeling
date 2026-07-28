namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// How to format weights when writing to ascii
	/// </summary>
	public enum AsciiWeightFormat
	{
		/// <summary>
		/// Automatically determine which format to write in
		/// </summary>
		Automatic,

		/// <summary>
		/// Force write weights with 1 byte range
		/// </summary>
		Force1,

		/// <summary>
		/// Force write weights with 2 byte range
		/// </summary>
		Force2
	}
}
