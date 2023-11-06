using SA3D.Common;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// Vector 3 Extensions
	/// </summary>
	public static class VectorUtilities
	{
		/// <summary>
		/// Returns the greatest of the 3 values in a vector.
		/// </summary>
		public static float GreatestValue(this Vector3 vector)
		{
			float r = vector.X;
			if(vector.Y > r)
			{
				r = vector.Y;
			}

			if(vector.Z > r)
			{
				r = vector.Z;
			}

			return r;
		}

		/// <summary>
		/// Calculates the average position of a collection of points.
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		public static Vector3 CalculateAverage(Vector3[] points)
		{
			Vector3 center = new();

			if(points == null || points.Length == 0)
			{
				return center;
			}

			foreach(Vector3 p in points)
			{
				center += p;
			}

			return center / points.Length;
		}

		/// <summary>
		/// Calculates the bounding box center of a collection of points.
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		public static Vector3 CalculateCenter(IEnumerable<Vector3> points)
		{
			Vector3? first = null;
			foreach(Vector3 point in points)
			{
				first = point;
				break;
			}

			if(first == null)
			{
				return default;
			}

			Vector3 Positive = first.Value;
			Vector3 Negative = first.Value;

			static void boundsCheck(float i, ref float p, ref float n)
			{
				if(i > p)
				{
					p = i;
				}
				else if(i < n)
				{
					n = i;
				}
			}

			foreach(Vector3 p in points)
			{
				boundsCheck(p.X, ref Positive.X, ref Negative.X);
				boundsCheck(p.Y, ref Positive.Y, ref Negative.Y);
				boundsCheck(p.Z, ref Positive.Z, ref Negative.Z);
			}

			return (Positive + Negative) / 2;
		}

		/// <summary>
		/// Calculates the XZ euler angles necessary to rotate <see cref="Vector3.UnitY"/> towards the given normal.
		/// </summary>
		/// <param name="normal">The normal to get the rotation of.</param>
		/// <returns>The euler angle.</returns>
		public static Vector3 NormalToXZAngles(this Vector3 normal)
		{
			bool close0 = MathF.Abs(normal.X) < 0.002f && MathF.Abs(normal.Y) < 0.002f;

			if(normal.Z > 0.9999f || (close0 && normal.Z > 0))
			{
				return new(MathHelper.HalfPi, 0, 0);
			}
			else if(normal.Z < -0.9999f || (close0 && normal.Z < 0))
			{
				return new(-MathHelper.HalfPi, 0, 0);
			}
			else
			{
				return new(
					MathF.Asin(normal.Z),
					0,
					-MathF.Atan2(normal.X, normal.Y)
				);
			}
		}

		/// <summary>
		/// Rotates a <see cref="Vector3.UnitY"/> by the given XZ euler angles.
		/// </summary>
		/// <param name="rotation">The angles to rotate by.</param>
		/// <returns>The calculated normal.</returns>
		public static Vector3 XZAnglesToNormal(this Vector3 rotation)
		{
			float cos = MathF.Cos(rotation.X);
			return new Vector3(
				MathF.Sin(-rotation.Z) * cos,
				MathF.Cos(-rotation.Z) * cos,
				MathF.Sin(rotation.X)
				);
		}
	}
}
