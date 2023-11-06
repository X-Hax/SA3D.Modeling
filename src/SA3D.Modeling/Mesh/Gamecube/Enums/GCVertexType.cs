namespace SA3D.Modeling.Mesh.Gamecube.Enums
{
	/// <summary>
	/// Indicates which type of data a vertex set stores.
	/// </summary>
	public enum GCVertexType : byte
	{
		/// <summary>
		/// Position Matrix Indices.
		/// </summary>
		PositionMatrixID = 0,

		/// <summary>
		/// Vertex Positions.
		/// </summary>
		Position = 1,

		/// <summary>
		/// Vertex normals.
		/// </summary>
		Normal = 2,

		/// <summary>
		/// Vertex colors (First slot).
		/// </summary>
		Color0 = 3,

		/// <summary>
		/// Vertex colors (Second slot).
		/// </summary>
		Color1 = 4,

		/// <summary>
		/// Texture Coordinates (First slot).
		/// </summary>
		TexCoord0 = 5,

		/// <summary>
		/// Texture Coordinates (Second slot).
		/// </summary>
		TexCoord1 = 6,

		/// <summary>
		/// Texture Coordinates (Third slot).
		/// </summary>
		TexCoord2 = 7,

		/// <summary>
		/// Texture Coordinates (Fourth slot).
		/// </summary>
		TexCoord3 = 8,

		/// <summary>
		/// Texture Coordinates (Fifth slot).
		/// </summary>
		TexCoord4 = 9,

		/// <summary>
		/// Texture Coordinates (Sixth slot).
		/// </summary>
		TexCoord5 = 10,

		/// <summary>
		/// Texture Coordinates (seventh slot).
		/// </summary>
		TexCoord6 = 11,

		/// <summary>
		/// Texture Coordinates (eight slot).
		/// </summary>
		TexCoord7 = 12,

		/// <summary>
		/// Marks end of vertex sets.
		/// </summary>
		End = 255
	}
}
