using System;

namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Extension methods
	/// </summary>
	public static class ChunkTypeExtensions
	{
		internal const byte _bits = 1;
		internal const byte _tiny = 8;
		internal const byte _material = 16;
		internal const byte _vertex = 32;
		internal const byte _volume = 56;
		internal const byte _strip = 64;

		/// <summary>
		/// Checks whether a vertex chunktype uses 4 component vectors for positions/normals.
		/// </summary>
		/// <param name="type">The type to check.</param>
		public static bool CheckIsVec4(this VertexChunkType type)
		{
			return type is VertexChunkType.BlankVec4 or VertexChunkType.NormalVec4;
		}

		/// <summary>
		/// Checks whether a vertex chunktype uses 32 bit compressed vectors for normals.
		/// </summary>
		/// <param name="type">The type to check.</param>
		public static bool CheckIsNormal32(this VertexChunkType type)
		{
			return type is VertexChunkType.Normal32
				or VertexChunkType.Normal32Diffuse
				or VertexChunkType.Normal32UserAttributes;
		}

		/// <summary>
		/// Checks whether a vertex chunktype has normals.
		/// </summary>
		/// <param name="type">The type to check.</param>
		public static bool CheckHasNormal(this VertexChunkType type)
		{
			return type
				is VertexChunkType.NormalVec4
				or VertexChunkType.Normal
				or VertexChunkType.NormalDiffuse
				or VertexChunkType.NormalUserAttributes
				or VertexChunkType.NormalAttributes
				or VertexChunkType.NormalDiffuseSpecular5
				or VertexChunkType.NormalDiffuseSpecular4
				or VertexChunkType.NormalIntensity
				or VertexChunkType.Normal32
				or VertexChunkType.Normal32Diffuse
				or VertexChunkType.Normal32UserAttributes;
		}

		/// <summary>
		/// Checks whether a vertex chunktype has diffuse colors.
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <returns></returns>
		public static bool CheckHasDiffuseColor(this VertexChunkType type)
		{
			return type
				is VertexChunkType.Diffuse
				or VertexChunkType.DiffuseSpecular5
				or VertexChunkType.DiffuseSpecular4
				or VertexChunkType.Intensity
				or VertexChunkType.NormalDiffuse
				or VertexChunkType.NormalDiffuseSpecular5
				or VertexChunkType.NormalDiffuseSpecular4
				or VertexChunkType.NormalIntensity
				or VertexChunkType.Normal32Diffuse;

		}

		/// <summary>
		/// Checks whether a vertex chunktype has specular colors.
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <returns></returns>
		public static bool CheckHasSpecularColor(this VertexChunkType type)
		{
			return type
				is VertexChunkType.DiffuseSpecular5
				or VertexChunkType.DiffuseSpecular4
				or VertexChunkType.Intensity
				or VertexChunkType.NormalDiffuseSpecular5
				or VertexChunkType.NormalDiffuseSpecular4
				or VertexChunkType.NormalIntensity;
		}

		/// <summary>
		/// Checks whether a vertex chunktype has attributes (user attributes included too).
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <returns></returns>
		public static bool CheckHasAttributes(this VertexChunkType type)
		{
			return type is VertexChunkType.Attributes
				or VertexChunkType.UserAttributes
				or VertexChunkType.NormalAttributes
				or VertexChunkType.NormalUserAttributes
				or VertexChunkType.Normal32UserAttributes;
		}

		/// <summary>
		/// Checks whether a vertex chunktype has weights.
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <returns></returns>
		public static bool CheckHasWeights(this VertexChunkType type)
		{
			return type is VertexChunkType.Attributes
				or VertexChunkType.NormalAttributes;
		}

		/// <summary>
		/// Checks whether a vertex chunktype has diffuse colors
		/// </summary>
		/// <param name="type">The type to check.</param>
		/// <returns></returns>
		public static bool CheckStripHasColor(this PolyChunkType type)
		{
			return type is PolyChunkType.Strip_Color
				or PolyChunkType.Strip_TexColor
				or PolyChunkType.Strip_HDTexColor;
		}

		/// <summary>
		/// Returns the number of 4-byte values a chunk vertex has.
		/// </summary>
		/// <param name="type">Type to get the size of.</param>
		/// <returns></returns>
		public static ushort GetIntegerSize(this VertexChunkType type)
		{
			switch(type)
			{
				case VertexChunkType.Blank:
					return 3;
				case VertexChunkType.BlankVec4:
				case VertexChunkType.Diffuse:
				case VertexChunkType.UserAttributes:
				case VertexChunkType.Attributes:
				case VertexChunkType.DiffuseSpecular5:
				case VertexChunkType.DiffuseSpecular4:
				case VertexChunkType.Intensity:
				case VertexChunkType.Normal32:
					return 4;
				case VertexChunkType.Normal32Diffuse:
				case VertexChunkType.Normal32UserAttributes:
					return 5;
				case VertexChunkType.Normal:
					return 6;
				case VertexChunkType.NormalDiffuse:
				case VertexChunkType.NormalUserAttributes:
				case VertexChunkType.NormalAttributes:
				case VertexChunkType.NormalDiffuseSpecular5:
				case VertexChunkType.NormalDiffuseSpecular4:
				case VertexChunkType.NormalIntensity:
					return 7;
				case VertexChunkType.NormalVec4:
					return 8;
				case VertexChunkType.Null:
				case VertexChunkType.End:
				default:
					throw new ArgumentException($"Invalid vertex chunk type: {type}", nameof(type));
			}
		}
	}
}
