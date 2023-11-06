namespace SA3D.Modeling.Mesh.Basic.Polygon
{
	/// <summary>
	/// The different primitive types for BASIC meshes
	/// </summary>
	public enum BasicPolygonType
	{
		/// <summary>
		/// Arranges polygons in a triangle list.
		/// </summary>
		Triangles,

		/// <summary>
		/// Arranges polygons in a quad list.
		/// </summary>
		Quads,

		/// <summary>
		/// Arranges polygons with an arbitrary number of corners in a list.
		/// </summary>
		NPoly,

		/// <summary>
		/// Arranges triangles in strips.
		/// </summary>
		TriangleStrips
	}

}
