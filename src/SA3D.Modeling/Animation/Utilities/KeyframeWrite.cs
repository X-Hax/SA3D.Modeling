using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Numerics;

namespace SA3D.Modeling.Animation.Utilities
{
	internal static class KeyframeWrite
	{
		public static void WriteVector3Set(this EndianStackWriter writer, SortedDictionary<uint, Vector3> dict, FloatIOType ioType)
		{
			foreach(KeyValuePair<uint, Vector3> pair in dict)
			{
				writer.WriteUInt(pair.Key);
				writer.WriteVector3(pair.Value, ioType);
			}
		}

		public static void WriteVector2Set(this EndianStackWriter writer, SortedDictionary<uint, Vector2> dict, FloatIOType ioType)
		{
			foreach(KeyValuePair<uint, Vector2> pair in dict)
			{
				writer.WriteUInt(pair.Key);
				writer.WriteVector2(pair.Value, ioType);
			}
		}

		public static void WriteColorSet(this EndianStackWriter writer, SortedDictionary<uint, Color> dict, ColorIOType ioType)
		{
			foreach(KeyValuePair<uint, Color> pair in dict)
			{
				writer.WriteUInt(pair.Key);
				writer.WriteColor(pair.Value, ioType);
			}
		}

		public static uint[] WriteVector3ArrayData(this EndianStackWriter writer, SortedDictionary<uint, ILabeledArray<Vector3>> dict, PointerLUT lut)
		{
			uint[] result = new uint[dict.Count * 2];
			int i = 0;

			foreach(KeyValuePair<uint, ILabeledArray<Vector3>> pair in dict)
			{
				result[i++] = pair.Key;
				result[i++] = lut.GetAddAddress(pair.Value, (array) =>
				{
					uint result = writer.PointerPosition;

					foreach(Vector3 v in pair.Value)
					{
						writer.WriteVector3(v);
					}

					return result;
				});
			}

			return result;
		}

		public static void WriteVector3ArraySet(this EndianStackWriter writer, uint[] arrayData)
		{
			foreach(uint value in arrayData)
			{
				writer.WriteUInt(value);
			}
		}

		public static void WriteFloatSet(this EndianStackWriter writer, SortedDictionary<uint, float> dict, bool BAMS)
		{
			foreach(KeyValuePair<uint, float> pair in dict)
			{
				writer.WriteUInt(pair.Key);
				if(BAMS)
				{
					writer.WriteInt(MathHelper.RadToBAMS(pair.Value));
				}
				else
				{
					writer.WriteFloat(pair.Value);
				}
			}
		}

		public static void WriteSpotlightSet(this EndianStackWriter writer, SortedDictionary<uint, Spotlight> dict)
		{
			foreach(KeyValuePair<uint, Spotlight> pair in dict)
			{
				writer.WriteUInt(pair.Key);
				pair.Value.Write(writer);
			}
		}

		public static void WriteQuaternionSet(this EndianStackWriter writer, SortedDictionary<uint, Quaternion> dict)
		{
			foreach(KeyValuePair<uint, Quaternion> pair in dict)
			{
				writer.WriteUInt(pair.Key);
				writer.WriteQuaternion(pair.Value);
			}
		}

	}
}
