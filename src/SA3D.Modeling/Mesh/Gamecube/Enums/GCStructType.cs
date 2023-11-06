namespace SA3D.Modeling.Mesh.Gamecube.Enums
{
	/// <summary>
	/// The component structure of the data.
	/// </summary>
	public enum GCStructType : byte
	{
		/// <summary>
		/// 2 component vector position (X, Y).
		/// </summary>
		PositionXY = 0,

		/// <summary>
		/// 3 Component vector position (X, Y, Z).
		/// </summary>
		PositionXYZ = 1,

		/// <summary>
		/// 3 Component vector normal (X, Y, Z).
		/// </summary>
		NormalXYZ = 2,

		/// <summary>
		/// Normal, Binormal and Tangent 3 component vectors (X, Y, Z).
		/// </summary>
		NormalNBT = 3,

		/// <summary>
		/// Normal, Binormal and Tangent 3 component vectors (X, Y, Z). (NBT"3"?)
		/// </summary>
		NormalNBT3 = 4,

		/// <summary>
		/// Color with 3 channels.
		/// </summary>
		ColorRGB = 5,

		/// <summary>
		/// Color with 4 channels.
		/// </summary>
		ColorRGBA = 6,

		/// <summary>
		/// Single channel texture coordinates.
		/// </summary>
		TexCoordU = 7,

		/// <summary>
		/// Dual channel texture coordinates.
		/// </summary>
		TexCoordUV = 8
	}
}
