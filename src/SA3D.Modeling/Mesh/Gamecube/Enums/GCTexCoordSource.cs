namespace SA3D.Modeling.Mesh.Gamecube.Enums
{
	/// <summary>
	/// Input values to use for when calculating texture coordinates.
	/// </summary>
	public enum GCTexCoordSource
	{
		/// <summary>
		/// Vertex positions.
		/// </summary>
		Position = 0x0,

		/// <summary>
		/// Vertex normals.
		/// </summary>
		Normal = 0x1,

		/// <summary>
		/// Vertex binormals.
		/// </summary>
		Binormal = 0x2,

		/// <summary>
		/// Vertex tangents.
		/// </summary>
		Tangent = 0x3,

		/// <summary>
		/// First set of the vertex' texture coordinates.
		/// </summary>
		TexCoord0 = 0x4,

		/// <summary>
		/// Second set of the vertex' texture coordinates.
		/// </summary>
		TexCoord1 = 0x5,

		/// <summary>
		/// Third set of the vertex' texture coordinates.
		/// </summary>
		TexCoord2 = 0x6,

		/// <summary>
		/// Fourth set of the vertex' texture coordinates.
		/// </summary>
		TexCoord3 = 0x7,

		/// <summary>
		/// Fifth set of the vertex' texture coordinates.
		/// </summary>
		TexCoord4 = 0x8,

		/// <summary>
		/// Sixth set of the vertex' texture coordinates.
		/// </summary>
		TexCoord5 = 0x9,

		/// <summary>
		/// Seventh set of the vertex' texture coordinates.
		/// </summary>
		TexCoord6 = 0xA,

		/// <summary>
		/// Eight set of the vertex' texture coordinates.
		/// </summary>
		TexCoord7 = 0xB,

		/// <summary>
		/// Bump related 0 (?).
		/// </summary>
		BumpTexCoord0 = 0xC,

		/// <summary>
		/// Bump related 1 (?).
		/// </summary>
		BumpTexCoord1 = 0xD,

		/// <summary>
		/// Bump related 2 (?).
		/// </summary>
		BumpTexCoord2 = 0xE,

		/// <summary>
		/// Bump related 3 (?).
		/// </summary>
		BumpTexCoord3 = 0xF,

		/// <summary>
		/// Bump related 4 (?).
		/// </summary>
		BumpTexCoord4 = 0x10,

		/// <summary>
		/// Bump related 5 (?).
		/// </summary>
		BumpTexCoord5 = 0x11,

		/// <summary>
		/// Bump related 6 (?).
		/// </summary>
		BumpTexCoord6 = 0x12,

		/// <summary>
		/// First set of the vertex' colors.
		/// </summary>
		Color0 = 0x13,

		/// <summary>
		/// Second set of the vertex' colors.
		/// </summary>
		Color1 = 0x14,
	}
}
