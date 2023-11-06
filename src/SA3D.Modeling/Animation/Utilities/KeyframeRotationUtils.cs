using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Numerics;
using Matrix4x4KF = System.Collections.Generic.SortedDictionary<uint, System.Numerics.Matrix4x4>;
using QuaternionKF = System.Collections.Generic.SortedDictionary<uint, System.Numerics.Quaternion>;
using EulerKF = System.Collections.Generic.SortedDictionary<uint, System.Numerics.Vector3>;

namespace SA3D.Modeling.Animation.Utilities
{
	/// <summary>
	/// Utility methods for converting keyframe rotations from and to matrices and each other.
	/// </summary>
	public static class KeyframeRotationUtils
	{
		#region Quaternion -> Euler

		/// <summary>
		/// Converts quaternion rotation keyframes to euler rotation keyframes.
		/// </summary>
		/// <param name="source">The quaternion keyframes to convert.</param>
		/// <param name="deviationThreshold">The deviation threshold below which converted values should be ignored.</param>
		/// <param name="rotateZYX">Whether euler angles are applied in ZYX order.</param>
		/// <param name="result">Keyframes in which the result should be stored.</param>
		public static void QuaternionToEuler(QuaternionKF source, float deviationThreshold, bool rotateZYX, EulerKF result)
		{
			if(source.Count == 0)
			{
				return;
			}

			deviationThreshold = Math.Max(deviationThreshold, 0);

			uint previousFrame = uint.MaxValue;
			foreach(KeyValuePair<uint, Quaternion> item in source)
			{
				if(previousFrame != uint.MaxValue)
				{
					float frameCount = item.Key - previousFrame;

					Vector3 previousEuler = result[previousFrame];
					for(uint i = 1; i <= frameCount; i++)
					{
						float fac = i / frameCount;
						Quaternion lerp = Quaternion.Lerp(source[previousFrame], item.Value, fac);

						previousEuler = lerp.QuaternionToCompatibleEuler(previousEuler, rotateZYX);

						result.Add(previousFrame + i, previousEuler);
					}

					if(deviationThreshold > 0)
					{
						result.OptimizeVector3(deviationThreshold, previousFrame, item.Key);
					}
				}
				else
				{
					Vector3 rotation = item.Value.QuaternionToEuler(rotateZYX);
					result.Add(item.Key, rotation);
				}

				previousFrame = item.Key;
			}
		}

		/// <summary>
		/// Converts quaternion rotation keyframes to euler rotation keyframes.
		/// </summary>
		/// <param name="source">The quaternion keyframes to convert.</param>
		/// <param name="deviationThreshold">The deviation threshold below which converted values should be ignored.</param>
		/// <param name="rotateZYX">Whether euler angles are applied in ZYX order.</param>
		/// <returns>The converted euler rotation keyframes.</returns>
		public static EulerKF QuaternionToEuler(QuaternionKF source, float deviationThreshold, bool rotateZYX)
		{
			EulerKF result = new();
			QuaternionToEuler(source, deviationThreshold, rotateZYX, result);
			return result;
		}

		/// <summary>
		/// Converts quaternion rotation keyframes to euler rotation keyframes.
		/// </summary>
		/// <param name="keyframes">The keyframes to convert and output to.</param>
		/// <param name="deviationThreshold">The deviation threshold below which converted values should be ignored.</param>
		/// <param name="rotateZYX">Whether euler angles are applied in ZYX order.</param>
		/// <param name="clearQuaternion">Whether quaternion keyframes should be cleared after converting.</param>
		public static void QuaternionToEuler(this Keyframes keyframes, float deviationThreshold, bool rotateZYX, bool clearQuaternion)
		{
			keyframes.EulerRotation.Clear();
			QuaternionToEuler(keyframes.QuaternionRotation, deviationThreshold, rotateZYX, keyframes.EulerRotation);

			if(clearQuaternion)
			{
				keyframes.QuaternionRotation.Clear();
			}
		}

		#endregion


		#region Euler -> Quaternion

		/// <summary>
		/// Converts euler rotation keyframes to quaternion rotation keyframes.
		/// </summary>
		/// <param name="source">The euler keyframes to convert.</param>
		/// <param name="deviationThreshold">The deviation threshold below which converted values should be ignored.</param>
		/// <param name="rotateZYX">Whether euler angles are applied in ZYX order.</param>
		/// <param name="result">Keyframes in which the result should be stored.</param>
		public static void EulerToQuaternion(EulerKF source, float deviationThreshold, bool rotateZYX, QuaternionKF result)
		{
			if(source.Count == 0)
			{
				return;
			}

			deviationThreshold = Math.Max(deviationThreshold, 0);

			uint previousFrame = uint.MaxValue;
			foreach(KeyValuePair<uint, Vector3> item in source)
			{
				if(previousFrame != uint.MaxValue)
				{
					float frameCount = item.Key - previousFrame;

					for(uint i = 1; i <= frameCount; i++)
					{
						float fac = i / frameCount;
						Vector3 lerp = Vector3.Lerp(source[previousFrame], item.Value, fac);
						result.Add(previousFrame + i, lerp.EulerToQuaternion(rotateZYX));
					}

					if(deviationThreshold > 0)
					{
						result.OptimizeQuaternion(deviationThreshold, previousFrame, item.Key);
					}
				}
				else
				{
					Quaternion quaternion = item.Value.EulerToQuaternion(rotateZYX);
					result.Add(item.Key, quaternion);
				}

				previousFrame = item.Key;
			}
		}

		/// <summary>
		/// Converts euler rotation keyframes to quaternion rotation keyframes.
		/// </summary>
		/// <param name="source">The euler keyframes to convert.</param>
		/// <param name="deviationThreshold">The deviation threshold below which converted values should be ignored.</param>
		/// <param name="rotateZYX">Whether euler angles are applied in ZYX order.</param>
		/// <returns>The converted quaternion rotation keyframes.</returns>
		public static QuaternionKF EulerToQuaternion(EulerKF source, float deviationThreshold, bool rotateZYX)
		{
			QuaternionKF result = new();
			EulerToQuaternion(source, deviationThreshold, rotateZYX, result);
			return result;
		}

		/// <summary>
		/// Converts euler rotation keyframes to quaternion rotation keyframes.
		/// </summary>
		/// <param name="keyframes">The keyframes to convert and output to.</param>
		/// <param name="deviationThreshold">The deviation threshold below which converted values should be ignored.</param>
		/// <param name="rotateZYX">Whether euler angles are applied in ZYX order.</param>
		/// <param name="clearEuler">Whether euler keyframes should be cleared after converting.</param>
		public static void EulerToQuaternion(this Keyframes keyframes, float deviationThreshold, bool rotateZYX, bool clearEuler)
		{
			keyframes.QuaternionRotation.Clear();
			EulerToQuaternion(keyframes.EulerRotation, deviationThreshold, rotateZYX, keyframes.QuaternionRotation);

			if(clearEuler)
			{
				keyframes.EulerRotation.Clear();
			}
		}

		#endregion


		#region Euler / Quaternion -> Matrix

		private static Matrix4x4[]? GetComplementaryMatrices(Vector3 previous, Vector3 current, bool rotateZYX)
		{
			Vector3 dif = current - previous;
			float maxDif = Vector3.Abs(dif).GreatestValue();

			int complementary_len = (int)MathF.Floor(maxDif / MathF.PI);
			if(complementary_len == 0)
			{
				return null;
			}

			complementary_len++;
			float dif_fac = 1.0f / (complementary_len + 1);

			Matrix4x4[] result = new Matrix4x4[complementary_len];

			for(int i = 0; i < complementary_len; i++)
			{
				Vector3 compl_euler = previous + (dif * (dif_fac * (i + 1)));
				result[i] = MatrixUtilities.CreateRotationMatrix(compl_euler, rotateZYX);
			}

			return result;
		}


		/// <summary>
		/// Converts rotation keyframes to rotation matrices. Will use Euler or Quaternion rotations. If both are present, euler is used regardless.
		/// </summary>
		/// <param name="keyframes">The keyframes to convert.</param>
		/// <param name="targetQuaternion">Whether the result is intended to be handled like quaternion rotations.</param>
		/// <param name="deviationThreshold">The deviation threshold below which converted values should be ignored.</param>
		/// <param name="rotateZYX">Whether euler angles are applied in ZYX order.</param>
		/// <param name="converted">Whether the output was converted between euler and quaternion</param>
		/// <param name="complementary">Complementary matrices for quaternion target.</param>
		/// <returns>The converted rotation matrix keyframes.</returns>
		public static Matrix4x4KF GetRotationMatrices(this Keyframes keyframes, bool targetQuaternion, float deviationThreshold, bool rotateZYX, out bool converted, out Dictionary<uint, Matrix4x4[]>? complementary)
		{
			Matrix4x4KF result = new();
			complementary = null;
			converted = false;

			if(keyframes.EulerRotation.Count == 0 && keyframes.QuaternionRotation.Count == 0)
			{
				return result;
			}

			if(targetQuaternion)
			{
				QuaternionKF output = keyframes.QuaternionRotation;

				if(keyframes.EulerRotation.Count > 0) // If eulers exist, convert to quaternion regardless of whether quaternions had values before
				{
					converted = true;
					output = EulerToQuaternion(keyframes.EulerRotation, deviationThreshold, rotateZYX);
				}

				foreach(KeyValuePair<uint, Quaternion> quaternion in output)
				{
					result.Add(quaternion.Key, Matrix4x4.CreateFromQuaternion(quaternion.Value));
				}
			}
			else
			{
				EulerKF output = keyframes.EulerRotation;

				if(output.Count == 0)
				{
					converted = true;
					output = QuaternionToEuler(keyframes.QuaternionRotation, deviationThreshold, rotateZYX);
				}

				Vector3? previous = null;
				uint previousFrame = 0;
				complementary = new();

				foreach(KeyValuePair<uint, Vector3> rotation in output)
				{
					result.Add(rotation.Key, MatrixUtilities.CreateRotationMatrix(rotation.Value, rotateZYX));

					if(previous != null)
					{
						Matrix4x4[]? compl_matrices = GetComplementaryMatrices(previous.Value, rotation.Value, rotateZYX);
						if(compl_matrices != null)
						{
							complementary.Add(previousFrame, compl_matrices);
						}
					}

					previous = rotation.Value;
					previousFrame = rotation.Key;
				}

				if(complementary.Count == 0)
				{
					complementary = null;
				}
			}

			return result;
		}

		private static void ConvertMatrixToQuaternion(Matrix4x4KF source, QuaternionKF result)
		{
			foreach(KeyValuePair<uint, Matrix4x4> item in source)
			{
				Matrix4x4.Decompose(item.Value, out _, out Quaternion value, out _);
				result.Add(item.Key, value);
			}
		}

		private static void ConvertMatrixToRotation(Matrix4x4KF source, bool rotateZYX, Dictionary<uint, Matrix4x4[]>? complementary, EulerKF result)
		{
			Vector3 previousEuler = default;

			foreach(KeyValuePair<uint, Matrix4x4> item in source)
			{
				previousEuler = MatrixUtilities.ToCompatibleEuler(item.Value, previousEuler, rotateZYX);
				result.Add(item.Key, previousEuler);

				if(complementary?.TryGetValue(item.Key, out Matrix4x4[]? matrices) == true)
				{
					for(int i = 0; i < matrices.Length; i++)
					{
						previousEuler = MatrixUtilities.ToCompatibleEuler(matrices[i], previousEuler, rotateZYX);
					}
				}
			}
		}

		#endregion


		#region Matrix -> Quaternion

		/// <summary>
		/// Converts rotation matrix keyframes to quaternion rotation keyframes.
		/// </summary>
		/// <param name="source">Rotation matrix keyframes to convert.</param>
		/// <param name="wasQuaternion">Whether the matrices should be handled as quaternion rotations.</param>
		/// <param name="deviationThreshold">The deviation threshold below which converted values should be ignored.</param>
		/// <param name="rotateZYX">Whether the euler angles should be applied in ZYX order.</param>
		/// <param name="result">The keyframes to output to.</param>
		public static void MatrixToQuaternion(Matrix4x4KF source, bool wasQuaternion, float deviationThreshold, bool rotateZYX, QuaternionKF result)
		{
			if(wasQuaternion)
			{
				ConvertMatrixToQuaternion(source, result);
			}
			else
			{
				EulerKF rotations = new();
				ConvertMatrixToRotation(source, rotateZYX, null, rotations);
				EulerToQuaternion(rotations, deviationThreshold, rotateZYX, result);
			}
		}

		/// <summary>
		/// Converts rotation matrix keyframes to quaternion rotation keyframes.
		/// </summary>
		/// <param name="source">Rotation matrix keyframes to convert.</param>
		/// <param name="wasQuaternion">Whether the matrices should be handled as quaternion rotations.</param>
		/// <param name="deviationThreshold">The deviation threshold below which converted values should be ignored.</param>
		/// <param name="rotateZYX">Whether the euler angles should be applied in ZYX order.</param>
		/// <returns>The converted quaternion keyframes.</returns>
		public static QuaternionKF MatrixToQuaternion(Matrix4x4KF source, bool wasQuaternion, float deviationThreshold, bool rotateZYX)
		{
			QuaternionKF result = new();
			MatrixToQuaternion(source, wasQuaternion, deviationThreshold, rotateZYX, result);
			return result;
		}

		/// <summary>
		/// Converts rotation matrix keyframes to quaternion rotation keyframes.
		/// </summary>
		/// <param name="keyframes">The keyframes to store the converted rotation into.</param>
		/// <param name="source">Rotation matrix keyframes to convert.</param>
		/// <param name="wasQuaternion">Whether the matrices should be handled as quaternion rotations.</param>
		/// <param name="deviationThreshold">The deviation threshold below which converted values should be ignored.</param>
		/// <param name="rotateZYX">Whether the euler angles should be applied in ZYX order.</param>
		public static void MatrixToQuaternion(this Keyframes keyframes, Matrix4x4KF source, bool wasQuaternion, float deviationThreshold, bool rotateZYX)
		{
			keyframes.QuaternionRotation.Clear();
			MatrixToQuaternion(source, wasQuaternion, deviationThreshold, rotateZYX, keyframes.QuaternionRotation);
		}

		#endregion


		#region Matrix -> Euler

		/// <summary>
		/// Converts rotation matrix keyframes to euler rotation keyframes.
		/// </summary>
		/// <param name="source">Rotation matrix keyframes to convert.</param>
		/// <param name="wasQuaternion">Whether the matrices should be handled as quaternion rotations.</param>
		/// <param name="deviationThreshold">The deviation threshold below which converted values should be ignored.</param>
		/// <param name="rotateZYX">Whether the euler angles should be applied in ZYX order.</param>
		/// <param name="complementary">Rotation matrices to be applied in between keyframes. Used for achieving angle differences greater than 180 degrees.</param>
		/// <param name="result">The keyframes to output to.</param>
		public static void MatrixToEuler(Matrix4x4KF source, bool wasQuaternion, float deviationThreshold, bool rotateZYX, Dictionary<uint, Matrix4x4[]>? complementary, EulerKF result)
		{
			if(wasQuaternion)
			{
				QuaternionKF quaternions = new();
				ConvertMatrixToQuaternion(source, quaternions);
				QuaternionToEuler(quaternions, deviationThreshold, rotateZYX, result);
			}
			else
			{
				ConvertMatrixToRotation(source, rotateZYX, complementary, result);
			}
		}

		/// <summary>
		/// Converts rotation matrix keyframes to euler rotation keyframes.
		/// </summary>
		/// <param name="source">Rotation matrix keyframes to convert.</param>
		/// <param name="wasQuaternion">Whether the matrices should be handled as quaternion rotations.</param>
		/// <param name="deviationThreshold">The deviation threshold below which converted values should be ignored.</param>
		/// <param name="rotateZYX">Whether the euler angles should be applied in ZYX order.</param>
		/// <param name="complementary">Rotation matrices to be applied in between keyframes. Used for achieving angle differences greater than 180 degrees.</param>
		/// <returns>The converted euler keyframes.</returns>
		public static EulerKF MatrixToEuler(Matrix4x4KF source, bool wasQuaternion, float deviationThreshold, bool rotateZYX, Dictionary<uint, Matrix4x4[]>? complementary)
		{
			EulerKF result = new();
			MatrixToEuler(source, wasQuaternion, deviationThreshold, rotateZYX, complementary, result);
			return result;
		}

		/// <summary>
		/// Converts rotation matrix keyframes to euler rotation keyframes.
		/// </summary>
		/// <param name="keyframes">The keyframes to store the converted rotation into.</param>
		/// <param name="source">Rotation matrix keyframes to convert.</param>
		/// <param name="wasQuaternion">Whether the matrices should be handled as quaternion rotations.</param>
		/// <param name="deviationThreshold">The deviation threshold below which converted values should be ignored.</param>
		/// <param name="rotateZYX">Whether the euler angles should be applied in ZYX order.</param>
		/// <param name="complementary">Rotation matrices to be applied in between keyframes. Used for achieving angle differences greater than 180 degrees.</param>
		public static void MatrixToEuler(this Keyframes keyframes, Matrix4x4KF source, bool wasQuaternion, float deviationThreshold, bool rotateZYX, Dictionary<uint, Matrix4x4[]>? complementary)
		{
			keyframes.EulerRotation.Clear();
			MatrixToEuler(source, wasQuaternion, deviationThreshold, rotateZYX, complementary, keyframes.EulerRotation);
		}

		#endregion
	}
}
