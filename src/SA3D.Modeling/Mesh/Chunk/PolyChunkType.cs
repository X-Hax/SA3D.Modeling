namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Chunk type
	/// </summary>
	public enum PolyChunkType : byte
	{
		/// <summary>
		/// Null chunk.
		/// </summary>
		Null = 0,

		/// <summary>
		/// Contains transparency blendmodes.
		/// </summary>
		BlendAlpha = ChunkTypeExtensions._bits + 0,

		/// <summary>
		/// Contains mipmap distance multiplier.
		/// </summary>
		MipmapDistanceMultiplier = ChunkTypeExtensions._bits + 1,

		/// <summary>
		/// Contains specularity exponent.
		/// </summary>
		SpecularExponent = ChunkTypeExtensions._bits + 2,

		/// <summary>
		/// Contains index for caching poly chunks.
		/// </summary>
		CacheList = ChunkTypeExtensions._bits + 3,

		/// <summary>
		/// Contains index for drawing poly chunks
		/// </summary>
		DrawList = ChunkTypeExtensions._bits + 4,

		/// <summary>
		/// Contains texture information.
		/// </summary>
		TextureID = ChunkTypeExtensions._tiny + 0,

		/// <summary>
		/// Contains texture information. Same as <see cref="TextureID"/>.
		/// </summary>
		TextureID2 = ChunkTypeExtensions._tiny + 1,

		/// <summary>
		/// Material; Contains diffuse color (ARGB32).
		/// </summary>
		Material_Diffuse = ChunkTypeExtensions._material + 1,

		/// <summary>
		/// Material; Contains ambient color (RGB24).
		/// </summary>
		Material_Ambient = ChunkTypeExtensions._material + 2,

		/// <summary>
		/// Material; Contains diffuse color (ARGB32) and ambient color (RGB24).
		/// </summary>
		Material_DiffuseAmbient = ChunkTypeExtensions._material + 3,

		/// <summary>
		/// Material; Contains specular exponent and color (RGB24).
		/// </summary>
		Material_Specular = ChunkTypeExtensions._material + 4,

		/// <summary>
		/// Material; Contains diffuse color (ARGB32), specular exponent and color (RGB24).
		/// </summary>
		Material_DiffuseSpecular = ChunkTypeExtensions._material + 5,

		/// <summary>
		/// Material; Contains ambient color (RGB24), specular exponent and color (RGB24).
		/// </summary>
		Material_AmbientSpecular = ChunkTypeExtensions._material + 6,

		/// <summary>
		/// Material; Contains diffuse color (ARGB32), ambient color (RGB24), specular exponent and color (RGB24).
		/// </summary>
		Material_DiffuseAmbientSpecular = ChunkTypeExtensions._material + 7,

		/// <summary>
		/// Material; Contains "bump" information (?).
		/// </summary>
		Material_Bump = ChunkTypeExtensions._material + 8,

		/// <summary>
		/// Material; Contains diffuse color (ARGB32).
		/// <br/> Same as <see cref="Material_Diffuse"/>.
		/// </summary>
		Material_Diffuse2 = ChunkTypeExtensions._material + 9,

		/// <summary>
		/// Material; Contains ambient color (RGB24).
		/// <br/> Same as <see cref="Material_Ambient"/>.
		/// </summary>
		Material_Ambient2 = ChunkTypeExtensions._material + 10,

		/// <summary>
		/// Material; Contains diffuse color (ARGB32) and ambient color (RGB24).
		/// <br/> Same as <see cref="Material_DiffuseAmbient"/>.
		/// </summary>
		Material_DiffuseAmbient2 = ChunkTypeExtensions._material + 11,

		/// <summary>
		/// Material; Contains specular exponent and color (RGB24).
		/// <br/> Same as <see cref="Material_Specular"/>.
		/// </summary>
		Material_Specular2 = ChunkTypeExtensions._material + 12,

		/// <summary>
		/// Material; Contains diffuse color (ARGB32), specular exponent and color (RGB24).
		/// <br/> Same as <see cref="Material_DiffuseSpecular"/>.
		/// </summary>
		Material_DiffuseSpecular2 = ChunkTypeExtensions._material + 13,

		/// <summary>
		/// Material; Contains ambient color (RGB24), specular exponent and color (RGB24).
		/// <br/> Same as <see cref="Material_AmbientSpecular"/>.
		/// </summary>
		Material_AmbientSpecular2 = ChunkTypeExtensions._material + 14,

		/// <summary>
		/// Material; Contains diffuse color (ARGB32), ambient color (RGB24), specular exponent and color (RGB24).
		/// <br/> Same as <see cref="Material_DiffuseAmbientSpecular"/>.
		/// </summary>
		Material_DiffuseAmbientSpecular2 = ChunkTypeExtensions._material + 15,

		/// <summary>
		/// Volume defined from triangles.
		/// </summary>
		Volume_Polygon3 = ChunkTypeExtensions._volume + 0,

		/// <summary>
		/// Volume defined from quads.
		/// </summary>
		Volume_Polygon4 = ChunkTypeExtensions._volume + 1,

		/// <summary>
		/// Volume defined from triangle strips.
		/// </summary>
		Volume_Strip = ChunkTypeExtensions._volume + 2,

		/// <summary>
		/// Triangle strips for rendering; No additional info.
		/// </summary>
		Strip_Blank = ChunkTypeExtensions._strip + 0,

		/// <summary>
		/// Triangle strips for rendering; Contains texture coordinates (0-255 range).
		/// </summary>
		Strip_Tex = ChunkTypeExtensions._strip + 1,

		/// <summary>
		/// Triangle strips for rendering; Contains texture coordinates (0-1023 range).
		/// </summary>
		Strip_HDTex = ChunkTypeExtensions._strip + 2,

		/// <summary>
		/// Triangle strips for rendering; Contains normals.
		/// </summary>
		Strip_Normal = ChunkTypeExtensions._strip + 3,

		/// <summary>
		/// Triangle strips for rendering; Contains normals, texture coordinates (0-255 range).
		/// </summary>
		Strip_TexNormal = ChunkTypeExtensions._strip + 4,

		/// <summary>
		/// Triangle strips for rendering; Contains normals, texture coordinates (0-1023 range).
		/// </summary>
		Strip_HDTexNormal = ChunkTypeExtensions._strip + 5,

		/// <summary>
		/// Triangle strips for rendering; Contains colors (ARGB32).
		/// </summary>
		Strip_Color = ChunkTypeExtensions._strip + 6,

		/// <summary>
		/// Triangle strips for rendering; Contains colors (ARGB32), texture coordinates (0-255 range).
		/// </summary>
		Strip_TexColor = ChunkTypeExtensions._strip + 7,

		/// <summary>
		/// Triangle strips for rendering; Contains colors (ARGB32), texture coordinates (0-1023 range).
		/// </summary>
		Strip_HDTexColor = ChunkTypeExtensions._strip + 8,

		/// <summary>
		/// Triangle strips for rendering; The same as <see cref="Strip_Blank"/>.
		/// </summary>
		Strip_BlankDouble = ChunkTypeExtensions._strip + 9,

		/// <summary>
		/// Triangle strips for rendering; Contains 2 sets of texture coordinates (0-255 range).
		/// </summary>
		Strip_TexDouble = ChunkTypeExtensions._strip + 10,

		/// <summary>
		/// Triangle strips for rendering; Contains 2 sets of texture coordinates (0-1023 range).
		/// </summary>
		Strip_HDTexDouble = ChunkTypeExtensions._strip + 11,

		/// <summary>
		/// End marker chunk.
		/// </summary>
		End = 255
	}
}
