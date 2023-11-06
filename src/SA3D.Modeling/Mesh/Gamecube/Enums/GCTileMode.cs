using SA3D.Common;
using System;

namespace SA3D.Modeling.Mesh.Gamecube.Enums
{
	/// <summary>
	/// Gamecube specific texture tiling modes.
	/// </summary>
	[Flags]
	public enum GCTileMode : byte
	{
		/// <summary>
		/// Repeats texture coordinates outside of 0-1 on the vertical axis. 
		/// <br/> Has priority over mirroring.
		/// </summary>
		RepeatV = Flag8.B0,

		/// <summary>
		/// Mirrors texture coordinates every second repeated instance on the vertical axis.
		/// </summary>
		MirrorV = Flag8.B1,

		/// <summary>
		/// Repeats texture coordinates outside of 0-1 on the horizontal axis. 
		/// <br/> Has priority over mirroring.
		/// </summary>
		RepeatU = Flag8.B2,

		/// <summary>
		/// Mirrors texture coordinates every second repeated instance on the horizontal axis.
		/// </summary>
		MirrorU = Flag8.B3,

		/// <summary>
		/// Unknown functionality.
		/// </summary>
		Unknown = Flag8.B4,

		/// <summary>
		/// Mask for all valid values.
		/// </summary>
		Mask = RepeatV | MirrorV | RepeatU | MirrorU | Unknown
	}
}
