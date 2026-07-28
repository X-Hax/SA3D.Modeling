namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// Ascii serialization context
	/// </summary>
	public struct AsciiIOContext
	{
		/// <summary>
		/// Do not print the quaternion appendix (if the model does not use quaternion)
		/// </summary>
		public bool NoQuaternionAppendix { get; set; }

		/// <summary>
		/// Vertex Chunks: Format chunk vertex
		/// </summary>
		public bool VertexUserAttributesAsColor { get; set; }

		/// <summary>
		/// Strip Chunks: Format attributes on triangles with 2 attributes as colors
		/// </summary>
		public bool PolygonAttributesAsColor { get; set; }

		/// <summary>
		/// How to format weights when writing
		/// </summary>
		public AsciiWeightFormat WeightFormat { get; set; }

		/// <summary>
		/// Add comments to improve readability of the file
		/// </summary>
		public bool WriteComments { get; set; }
	}
}
