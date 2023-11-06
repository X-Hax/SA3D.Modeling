namespace SA3D.Modeling.ObjectData.Enums
{
	/// <summary>
	/// Determines how the rotation values of a node should be handled after the rotation order has been changed.
	/// </summary>
	public enum RotationUpdateMode
	{
		/// <summary>
		/// Keeps the euler angles but updates the quaternion rotation based on the new order.
		/// </summary>
		UpdateQuaternion,

		/// <summary>
		/// Keeps the quaternion rotation but updates the euler to represent the same final rotation based on the new order.
		/// </summary>
		UpdateEuler,

		/// <summary>
		/// Keeps animation values as they are. (not recommended)
		/// </summary>
		Keep,
	}
}
