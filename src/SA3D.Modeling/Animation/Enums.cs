using SA3D.Common;
using System;

namespace SA3D.Modeling.Animation
{
	/// <summary>
	/// Depicts keyframe contents.
	/// </summary>
	[Flags]
	public enum KeyframeAttributes : ushort
	{
		/// <summary>
		/// Animation includes position keyframes.
		/// </summary>
		Position = Flag16.B0,

		/// <summary>
		/// Animation includes rotation (euler angles) keyframes.
		/// </summary>
		EulerRotation = Flag16.B1,

		/// <summary>
		/// Animation includes scale keyframes.
		/// </summary>
		Scale = Flag16.B2,

		/// <summary>
		/// Animation includes vector keyframes.
		/// </summary>
		Vector = Flag16.B3,

		/// <summary>
		/// Animation includes vertex keyframes.
		/// </summary>
		Vertex = Flag16.B4,

		/// <summary>
		/// Animation includes normal keyframes.
		/// </summary>
		Normal = Flag16.B5,

		/// <summary>
		/// Animation includes target keyframes.
		/// </summary>
		Target = Flag16.B6,

		/// <summary>
		/// Animation includes roll keyframes.
		/// </summary>
		Roll = Flag16.B7,

		/// <summary>
		/// Animation includes angle keyframes.
		/// </summary>
		Angle = Flag16.B8,

		/// <summary>
		/// Animation includes light color keyframes.
		/// </summary>
		LightColor = Flag16.B9,

		/// <summary>
		/// Animation includes intensity keyframes.
		/// </summary>
		Intensity = Flag16.B10,

		/// <summary>
		/// Animation includes spotlight keyframes.
		/// </summary>
		Spot = Flag16.B11,

		/// <summary>
		/// Animation includes point keyframes.
		/// </summary>
		Point = Flag16.B12,

		/// <summary>
		/// Animation includes rotation (quaternion) keyframes.
		/// </summary>
		QuaternionRotation = Flag16.B13
	}

	/// <summary>
	/// Keyframe interpolation mode.
	/// </summary>
	public enum InterpolationMode
	{
		/// <summary>
		/// Linear interpolation.
		/// </summary>
		Linear,

		/// <summary>
		/// Spline interpolation (?).
		/// </summary>
		Spline,

		/// <summary>
		/// User defined interpolation.
		/// </summary>
		User
	}

	/// <summary>
	/// Animation enum extension methods.
	/// </summary>
	public static class EnumExtensions
	{
		/// <summary>
		/// Counts the number of channels defined in animation attributes.
		/// </summary>
		/// <param name="attributes">Animation attributes to count.</param>
		/// <returns>The number of channel defined in animation attributes.</returns>
		public static int ChannelCount(this KeyframeAttributes attributes)
		{
			int channels = 0;

			ushort value = (ushort)attributes;
			for(int i = 0; i < 14; i++, value >>= 1)
			{
				if((value & 1) != 0)
				{
					channels++;
				}
			}

			return channels;
		}
	}
}
