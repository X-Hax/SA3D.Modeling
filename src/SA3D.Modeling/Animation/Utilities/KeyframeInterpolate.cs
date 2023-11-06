using SA3D.Common.Lookup;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Animation.Utilities
{
	/// <summary>
	/// Keyframe interpolation methods
	/// </summary>
	internal static class KeyframeInterpolate
	{
		/// <summary>
		/// Searches through a keyframe dictionary and returns the interpolation between the values last and next. <br/>
		/// If the returned float is 0, then next will be default (as its not used)
		/// </summary>
		/// <typeparam name="T">Type of the Keyframe values</typeparam>
		/// <param name="keyframes">KEyframes to iterate through</param>
		/// <param name="timestamp">Current frame to get</param>
		/// <param name="interpolation"></param>
		/// <param name="before">Last Keyframe before given frame</param>
		/// <param name="next">Next Keyframe after given frame</param>
		/// <returns></returns>
		private static bool GetNearestFrames<T>(SortedDictionary<uint, T> keyframes, float timestamp, out float interpolation, out T before, [MaybeNullWhen(false)] out T next)
		{
			if(timestamp < 0)
			{
				timestamp = 0;
			}

			// if there is only one frame, we can take that one
			next = default;
			interpolation = 0;

			if(keyframes.Count == 1)
			{
				foreach(T val in keyframes.Values) // faster than converting to an array and accessing the first index
				{
					before = val;
					return false;
				}
			}

			// if the given frame is spot on and exists, then we can use it
			uint baseFrame = (uint)Math.Floor(timestamp);
			if(timestamp == baseFrame && keyframes.ContainsKey(baseFrame))
			{
				before = keyframes[baseFrame];
				return false;
			}

			// we gotta find the frames that the given frame is between
			// this is pretty easy thanks to the fact that the dictionary is always sorted

			// getting the first frame index
			SortedDictionary<uint, T>.KeyCollection keys = keyframes.Keys;
			uint nextSmallestFrame = keys.First();

			// if the smallest frame is greater than the frame we are at right now, then we can just return the frame
			if(nextSmallestFrame > baseFrame)
			{
				before = keyframes[nextSmallestFrame];
				return false;
			}

			// getting the actual next smallest and biggest frames
			uint nextBiggestFrame = baseFrame;
			foreach(uint key in keyframes.Keys)
			{
				if(key > nextSmallestFrame && key <= baseFrame)
				{
					nextSmallestFrame = key;
				}
				else if(key > baseFrame)
				{
					// the first bigger value must be the next biggest frame
					nextBiggestFrame = key;
					break;
				}
			}

			// if the next biggest frame hasnt changed, then that means we are past the last frame
			before = keyframes[nextSmallestFrame];
			if(nextBiggestFrame == baseFrame)
			{
				return false;
			}

			// the regular result
			next = keyframes[nextBiggestFrame];

			// getting the interpolation between the two frames
			float duration = nextBiggestFrame - nextSmallestFrame;
			interpolation = (timestamp - nextSmallestFrame) / duration;
			return true;
		}

		public static Vector3? ValueAtFrame(this SortedDictionary<uint, Vector3> keyframes, float frame)
		{
			if(keyframes.Count == 0)
			{
				return null;
			}

			if(!GetNearestFrames(keyframes, frame, out float interpolation, out Vector3 before, out Vector3 next))
			{
				return before;
			}
			else
			{
				return Vector3.Lerp(before, next, interpolation);
			}
		}

		public static Vector3[]? ValueAtFrame(this SortedDictionary<uint, ILabeledArray<Vector3>> keyframes, float frame)
		{
			if(keyframes.Count == 0)
			{
				return null;
			}

			if(!GetNearestFrames(keyframes, frame, out float interpolation, out ILabeledArray<Vector3> before, out ILabeledArray<Vector3>? next))
			{
				return before.ToArray();
			}

			Vector3[] result = new Vector3[before.Length];
			for(int i = 0; i < result.Length; i++)
			{
				result[i] = Vector3.Lerp(before[i], next[i], interpolation);
			}

			return result;
		}

		public static Vector2? ValueAtFrame(this SortedDictionary<uint, Vector2> keyframes, float frame)
		{
			if(keyframes.Count == 0)
			{
				return null;
			}

			if(!GetNearestFrames(keyframes, frame, out float interpolation, out Vector2 before, out Vector2 next))
			{
				return before;
			}
			else
			{
				return Vector2.Lerp(before, next, interpolation);
			}
		}

		public static Color? ValueAtFrame(this SortedDictionary<uint, Color> keyframes, float frame)
		{
			if(keyframes.Count == 0)
			{
				return null;
			}

			if(!GetNearestFrames(keyframes, frame, out float interpolation, out Color before, out Color next))
			{
				return before;
			}
			else
			{
				return Color.Lerp(before, next, interpolation);
			}
		}

		public static float? ValueAtFrame(this SortedDictionary<uint, float> keyframes, float frame)
		{
			if(keyframes.Count == 0)
			{
				return null;
			}

			if(!GetNearestFrames(keyframes, frame, out float interpolation, out float before, out float next))
			{
				return before;
			}
			else
			{
				return (next * interpolation) + (before * (1 - interpolation));
			}
		}

		public static Spotlight? ValueAtFrame(this SortedDictionary<uint, Spotlight> keyframes, float frame)
		{
			if(keyframes.Count == 0)
			{
				return null;
			}

			if(!GetNearestFrames(keyframes, frame, out float interpolation, out Spotlight before, out Spotlight next))
			{
				return before;
			}
			else
			{
				return Spotlight.Lerp(before, next, interpolation);
			}
		}

		public static Quaternion? ValueAtFrame(this SortedDictionary<uint, Quaternion> keyframes, float frame)
		{
			if(keyframes.Count == 0)
			{
				return null;
			}

			if(!GetNearestFrames(keyframes, frame, out float interpolation, out Quaternion before, out Quaternion next))
			{
				return before;
			}
			else
			{
				return Quaternion.Lerp(before, next, interpolation);
			}
		}

	}
}
