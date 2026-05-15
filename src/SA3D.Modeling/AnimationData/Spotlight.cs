using Amicitia.IO.Binary;
using SA3D.Modeling.Structs;
using System.Numerics;

namespace SA3D.Modeling.AnimationData
{
	/// <summary>
	/// Spotlight for cutscenes.
	/// </summary>
	public struct Spotlight : IBinarySerializable
	{
		/// <summary>
		/// Closest light distance.
		/// </summary>
		public float Near { get; set; }

		/// <summary>
		/// Furthest light distance.
		/// </summary>
		public float Far { get; set; }

		/// <summary>
		/// Inner cone angle.
		/// </summary>
		public float InsideAngle { get; set; }

		/// <summary>
		/// Outer cone angle.
		/// </summary>
		public float OutsideAngle { get; set; }

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
				Near = (to.Near * time) + (from.Near * inverse),
				Far = (to.Far * time) + (from.Far * inverse),
				InsideAngle = (to.InsideAngle * time) + (from.InsideAngle * inverse),
				OutsideAngle = (to.OutsideAngle * time) + (from.OutsideAngle * inverse),
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
			return Vector4.Distance(
				new(from.Near, from.Far, from.InsideAngle, from.OutsideAngle),
				new(to.Near, to.Far, to.InsideAngle, to.OutsideAngle)
			);
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			Near = reader.ReadSingle();
			Far = reader.ReadSingle();
			InsideAngle = reader.ReadSingle(FloatIOType.BAMS32);
			OutsideAngle = reader.ReadSingle(FloatIOType.BAMS32);
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer)
		{
			writer.WriteSingle(Near);
			writer.WriteSingle(Far);
			writer.WriteSingle(InsideAngle, FloatIOType.BAMS32);
			writer.WriteSingle(OutsideAngle, FloatIOType.BAMS32);
		}
	}
}
