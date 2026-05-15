using SA3D.Modeling.Structs;
using System.Numerics;

namespace SA3D.Modeling.Animation
{
	/// <summary>
	/// Frame on timeline with interpolated values from a keyframe storage.
	/// </summary>
	public struct Frame
	{
		/// <summary>
		/// Position on the timeline.
		/// </summary>
		public float FrameTime { get; set; }

		/// <summary>
		/// Position at the frame.
		/// </summary>
		public Vector3? Position { get; set; }

		/// <summary>
		/// Rotation (euler angles) at the frame.
		/// </summary>
		public Vector3? EulerRotation { get; set; }

		/// <summary>
		/// Scale at the frame.
		/// </summary>
		public Vector3? Scale { get; set; }

		/// <summary>
		/// Vector at the frame.
		/// </summary>
		public Vector3? Vector { get; set; }

		/// <summary>
		/// Vertex positions at the frame.
		/// </summary>
		public Vector3[]? Vertex { get; set; }

		/// <summary>
		/// Vertex normals at the frame.
		/// </summary>
		public Vector3[]? Normal { get; set; }

		/// <summary>
		/// Camera target position at the frame.
		/// </summary>
		public Vector3? Target { get; set; }

		/// <summary>
		/// Camera roll at the frame.
		/// </summary>
		public float? Roll { get; set; }

		/// <summary>
		/// Camera FOV at the frame.
		/// </summary>
		public float? Angle { get; set; }

		/// <summary>
		/// Light color at the frame.
		/// </summary>
		public Color? Color { get; set; }

		/// <summary>
		/// Light intensity at the frame.
		/// </summary>
		public float? Intensity { get; set; }

		/// <summary>
		/// Spotlight at the frame.
		/// </summary>
		public Spotlight? Spotlight { get; set; }

		/// <summary>
		/// Point light stuff at the frame.
		/// </summary>
		public Vector2? Point { get; set; }

		/// <summary>
		/// Rotation (quaternion) at the frame.
		/// </summary>
		public Quaternion? QuaternionRotation { get; set; }

	}
}
