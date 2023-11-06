using System;
using System.Numerics;
using static SA3D.Common.MathHelper;

namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// Quaternion related utility methods.
	/// </summary>
	public static class QuaternionUtilities
	{
		/// <summary>
		/// Converts a quaternion to euler angles.
		/// </summary>
		/// <param name="quaternion">The quaternion to convert.</param>
		/// <param name="rotateZYX">Whether the euler angles should be in ZYX order.</param>
		/// <returns>The converted euler.</returns>
		public static Vector3 QuaternionToEuler(this Quaternion quaternion, bool rotateZYX)
		{
			// normalize the values
			Quaternion vN = Quaternion.Normalize(quaternion);
			float x = vN.X;
			float y = vN.Y;
			float z = vN.Z;
			float w = vN.W;

			// this will have a magnitude of 0.5 or greater if and only if this is a singularity case
			Vector3 v = new();
			if(rotateZYX)
			{
				float test = (w * x) - (y * z);

				if(test > 0.4995f) // singularity at north pole
				{
					v.Z = 2f * MathF.Atan2(z, x);
					v.X = HalfPi;
				}
				else if(test < -0.4995f) // singularity at south pole
				{
					v.Z = -2f * MathF.Atan2(z, x);
					v.X = -HalfPi;
				}
				else
				{
					v.X = MathF.Asin(2f * test);
					v.Y = MathF.Atan2(2f * ((w * y) + (z * x)), 1 - (2f * ((y * y) + (x * x))));
					v.Z = MathF.Atan2(2f * ((w * z) + (y * x)), 1 - (2f * ((z * z) + (x * x))));
				}

			}
			else
			{
				float test = (w * y) - (x * z);

				if(test > 0.4995f) // singularity at north pole
				{
					v.X = 2f * MathF.Atan2(x, y);
					v.Y = HalfPi;
				}
				else if(test < -0.4995f) // singularity at south pole
				{
					v.X = -2f * MathF.Atan2(x, y);
					v.Y = -HalfPi;
				}
				else
				{
					v.X = MathF.Atan2(2f * ((w * x) + (z * y)), 1 - (2f * ((y * y) + (x * x))));
					v.Y = MathF.Asin(2f * test);
					v.Z = MathF.Atan2(2f * ((w * z) + (y * x)), 1 - (2f * ((z * z) + (y * y))));
				}
			}

			static void normalize(ref float t)
			{
				t %= float.Tau;
				if(t < -float.Pi)
				{
					t += float.Tau;
				}
				else if(t > float.Pi)
				{
					t -= float.Tau;
				}
			}

			normalize(ref v.X);
			normalize(ref v.Y);
			normalize(ref v.Z);
			return v;
		}

		/// <summary>
		/// Converts a quaternion to euler angles relative to pre-existing euler angles, which ensures that there is no more than half a rotation between the two eulers.
		/// </summary>
		/// <param name="rotation">The quaternion to convert.</param>
		/// <param name="previous">The euler to be compatible to.</param>
		/// <param name="rotateZYX">Whether the euler angles should be in ZYX order.</param>
		/// <returns></returns>
		public static Vector3 QuaternionToCompatibleEuler(this Quaternion rotation, Vector3 previous, bool rotateZYX)
		{
			Matrix4x4 matrix = Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(rotation));
			return MatrixUtilities.ToCompatibleEuler(matrix, previous, rotateZYX);
		}

		/// <summary>
		/// Converts euler angles to a quaternion.
		/// </summary>
		/// <param name="rotation">The euler angles to convert.</param>
		/// <param name="rotateZYX">Whether the euler angles are applied in ZYX order.</param>
		/// <returns>The converted Quaternion.</returns>
		public static Quaternion EulerToQuaternion(this Vector3 rotation, bool rotateZYX)
		{
			Matrix4x4 mtx = MatrixUtilities.CreateRotationMatrix(rotation, rotateZYX);
			return Quaternion.CreateFromRotationMatrix(mtx);
		}


		/// <summary>
		/// Lerps the individual components of two quaternions without additional checks.
		/// </summary>
		/// <param name="from">The quaternion from which to start interpolating.</param>
		/// <param name="to">The quaternion to interpolate to.</param>
		/// <param name="t">The time value to interpolate by.</param>
		/// <returns>The interpolated quaternion.</returns>
		public static Quaternion RealLerp(Quaternion from, Quaternion to, float t)
		{
			float t1 = 1 - t;
			return new(
				(t1 * from.X) + (t * to.X),
				(t1 * from.Y) + (t * to.Y),
				(t1 * from.Z) + (t * to.Z),
				(t1 * from.W) + (t * to.W));
		}
	}
}

