using SA3D.Common;
using System;

namespace SA3D.Modeling.Mesh.Gamecube.Enums
{
	/// <summary>
	/// Holds information about the triangle lists index formatting of geometry.
	/// </summary>
	[Flags]
	public enum GCIndexFormat : uint
	{
		/// <summary>
		/// Whether the position matrix ID indices are 16 bit rather than 8 bit.
		/// </summary>
		PositionMatrixIDLargeIndex = Flag32.B0,

		/// <summary>
		/// Whether the Geometry contains position matrix ID data.
		/// </summary>
		HasPositionMatrixID = Flag32.B1,

		/// <summary>
		/// Whether the position indices are 16 bit rather than 8 bit.
		/// </summary>
		PositionLargeIndex = Flag32.B2,

		/// <summary>
		/// Whether the Geometry contains position data.
		/// </summary>
		HasPosition = Flag32.B3,

		/// <summary>
		/// Whether the normal indices are 16 bit rather than 8 bit.
		/// </summary>
		NormalLargeIndex = Flag32.B4,

		/// <summary>
		/// Whether the Geometry contains normal data.
		/// </summary>
		HasNormal = Flag32.B5,

		/// <summary>
		/// Whether the color0 indices are 16 bit rather than 8 bit.
		/// </summary>
		Color0LargeIndex = Flag32.B6,

		/// <summary>
		/// Whether the Geometry contains color0 data.
		/// </summary>
		HasColor0 = Flag32.B7,

		/// <summary>
		/// Whether the color1 indices are 16 bit rather than 8 bit.
		/// </summary>
		Color1LargeIndex = Flag32.B8,

		/// <summary>
		/// Whether the Geometry contains color1 data.
		/// </summary>
		HasColor1 = Flag32.B9,

		/// <summary>
		/// Whether the texcoord0 indices are 16 bit rather than 8 bit.
		/// </summary>
		TexCoord0LargeIndex = Flag32.B10,

		/// <summary>
		/// Whether the Geometry contains texcoord0 data.
		/// </summary>
		HasTexCoord0 = Flag32.B11,

		/// <summary>
		/// Whether the texcoord1 indices are 16 bit rather than 8 bit.
		/// </summary>
		TexCoord1LargeIndex = Flag32.B12,

		/// <summary>
		/// Whether the Geometry contains texcoord1 data.
		/// </summary>
		HasTexCoord1 = Flag32.B13,

		/// <summary>
		/// Whether the texcoord2 indices are 16 bit rather than 8 bit.
		/// </summary>
		TexCoord2LargeIndex = Flag32.B14,

		/// <summary>
		/// Whether the Geometry contains texcoord2 data.
		/// </summary>
		HasTexCoord2 = Flag32.B15,

		/// <summary>
		/// Whether the texcoord3 indices are 16 bit rather than 8 bit.
		/// </summary>
		TexCoord3LargeIndex = Flag32.B16,

		/// <summary>
		/// Whether the Geometry contains texcoord3 data.
		/// </summary>
		HasTexCoord3 = Flag32.B17,

		/// <summary>
		/// Whether the texcoord4 indices are 16 bit rather than 8 bit.
		/// </summary>
		TexCoord4LargeIndex = Flag32.B18,

		/// <summary>
		/// Whether the Geometry contains texcoord4 data.
		/// </summary>
		HasTexCoord4 = Flag32.B19,

		/// <summary>
		/// Whether the texcoord5 indices are 16 bit rather than 8 bit.
		/// </summary>
		TexCoord5LargeIndex = Flag32.B20,

		/// <summary>
		/// Whether the Geometry contains texcoord5 data.
		/// </summary>
		HasTexCoord5 = Flag32.B21,

		/// <summary>
		/// Whether the texcoord6 indices are 16 bit rather than 8 bit.
		/// </summary>
		TexCoord6LargeIndex = Flag32.B22,

		/// <summary>
		/// Whether the Geometry contains texcoord6 data.
		/// </summary>
		HasTexCoord6 = Flag32.B23,

		/// <summary>
		/// Whether the texcoord7 indices are 16 bit rather than 8 bit.
		/// </summary>
		TexCoord7LargeIndex = Flag32.B24,

		/// <summary>
		/// Whether the Geometry contains texcoord7 data.
		/// </summary>
		HasTexCoord7 = Flag32.B25,
	}
}
