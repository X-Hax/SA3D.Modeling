namespace SA3D.Modeling.Mesh.Gamecube.Enums
{
	/// <summary>
	/// Primitive type on how surface data is stored.
	/// </summary>
	public enum GCPolyType
	{
		/// <summary>
		/// Triangle list.
		/// </summary>
		Triangles = 0x90,

		/// <summary>
		/// Triangle strip.
		/// </summary>
		TriangleStrip = 0x98,

		/// <summary>
		/// Triangle fan.
		/// </summary>
		TriangleFan = 0xA0,

		/// <summary>
		/// Edge list.
		/// </summary>
		Lines = 0xA8,

		/// <summary>
		/// Consecutive edges.
		/// </summary>
		LineStrip = 0xB0,

		/// <summary>
		/// Lone points in space.
		/// </summary>
		Points = 0xB8
	}
}
