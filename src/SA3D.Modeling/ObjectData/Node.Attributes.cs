﻿using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.Structs;
using System.Numerics;

namespace SA3D.Modeling.ObjectData
{
	public partial class Node
	{
		/// <summary>
		/// Various additional info for the node.
		/// </summary>
		public NodeAttributes Attributes { get; private set; }


		/// <summary>
		/// The nodes position has no effect on the transforms.
		/// </summary>
		public bool NoPosition
		{
			get => GetNodeAttribute(NodeAttributes.NoPosition);
			set => SetNodeAttribute(NodeAttributes.NoPosition, value);
		}

		/// <summary>
		/// The nodes rotation has no effect on the transforms.
		/// </summary>
		public bool NoRotation
		{
			get => GetNodeAttribute(NodeAttributes.NoRotation);
			set => SetNodeAttribute(NodeAttributes.NoRotation, value);
		}

		/// <summary>
		/// The nodes scale has no effect on the transforms.
		/// </summary>
		public bool NoScale
		{
			get => GetNodeAttribute(NodeAttributes.NoScale);
			set => SetNodeAttribute(NodeAttributes.NoScale, value);
		}

		/// <summary>
		/// Node should be skipped for attach related evaluations. Required if node has no attach.
		/// </summary>
		public bool SkipDraw
		{
			get => GetNodeAttribute(NodeAttributes.SkipDraw);
			set => SetNodeAttribute(NodeAttributes.SkipDraw, value);
		}

		/// <summary>
		/// Child of the node is skipped. Required if the node has no child.
		/// </summary>
		public bool SkipChildren
		{
			get => GetNodeAttribute(NodeAttributes.SkipChildren);
			set => SetNodeAttribute(NodeAttributes.SkipChildren, value);
		}

		/// <summary>
		/// Whether euler angles are applied in ZYX order, instead of XYZ.
		/// </summary>
		public bool RotateZYX
		{
			get => GetNodeAttribute(NodeAttributes.RotateZYX);
			private set => SetNodeAttribute(NodeAttributes.RotateZYX, value);
		}

		/// <summary>
		/// If enabled, the node will be ignored by animations (but not its child or successor).
		/// </summary>
		public bool NoAnimate
		{
			get => GetNodeAttribute(NodeAttributes.NoAnimate);
			set => SetNodeAttribute(NodeAttributes.NoAnimate, value);
		}

		/// <summary>
		/// If enabled, the node will be ignored by shape animations (but not its child or successor).
		/// </summary>
		public bool NoMorph
		{
			get => GetNodeAttribute(NodeAttributes.NoMorph);
			set => SetNodeAttribute(NodeAttributes.NoMorph, value);
		}

		/// <summary>
		/// If the mesh bounds are out of view, skip children (for drawing)
		/// </summary>
		public bool Clip
		{
			get => GetNodeAttribute(NodeAttributes.Clip);
			set => SetNodeAttribute(NodeAttributes.Clip, value);
		}

		/// <summary>
		/// Unused
		/// </summary>
		public bool Modifier
		{
			get => GetNodeAttribute(NodeAttributes.Modifier);
			set => SetNodeAttribute(NodeAttributes.Modifier, value);
		}

		/// <summary>
		/// Whether the node uses quaternion rotations.
		/// </summary>
		public bool UseQuaternionRotation
		{
			get => GetNodeAttribute(NodeAttributes.UseQuaternionRotation);
			set => SetNodeAttribute(NodeAttributes.UseQuaternionRotation, value);
		}

		/// <summary>
		/// Cache rotation data before the object is processed
		/// </summary>
		public bool CacheRotation
		{
			get => GetNodeAttribute(NodeAttributes.CacheRotation);
			set => SetNodeAttribute(NodeAttributes.CacheRotation, value);
		}

		/// <summary>
		/// Use cached rotation data (see <see cref="CacheRotation"/>)
		/// </summary>
		public bool ApplyCachedRotation
		{
			get => GetNodeAttribute(NodeAttributes.ApplyCachedRotation);
			set => SetNodeAttribute(NodeAttributes.ApplyCachedRotation, value);
		}

		/// <summary>
		/// Unused
		/// </summary>
		public bool Envelope
		{
			get => GetNodeAttribute(NodeAttributes.Envelope);
			set => SetNodeAttribute(NodeAttributes.Envelope, value);
		}


		private void SetNodeAttribute(NodeAttributes attribute, bool state)
		{
			if(state)
			{
				Attributes |= attribute;
			}
			else
			{
				Attributes &= ~attribute;
			}
		}

		private bool GetNodeAttribute(NodeAttributes attribute)
		{
			return Attributes.HasFlag(attribute);
		}

		/// <summary>
		/// Automatically fills in attributes based on other properties of the node.
		/// </summary>
		/// <param name="overrideExisting">Whether the automatic attributes should override the existing values, instead of "adding" to them</param>
		public void AutoNodeAttributes(bool overrideExisting = false)
		{
			NoPosition = (overrideExisting && NoPosition) || Position.IsDistanceApproximate(Vector3.Zero);
			NoScale = (overrideExisting && NoScale) || Scale.IsDistanceApproximate(Vector3.One);
			NoRotation = (overrideExisting && NoRotation) || EulerRotation.IsDistanceApproximate(Vector3.Zero);
			SkipChildren = (overrideExisting && SkipChildren) || Child == null;
			SkipDraw = (overrideExisting && SkipDraw) || Attach == null;
		}

		/// <summary>
		/// Sets the rotation order.
		/// </summary>
		/// <param name="newValue">New rotation order state.</param>
		/// <param name="mode">Determines how the rotation values of a node should be handled after the rotation order has been changed.</param>
		public void SetRotationZYX(bool newValue, RotationUpdateMode mode = RotationUpdateMode.UpdateEuler)
		{
			if(RotateZYX == newValue)
			{
				return;
			}

			RotateZYX = newValue;

			if(mode != RotationUpdateMode.Keep)
			{
				UpdateTransforms(null, null, null, null, mode);
			}
		}

		/// <summary>
		/// Sets all node attributes at the same time.
		/// </summary>
		/// <param name="attributes">The new attributes to set.</param>
		/// <param name="rotationUpdateMode">Determines how the rotation values of a node should be handled after the rotation order has been changed.</param>
		public void SetAllNodeAttributes(NodeAttributes attributes, RotationUpdateMode rotationUpdateMode = RotationUpdateMode.UpdateEuler)
		{
			SetRotationZYX(attributes.HasFlag(NodeAttributes.RotateZYX), rotationUpdateMode);
			Attributes = attributes;
		}
	}
}
