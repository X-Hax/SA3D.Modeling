using System.Numerics;

namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// Extends float and various float structures to allow for printing strings that are always the same length.
	/// </summary>
	public static class DebugStringExtensions
	{
		/// <summary>
		/// Creates an easily readable debug string for a float.
		/// </summary>
		/// <param name="val">The float to create the string for.</param>
		/// <returns>The debug string.</returns>
		public static string DebugString(this float val)
		{
			return (val >= 0 ? " " : string.Empty) + val.ToString("F3");
		}

		/// <summary>
		/// Creates an easily readable debug string for a quaternion.
		/// </summary>
		/// <param name="quat">The quaternion to create the string for.</param>
		/// <returns>The debug string.</returns>
		public static string DebugString(this Quaternion quat)
		{
			return $"({quat.W.DebugString()}, {quat.X.DebugString()}, {quat.Y.DebugString()}, {quat.Z.DebugString()})";
		}

		/// <summary>
		/// Creates an easily readable debug string for a vector.
		/// </summary>
		/// <param name="vector">The vector to create the string for.</param>
		/// <returns>The debug string.</returns>
		public static string DebugString(this Vector2 vector)
		{
			return $"({vector.X.DebugString()}, {vector.Y.DebugString()})";
		}

		/// <summary>
		/// Creates an easily readable debug string for a vector.
		/// </summary>
		/// <param name="vector">The vector to create the string for.</param>
		/// <returns>The debug string.</returns>
		public static string DebugString(this Vector3 vector)
		{
			return $"({vector.X.DebugString()}, {vector.Y.DebugString()}, {vector.Z.DebugString()})";
		}
	}
}
