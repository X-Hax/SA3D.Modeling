namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// General read/write context
	/// </summary>
	public struct IOContext
	{
		/// <summary>
		/// Format that the mesh data is serialized with
		/// </summary>
		public Format MeshFormat { get; set; }

		/// <summary>
		/// Format that the level data is serialized with
		/// </summary>
		public Format LevelFormat { get; set; }

		/// <summary>
		/// Offset lookup table
		/// </summary>
		public ModelOffsetLUT OffsetLUT { get; set; }
	}
}
