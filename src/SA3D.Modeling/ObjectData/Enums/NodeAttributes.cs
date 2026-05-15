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
		/// Node is a modifier volume. Only an identifier flag
		/// </summary>
		Modifier = Flag32.B9,

		/// <summary>
		/// Node uses a quaternion instead of euler angle for rotation information.
		/// </summary>
		UseQuaternionRotation = Flag32.B10,

		/// <summary>
		/// Cache current matrix before translating this node. Only one matrix can be actively cached at a time
		/// </summary>
		CacheMatrix = Flag32.B11,

		/// <summary>
		/// Set non-translation components of cached matrix to the current matrix, instead of rotating/scaling with node data
		/// </summary>
		ApplyCachedMatrix = Flag32.B12,

		/// <summary>
		/// Node has weight data / mesh data that is not self dependent. Only an identifier flag, not read by the game engine.
		/// </summary>
		Envelope = Flag32.B13,
	}
}
