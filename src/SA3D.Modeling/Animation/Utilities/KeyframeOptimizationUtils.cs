using SA3D.Common;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Animation.Utilities
{
	internal static class KeyframeOptimizationUtils
	{
		private static void RemoveDeviations<T>(
			this SortedDictionary<uint, T> keyframes,
			uint? start,
			uint? end,
			float deviationThreshold,
			Func<T, T, float, T> lerp,
			Func<T, T, float> calculateDeviation)
		{
			start ??= keyframes.Keys.FirstOrDefault();
			end ??= keyframes.Keys.LastOrDefault();

			if(end - start < 2 || deviationThreshold <= 0.0)
			{
				return;
			}

			List<uint> frames = new();
			for(uint frame = start.Value; frame <= end; frame++)
			{
				if(keyframes.ContainsKey(frame))
				{
					frames.Add(frame);
				}
			}

			// whenever a frame is removed, we skip the next one.
			// repeat that until we reach an iteration where no frame was removed

			bool done;
			do
			{
				done = true;
				for(int i = 1; i < frames.Count - 1; i++)
				{
					uint previous = frames[i - 1];
					uint current = frames[i];
					uint next = frames[i + 1];

					float linearFac = (current - previous) / (float)(next - previous);

					T linear = lerp(keyframes[previous], keyframes[next], linearFac);
					T actual = keyframes[current];

					float deviation = calculateDeviation(linear, actual);
					if(deviation < deviationThreshold)
					{
						keyframes.Remove(current);
						frames.RemoveAt(i);
						done = false;
					}
				}
			}
			while(!done);
		}

		public static void OptimizeFloat(this SortedDictionary<uint, float> keyframes, float deviationThreshold, uint? start, uint? end)
		{
			keyframes.RemoveDeviations(
				start, end, deviationThreshold,
				(a, b, t) => (a * (1 - t)) + (b * t),
				(a, b) => Math.Abs(a - b));
		}

		public static void OptimizeFloatDegrees(this SortedDictionary<uint, float> keyframes, float deviationThreshold, uint? start, uint? end)
		{
			keyframes.RemoveDeviations(
				start, end, deviationThreshold,
				(a, b, t) => (a * (1 - t)) + (b * t),
				(a, b) => MathHelper.RadToDeg(Math.Abs(a - b)));
		}

		public static void OptimizeVector2(this SortedDictionary<uint, Vector2> keyframes, float deviationThreshold, uint? start, uint? end)
		{
			keyframes.RemoveDeviations(
				start, end, deviationThreshold,
				Vector2.Lerp,
				(a, b) => (a - b).Length());
		}

		public static void OptimizeVector3(this SortedDictionary<uint, Vector3> keyframes, float deviationThreshold, uint? start, uint? end)
		{
			keyframes.RemoveDeviations(
				start, end, deviationThreshold,
				Vector3.Lerp,
				(a, b) => (a - b).Length());
		}

		public static void OptimizeVector3Degrees(this SortedDictionary<uint, Vector3> keyframes, float deviationThreshold, uint? start, uint? end)
		{
			keyframes.RemoveDeviations(
				start, end, deviationThreshold,
				Vector3.Lerp,
				(a, b) => MathHelper.RadToDeg((a - b).Length()));
		}

		public static void OptimizeColor(this SortedDictionary<uint, Color> keyframes, float deviationThreshold, uint? start, uint? end)
		{
			keyframes.RemoveDeviations(
				start, end, deviationThreshold,
				Color.Lerp,
				Color.Distance);
		}

		public static void OptimizeQuaternion(this SortedDictionary<uint, Quaternion> keyframes, float deviationThreshold, uint? start, uint? end)
		{
			keyframes.RemoveDeviations(
				start, end, deviationThreshold,
				QuaternionUtilities.RealLerp,
				(a, b) => (a - b).Length());
		}

		public static void OptimizeSpotlight(this SortedDictionary<uint, Spotlight> keyframes, float deviationThreshold, uint? start, uint? end)
		{
			keyframes.RemoveDeviations(
				start, end, deviationThreshold,
				Spotlight.Lerp,
				Spotlight.Distance);
		}
	}
}
