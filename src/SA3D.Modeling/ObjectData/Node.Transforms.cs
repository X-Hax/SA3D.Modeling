using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.ObjectData.Events;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.ObjectData
{
	public partial class Node
	{
		private Vector3 _position;
		private Vector3 _eulerRotation;
		private Quaternion _quaternionRotation = Quaternion.Identity;
		private Vector3 _scale = Vector3.One;

		/// <summary>
		/// Position in localspace.
		/// </summary>
		public Vector3 Position
		{
			get => _position;
			set => UpdateTransforms(value, null, null, null, RotationUpdateMode.Keep);
		}

		/// <summary>
		/// Euler angles rotation in localspace (radians).
		/// <br/> Affects <see cref="QuaternionRotation"/>.
		/// </summary>
		public Vector3 EulerRotation
		{
			get => _eulerRotation;
			set => UpdateTransforms(null, value, null, null, RotationUpdateMode.Keep);
		}

		/// <summary>
		/// Quaternion rotation in local space.
		/// <br/> Affects <see cref="EulerRotation"/>.
		/// </summary>
		public Quaternion QuaternionRotation
		{
			get => _quaternionRotation;
			set => UpdateTransforms(null, null, value, null, RotationUpdateMode.Keep);
		}

		/// <summary>
		/// Scale in local space.
		/// </summary>
		public Vector3 Scale
		{
			get => _scale;
			set => UpdateTransforms(null, null, null, value, RotationUpdateMode.Keep);
		}

		/// <summary>
		/// Matrix representation of the localspace transform channels.
		/// </summary>
		public Matrix4x4 LocalMatrix { get; private set; } = Matrix4x4.Identity;

		/// <summary>
		/// Raised whenever any transform property changes.
		/// </summary>
		public event TransformsUpdatedEventHandler? OnTransformsUpdated;


		private void UpdateTransforms(Vector3? position, Vector3? eulerRotation, Quaternion? quaternionRotation, Vector3? scale, RotationUpdateMode rotationUpdateMode)
		{
			TransformSet oldTransforms = TransformSet.FromNode(this);
			UpdatedTransformValue updated = default;

			if(position != null)
			{
				_position = position.Value;
				updated |= UpdatedTransformValue.Position;
			}

			if(eulerRotation != null)
			{
				_eulerRotation = eulerRotation.Value;
				_quaternionRotation = QuaternionUtilities.EulerToQuaternion(_eulerRotation, RotateZYX);
				updated |= UpdatedTransformValue.Rotation;
			}

			if(quaternionRotation != null)
			{
				_quaternionRotation = quaternionRotation.Value;
				_eulerRotation = QuaternionUtilities.QuaternionToEuler(_quaternionRotation, RotateZYX);
				updated |= UpdatedTransformValue.Rotation;
			}

			if(scale != null)
			{
				_scale = scale.Value;
				updated |= UpdatedTransformValue.Scale;
			}

			switch(rotationUpdateMode)
			{
				case RotationUpdateMode.UpdateQuaternion:
					_quaternionRotation = QuaternionUtilities.EulerToQuaternion(_eulerRotation, RotateZYX);
					break;
				case RotationUpdateMode.UpdateEuler:
					_eulerRotation = QuaternionUtilities.QuaternionToEuler(_quaternionRotation, RotateZYX);
					break;
				case RotationUpdateMode.Keep:
				default:
					break;
			}

			if(updated == default)
			{
				return;
			}

			LocalMatrix = MatrixUtilities.CreateTransformMatrix(_position, _quaternionRotation, _scale);

			TransformSet newTransforms = TransformSet.FromNode(this);
			OnTransformsUpdated?.Invoke(this, new(oldTransforms, newTransforms, updated));
		}

		/// <summary>
		/// Updates multiple transforms at the same time.
		/// </summary>
		/// <param name="position">New position.</param>
		/// <param name="eulerRotation">New euler angles.</param>
		/// <param name="scale">New scale.</param>
		public void UpdateTransforms(Vector3? position, Vector3? eulerRotation, Vector3? scale)
		{
			UpdateTransforms(position, eulerRotation, null, scale, RotationUpdateMode.Keep);
		}

		/// <summary>
		/// Updates multiply transforms at the same time.
		/// </summary>
		/// <param name="position">New position.</param>
		/// <param name="quaternionRotation">New quaternion rotation.</param>
		/// <param name="scale">New scale.</param>
		public void UpdateTransforms(Vector3? position, Quaternion? quaternionRotation, Vector3? scale)
		{
			UpdateTransforms(position, null, quaternionRotation, scale, RotationUpdateMode.Keep);
		}


		/// <summary>
		/// Calculates the world matrix for this object (recursive).
		/// </summary>
		/// <returns></returns>
		public Matrix4x4 GetWorldMatrix()
		{
			Matrix4x4 local = LocalMatrix;

			if(Parent != null)
			{
				local *= Parent.GetWorldMatrix();
			}

			return local;
		}

		/// <summary>
		/// Returns an enumerable that iterates over the entire tree, starting at this node.
		/// <br/> Works like <see cref="GetTreeNodeEnumerable"/>, but includes the world matrix too without recursive calculations.
		/// </summary>
		public IEnumerable<(Node node, Matrix4x4 worldMatrix)> GetWorldMatrixTreeEnumerator()
		{
			Stack<(Node node, Matrix4x4 parentWorldMatrix)> nmStack = new();

			nmStack.Push((this, Matrix4x4.Identity));

			while(nmStack.TryPop(out (Node node, Matrix4x4 parentWorldMatrix) nm))
			{
				Matrix4x4 worldMatrix = nm.node.LocalMatrix * nm.parentWorldMatrix;
				yield return (nm.node, worldMatrix);

				if(nm.node.Next != null)
				{
					nmStack.Push((nm.node.Next, nm.parentWorldMatrix));
				}

				if(nm.node.Child != null)
				{
					nmStack.Push((nm.node.Child, worldMatrix));
				}
			}
		}

		/// <summary>
		/// Returns the node tree with their corresponding world matrices. Does not utilize recursiveness.
		/// </summary>
		public (Node node, Matrix4x4 worldMatrix)[] GetWorldMatrixTree()
		{
			return GetWorldMatrixTreeEnumerator().ToArray();
		}

		/// <summary>
		/// Returns the node tree with their corresponding world matrices in a dictionary. Does not utilize recursiveness.
		/// </summary>
		/// <returns></returns>
		public Dictionary<Node, Matrix4x4> GetWorldMatrixTreeLUT()
		{
			Dictionary<Node, Matrix4x4> result = new();
			foreach((Node node, Matrix4x4 worldMatrix) in GetWorldMatrixTreeEnumerator())
			{
				result.Add(node, worldMatrix);
			}

			return result;
		}

	}
}
