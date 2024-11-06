using SA3D.Common;
using System;

namespace SA3D.Modeling.ObjectData.Enums
{
	/// <summary>
	/// Node attributes
	/// </summary>
	[Flags]
	public enum NodeAttributes : uint
	{
		/// <summary>
		/// The nodes position has no effect on the transforms.
		/// </summary>
		NoPosition = Flag32.B0,

		/// <summary>
		/// The nodes rotation has no effect on the transforms.
		/// </summary>
		NoRotation = Flag32.B1,

		/// <summary>
		/// The nodes scale has no effect on the transforms.
		/// </summary>
		NoScale = Flag32.B2,

		/// <summary>
		/// Node should be skipped for attach related evaluations. Required if node has no attach.
		/// </summary>
		SkipDraw = Flag32.B3,

		/// <summary>
		/// Child of the node is skipped. Required if the node has no child.
		/// </summary>
		SkipChildren = Flag32.B4,

		/// <summary>
		/// Euler angles are applied in ZYX order, instead of XYZ.
		/// </summary>
		RotateZYX = Flag32.B5,

		/// <summary>
		/// Ignore this node (but not its children) entirely for when evaluation an animation.
		/// </summary>
		NoAnimate = Flag32.B6,

		/// <summary>
		/// Ignore this node (but not its children) entirely for when evaluation a shape animation.
		/// </summary>
		NoMorph = Flag32.B7,

		/// <summary>
		/// If the mesh bounds are out of view, skip children (for drawing)
		/// </summary>
		Clip = Flag32.B8,

		/// <summary>
		/// Unused
		/// </summary>
		Modifier = Flag32.B9,

		/// <summary>
		/// Node uses a quaternion instead of euler angle for rotation information.
		/// </summary>
		UseQuaternionRotation = Flag32.B10,

		/// <summary>
		/// Cache rotation data before the object is processed
		/// </summary>
		CacheRotation = Flag32.B11,

		/// <summary>
		/// Use cached rotation data (see <see cref="CacheRotation"/>)
		/// </summary>
		ApplyCachedRotation = Flag32.B12,

		/// <summary>
		/// Unused
		/// </summary>
		Envelope = Flag32.B13,
	}
}
