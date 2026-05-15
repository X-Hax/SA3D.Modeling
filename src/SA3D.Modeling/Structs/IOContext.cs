namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// General read/write context
	/// </summary>
	public readonly struct IOContext
	{
		/// <summary>
		/// Format that the level data is serialized with
		/// </summary>
		public Format MeshFormat { init;  get; }

		/// <summary>
		/// Format that the level data is serialized with
		/// </summary>
		public Format LevelFormat { init; get; }

		/// <summary>
		/// Pointer lookup table
		/// </summary>
		public PointerLUT PointerLUT { init; get; }
	}
}
