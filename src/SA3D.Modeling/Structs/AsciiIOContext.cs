namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// Ascii serialization context
	/// </summary>
	public readonly struct AsciiIOContext
	{
		/// <summary>
		/// Do not print the quaternion appendix (if the model does not use quaternion)
		/// </summary>
		public bool NoQuaternionAppendix { init; get; }

		/// <summary>
		/// Vertex Chunks: Format chunk vertex
		/// </summary>
		public bool VertexUserAttributesAsColor { init; get; }

		/// <summary>
		/// Strip Chunks: Format attributes on triangles with 2 attributes as colors
		/// </summary>
		public bool PolygonAttributesAsColor { init; get; }

		/// <summary>
		/// How to format weights when writing
		/// </summary>
		public AsciiWeightFormat WeightFormat { init; get; }

		/// <summary>
		/// Add comments to improve readability of the file
		/// </summary>
		public bool WriteComments { init; get; }
	}
}
