namespace SA3D.Modeling.Mesh.Weighted
{
	/// <summary>
	/// Buffer mode for meshdata.
	/// </summary>
	public enum BufferMode
	{
		/// <summary>
		/// Uses preexisting buffer data.
		/// </summary>
		None,

		/// <summary>
		/// Generates buffer meshdata.
		/// </summary>
		Generate,

		/// <summary>
		/// Generates optimized buffer meshdata.
		/// </summary>
		GenerateOptimized
	}
}
