using System.Numerics;

namespace SA3D.Modeling.ObjectData.Events
{
	/// <summary>
	/// Stores 3D transform data in local space.
	/// </summary>
	public readonly struct TransformSet
	{
		/// <summary>
		/// Localspace matrix.
		/// </summary>
		public Matrix4x4 LocalMatrix { get; }

		/// <summary>
		/// Localspace position.
		/// </summary>
		public Vector3 Position { get; }

		/// <summary>
		/// Localspace euler angles.
		/// </summary>
		public Vector3 Rotation { get; }

		/// <summary>
		/// Localspace quaternion.
		/// </summary>
		public Quaternion QuaternionRotation { get; }

		/// <summary>
		/// Localspace scale.
		/// </summary>
		public Vector3 Scale { get; }

		/// <summary>
		/// Creates a new transform set.
		/// </summary>
		/// <param name="matrix">Localspace matrix.</param>
		/// <param name="position">Localspace position.</param>
		/// <param name="rotation">Localspace euler angles.</param>
		/// <param name="quaternionRotation">Localspace quaternion.</param>
		/// <param name="scale">Localspace scale.</param>
		public TransformSet(Matrix4x4 matrix, Vector3 position, Vector3 rotation, Quaternion quaternionRotation, Vector3 scale)
		{
			LocalMatrix = matrix;
			Position = position;
			Rotation = rotation;
			QuaternionRotation = quaternionRotation;
			Scale = scale;
		}

		/// <summary>
		/// Creates a new transform set from a nodes transform properties.
		/// </summary>
		/// <param name="node">The node to get the transform properties of.</param>
		/// <returns>The transform set.</returns>
		public static TransformSet FromNode(Node node)
		{
			return new(
				node.LocalMatrix,
				node.Position,
				node.EulerRotation,
				node.QuaternionRotation,
				node.Scale);
		}
	}
}
