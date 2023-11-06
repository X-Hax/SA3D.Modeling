using System;
using System.Numerics;

namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// Matrix related utility methods.
	/// </summary>
	public static class MatrixUtilities
	{
		/// <summary>
		/// Creates a rotation matrix from euler angles.
		/// </summary>
		/// <param name="rotation">The euler angles to use.</param>
		/// <param name="ZYX">Whether the euler angles are used in ZYX order.</param>
		/// <returns></returns>
		public static Matrix4x4 CreateRotationMatrix(Vector3 rotation, bool ZYX)
		{
			float radX = rotation.X;
			float radY = rotation.Y;
			float radZ = rotation.Z;

			float sX = MathF.Sin(radX);
			float cX = MathF.Cos(radX);

			float sY = MathF.Sin(radY);
			float cY = MathF.Cos(radY);

			float sZ = MathF.Sin(radZ);
			float cZ = MathF.Cos(radZ);

			if(ZYX)
			{
				// Well, in sa2 it rotates in ZXY order, so thats what we do here. dont ask me...
				// equal to matZ * matX * matY
				return new()
				{
					M11 = (sY * sX * sZ) + (cY * cZ),
					M12 = cX * sZ,
					M13 = (cY * sX * sZ) - (sY * cZ),

					M21 = (sY * sX * cZ) - (cY * sZ),
					M22 = cX * cZ,
					M23 = (cY * sX * cZ) + (sY * sZ),

					M31 = sY * cX,
					M32 = -sX,
					M33 = cY * cX,

					M44 = 1
				};
			}
			else
			{
				// equal to matX * matY * matZ
				return new()
				{
					M11 = cZ * cY,
					M12 = sZ * cY,
					M13 = -sY,

					M21 = (cZ * sY * sX) - (sZ * cX),
					M22 = (sZ * sY * sX) + (cZ * cX),
					M23 = cY * sX,

					M31 = (cZ * sY * cX) + (sZ * sX),
					M32 = (sZ * sY * cX) - (cZ * sX),
					M33 = cY * cX,

					M44 = 1
				};
			}
		}

		/// <summary>
		/// Creates a transform matrix from a position, rotation and scale.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="rotation">The quaternion rotation.</param>
		/// <param name="scale">The scale.</param>
		/// <returns>The transform matrix.</returns>
		public static Matrix4x4 CreateTransformMatrix(Vector3 position, Quaternion rotation, Vector3 scale)
		{
			return Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(rotation) * Matrix4x4.CreateTranslation(position);
		}

		/// <summary>
		/// Creates a transform matrix from a position, euler angles and scale.
		/// </summary>
		/// <param name="position">The position.</param>
		/// <param name="rotation">The euler angles.</param>
		/// <param name="scale">The scale.</param>
		/// <param name="rotateZYX">Whether the euler angles are used in ZYX order.</param>
		/// <returns>The transform matrix.</returns>
		public static Matrix4x4 CreateTransformMatrix(Vector3 position, Vector3 rotation, Vector3 scale, bool rotateZYX)
		{
			return Matrix4x4.CreateScale(scale) * CreateRotationMatrix(rotation, rotateZYX) * Matrix4x4.CreateTranslation(position);
		}

		/// <summary>
		/// Returns the normal matrix for a transform matrix.
		/// </summary>
		/// <param name="matrix">The transform matrix to get the normal matrix for.</param>
		/// <returns>The normal matrix.</returns>
		/// <exception cref="InvalidOperationException"/>
		public static Matrix4x4 GetNormalMatrix(this Matrix4x4 matrix)
		{
			if(!Matrix4x4.Invert(matrix, out Matrix4x4 result))
			{
				throw new InvalidOperationException("Matrix failed to invert.");
			}

			result = Matrix4x4.Transpose(result);
			return result;
		}


		private static (Vector3 a, Vector3 b) MatrixToNormalizedEuler2(Matrix4x4 matrix, bool rotateZYX)
		{
			Vector3 a, b;

			if(rotateZYX)
			{
				float cx = float.Hypot(matrix.M33, matrix.M31);

				if(cx > 16f * 1.192092896e-07F)
				{
					a = new(
						MathF.Atan2(-matrix.M32, cx),
						MathF.Atan2(matrix.M31, matrix.M33),
						MathF.Atan2(matrix.M12, matrix.M22)
					);

					b = new(
						MathF.Atan2(-matrix.M32, -cx),
						MathF.Atan2(-matrix.M31, -matrix.M33),
						MathF.Atan2(-matrix.M12, -matrix.M22)
					);
				}
				else
				{
					a = b = new(
						MathF.Atan2(-matrix.M32, cx),
						MathF.Atan2(-matrix.M21, matrix.M11),
						0f
					);
				}
			}
			else
			{
				float cy = float.Hypot(matrix.M11, matrix.M12);

				if(cy > 16f * 1.192092896e-07F)
				{
					a = new(
						MathF.Atan2(matrix.M23, matrix.M33),
						MathF.Atan2(-matrix.M13, cy),
						MathF.Atan2(matrix.M12, matrix.M11)
					);

					b = new(
						MathF.Atan2(-matrix.M23, -matrix.M33),
						MathF.Atan2(-matrix.M13, -cy),
						MathF.Atan2(-matrix.M12, -matrix.M11)
					);
				}
				else
				{
					a = new(
						MathF.Atan2(-matrix.M32, matrix.M22),
						MathF.Atan2(-matrix.M13, cy),
						0f
					);
					b = a;
				}
			}

			return (a, b);
		}

		private static Vector3 CompatibleEuler(Vector3 rotation, Vector3 previous)
		{
			const float piThreshold = 5.1f;
			const float pi2 = 2 * MathF.PI;

			Vector3 dif = rotation - previous;
			for(int i = 0; i < 3; i++)
			{
				if(dif[i] > piThreshold)
				{
					rotation[i] -= MathF.Floor((dif[i] / pi2) + 0.5f) * pi2;
				}
				else if(dif[i] < piThreshold)
				{
					rotation[i] += MathF.Floor((-dif[i] / pi2) + 0.5f) * pi2;
				}
			}

			dif = rotation - previous;

			for(int i = 0; i < 3; i++)
			{
				if(MathF.Abs(dif[i]) > 3.2f
					&& MathF.Abs(dif[(i + 1) % 3]) < 1.6f
					&& MathF.Abs(dif[(i + 2) % 3]) < 1.6f)
				{
					rotation[i] += dif[i] > 0.0 ? -pi2 : pi2;
				}

			}

			return rotation;
		}

		/// <summary>
		/// Converts a matrix to euler angles relative to pre-existing euler angles, which ensures that there is no more than half a rotation between the two eulers.
		/// </summary>
		/// <param name="matrix">The matrix to convert.</param>
		/// <param name="previous">The euler to be compatible to.</param>
		/// <param name="rotateZYX">Whether the euler angles should be in ZYX order.</param>
		/// <returns></returns>
		public static Vector3 ToCompatibleEuler(Matrix4x4 matrix, Vector3 previous, bool rotateZYX)
		{
			(Vector3 a, Vector3 b) = MatrixToNormalizedEuler2(matrix, rotateZYX);

			a = CompatibleEuler(a, previous);
			b = CompatibleEuler(b, previous);

			float d1 = MathF.Abs(a.X - previous.X) + MathF.Abs(a.Y - previous.Y) + MathF.Abs(a.Z - previous.Z);
			float d2 = MathF.Abs(b.X - previous.X) + MathF.Abs(b.Y - previous.Y) + MathF.Abs(b.Z - previous.Z);

			return d1 > d2 ? b : a;
		}

		/// <summary>
		/// Converts a quaternion to euler angles.
		/// </summary>
		/// <param name="matrix">The matrix to convert.</param>
		/// <param name="rotateZYX">Whether the euler angles should be in ZYX order.</param>
		/// <returns>The converted euler.</returns>
		public static Vector3 ToEuler(Matrix4x4 matrix, bool rotateZYX)
		{
			(Vector3 a, _) = MatrixToNormalizedEuler2(matrix, rotateZYX);
			return a;
		}
	}
}
