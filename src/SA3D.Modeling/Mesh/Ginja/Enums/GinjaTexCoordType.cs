namespace SA3D.Modeling.Mesh.Gamecube.Enums
{
	/// <summary>
	/// The function type used to generate the texture coordinates.
	/// </summary>
	public enum GCTexCoordType
	{
		/// <summary>
		/// Multiplies input by a 3x4 matrix.
		/// </summary>
		Matrix3x4 = 0x0,

		/// <summary>
		/// Multiplies input by a 2x4 matrix.
		/// </summary>
		Matrix2x4 = 0x1,

		/// <summary>
		/// Gamecube bump mapping function 0 (?).
		/// </summary>
		Bump0 = 0x2,

		/// <summary>
		/// Gamecube bump mapping function 1 (?).
		/// </summary>
		Bump1 = 0x3,

		/// <summary>
		/// Gamecube bump mapping function 2 (?).
		/// </summary>
		Bump2 = 0x4,

		/// <summary>
		/// Gamecube bump mapping function 3 (?).
		/// </summary>
		Bump3 = 0x5,

		/// <summary>
		/// Gamecube bump mapping function 4 (?).
		/// </summary>
		Bump4 = 0x6,

		/// <summary>
		/// Gamecube bump mapping function 5 (?).
		/// </summary>
		Bump5 = 0x7,

		/// <summary>
		/// Gamecube bump mapping function 6 (?).
		/// </summary>
		Bump6 = 0x8,

		/// <summary>
		/// Gamecube bump mapping function 7 (?).
		/// </summary>
		Bump7 = 0x9,

		/// <summary>
		/// Gamecube SRTG function (?).
		/// </summary>
		SRTG = 0xA
	}
}
