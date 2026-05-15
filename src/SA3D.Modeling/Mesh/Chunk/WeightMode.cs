namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Vertex chunk weight status.
	/// </summary>
	public enum WeightStatus
	{
		/// <summary>
		/// Start of a weighted model (replaces cached vertices).
		/// </summary>
		Start,

		/// <summary>
		/// Middle of a weighted model (adds onto cached vertices).
		/// </summary>
		Middle,

		/// <summary>
		/// End of a weighted model (adds onto cached vertices and normalizes them).
		/// </summary>
		End
	}
}
