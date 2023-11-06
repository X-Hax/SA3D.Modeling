namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Types of vertex chunks.
	/// </summary>
	public enum VertexChunkType : byte
	{
		/// <summary>
		/// Null chunk.
		/// </summary>
		Null = 0,

		/// <summary>
		/// Position only; Uses GPU ready 4 component vector.
		/// </summary>
		BlankVec4 = ChunkTypeExtensions._vertex + 0,

		/// <summary>
		/// Contains: Normals; Uses GPU ready 4 component vector.
		/// </summary>
		NormalVec4 = ChunkTypeExtensions._vertex + 1,

		/// <summary>
		/// Position only.
		/// </summary>
		Blank = ChunkTypeExtensions._vertex + 2,

		/// <summary>
		/// Contains: Diffuse colors (BGRA8).
		/// </summary>
		Diffuse = ChunkTypeExtensions._vertex + 3,

		/// <summary>
		/// Contains: User defined attributes.
		/// </summary>
		UserAttributes = ChunkTypeExtensions._vertex + 4,

		/// <summary>
		/// Contains: System defined attributes.
		/// </summary>
		Attributes = ChunkTypeExtensions._vertex + 5,

		/// <summary>
		/// Contains: Diffuse colors (RGB565), Specular colors (RGB565).
		/// </summary>
		DiffuseSpecular5 = ChunkTypeExtensions._vertex + 6,

		/// <summary>
		/// Contains: Diffuse colors (RGB4444), Specular colors (RGB565).
		/// </summary>
		DiffuseSpecular4 = ChunkTypeExtensions._vertex + 7,

		/// <summary>
		/// Contains: Diffuse intensity (16-bit), Specular intensity (16-bit).
		/// </summary>
		Intensity = ChunkTypeExtensions._vertex + 8,

		/// <summary>
		/// Contains: Normals.
		/// </summary>
		Normal = ChunkTypeExtensions._vertex + 9,

		/// <summary>
		/// Contains: Normals, Normal, Diffuse colors (BGRA32).
		/// </summary>
		NormalDiffuse = ChunkTypeExtensions._vertex + 10,

		/// <summary>
		/// Contains: Normals, User defined attributes.
		/// </summary>
		NormalUserAttributes = ChunkTypeExtensions._vertex + 11,

		/// <summary>
		/// Contains: Normals, System defined attributes.
		/// </summary>
		NormalAttributes = ChunkTypeExtensions._vertex + 12,

		/// <summary>
		/// Contains: Normals, Diffuse colors (RGB565), Specular colors (RGB565).
		/// </summary>
		NormalDiffuseSpecular5 = ChunkTypeExtensions._vertex + 13,

		/// <summary>
		/// Contains: Normals, Diffuse colors (RGB4444), Specular colors (RGB565).
		/// </summary>
		NormalDiffuseSpecular4 = ChunkTypeExtensions._vertex + 14,

		/// <summary>
		/// Contains: Normals, Diffuse intensity (16-bit), Specular intensity (16-bit).
		/// </summary>
		NormalIntensity = ChunkTypeExtensions._vertex + 15,

		/// <summary>
		/// Contains: 32 Bit vertex normals (first 2 bits unused, each channel takes 10).
		/// </summary>
		Normal32 = ChunkTypeExtensions._vertex + 16,

		/// <summary>
		/// Contains: 32 Bit vertex normals (first 2 bits unused, each channel takes 10), Diffuse color (BGRA32).
		/// </summary>
		Normal32Diffuse = ChunkTypeExtensions._vertex + 17,

		/// <summary>
		/// Contains: 32 Bit vertex normals (first 2 bits unused, each channel takes 10), user attributes.
		/// </summary>
		Normal32UserAttributes = ChunkTypeExtensions._vertex + 18,

		/// <summary>
		/// End marker chunk.
		/// </summary>
		End = 255
	}
}
