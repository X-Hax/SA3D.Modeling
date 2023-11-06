using SA3D.Common.IO;
using System;
using static SA3D.Common.MathHelper;

namespace SA3D.Modeling.Animation
{
	/// <summary>
	/// Spotlight for cutscenes.
	/// </summary>
	public struct Spotlight
	{
		/// <summary>
		/// Size of the spotlight struct.
		/// </summary>
		public static uint StructSize => 16;

		/// <summary>
		/// Closest light distance.
		/// </summary>
		public float near;

		/// <summary>
		/// Furthest light distance.
		/// </summary>
		public float far;

		/// <summary>
		/// Inner cone angle.
		/// </summary>
		public float insideAngle;

		/// <summary>
		/// Outer cone angle.
		/// </summary>
		public float outsideAngle;

		/// <summary>
		/// Linearly interpolate between two spotlights.
		/// </summary>
		/// <param name="from">Spotlight from which to start interpolating.</param>
		/// <param name="to">Spotlight to which to interpolate.</param>
		/// <param name="time">Value by which to interpolate</param>
		/// <returns>The interpolated spotlight.</returns>
		public static Spotlight Lerp(Spotlight from, Spotlight to, float time)
		{
			float inverse = 1 - time;
			return new Spotlight()
			{
				near = (to.near * time) + (from.near * inverse),
				far = (to.far * time) + (from.far * inverse),
				insideAngle = (to.insideAngle * time) + (from.insideAngle * inverse),
				outsideAngle = (to.outsideAngle * time) + (from.outsideAngle * inverse),
			};
		}

		/// <summary>
		/// Calculates the distance between two spotlight values (handled like a Vector4).
		/// </summary>
		/// <param name="from">First spotlight.</param>
		/// <param name="to">Second spotlight.</param>
		/// <returns>The distance</returns>
		public static float Distance(Spotlight from, Spotlight to)
		{
			return MathF.Sqrt(
				MathF.Pow(from.near - to.near, 2) +
				MathF.Pow(from.far - to.far, 2) +
				MathF.Pow(from.insideAngle - to.insideAngle, 2) +
				MathF.Pow(from.outsideAngle - to.outsideAngle, 2)
				);
		}

		/// <summary>
		/// Reads a spotlight off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <returns>The spotlight that was read.</returns>
		public static Spotlight Read(EndianStackReader reader, uint address)
		{
			return new Spotlight()
			{
				near = reader.ReadFloat(address),
				far = reader.ReadFloat(address + 4),
				insideAngle = BAMSToRad(reader.ReadInt(address + 8)),
				outsideAngle = BAMSToRad(reader.ReadInt(address + 12))
			};
		}

		/// <summary>
		/// Writes the spotlight to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public readonly void Write(EndianStackWriter writer)
		{
			writer.WriteFloat(near);
			writer.WriteFloat(far);
			writer.WriteInt(RadToBAMS(insideAngle));
			writer.WriteInt(RadToBAMS(outsideAngle));
		}
	}
}
