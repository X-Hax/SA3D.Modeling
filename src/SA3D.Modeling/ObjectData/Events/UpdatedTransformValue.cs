using System;

namespace SA3D.Modeling.ObjectData.Events
{
	/// <summary>
	/// Determines which transform property triggered the event.
	/// </summary>
	[Flags]
	public enum UpdatedTransformValue : int
	{
		/// <summary>
		/// Position was updated.
		/// </summary>
		Position = 0x01,

		/// <summary>
		/// Rotation was updated.
		/// </summary>
		Rotation = 0x02,

		/// <summary>
		/// Scale was updated.
		/// </summary>
		Scale = 0x4,

		/// <summary>
		/// "RotateZYX" was updated.
		/// </summary>
		RotateMode = 0x08
	}
}
