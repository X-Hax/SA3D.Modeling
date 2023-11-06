namespace SA3D.Modeling.ObjectData.Enums
{
	/// <summary>
	/// Model data formats.
	/// </summary>
	public enum ModelFormat : int
	{
		/// <summary>
		/// Sonic Adventure 1; Uses BASIC.
		/// </summary>
		SA1 = 0,

		/// <summary>
		/// Sonic Adventure DX; Uses BASIC.
		/// </summary>
		SADX = 1,

		/// <summary>
		/// Sonic Adventure 2; Uses CHUNK (and BASIC).
		/// </summary>
		SA2 = 2,

		/// <summary>
		/// Sonic Adventure 2 Battle; Uses GC (and BASIC).
		/// </summary>
		SA2B = 3,

		/// <summary>
		/// SA3D in-house API format.
		/// </summary>
		Buffer = 4
	}
}
