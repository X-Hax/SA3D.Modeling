using Amicitia.IO.Binary;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Numerics;

namespace SA3D.Modeling.AnimationData.Utilities
{
	internal static class KeyframeWrite
	{
		public static void WriteFloatSet(this BinaryObjectWriter writer, KeyframeArray<float> dict, FloatIOType ioType)
		{
			foreach(KeyValuePair<uint, float> pair in dict)
			{
				writer.WriteUInt32(pair.Key);
				writer.WriteSingle(pair.Value, ioType);
			}
		}

		public static void WriteVector2Set(this BinaryObjectWriter writer, KeyframeArray<Vector2> dict, FloatIOType ioType)
		{
			foreach(KeyValuePair<uint, Vector2> pair in dict)
			{
				writer.WriteUInt32(pair.Key);
				writer.WriteVector2(pair.Value, ioType);
			}
		}

		public static void WriteVector3Set(this BinaryObjectWriter writer, KeyframeArray<Vector3> dict, FloatIOType ioType)
		{
			if(ioType.GetByteSize() == 2)
			{
				foreach(KeyValuePair<uint, Vector3> pair in dict)
				{
					writer.WriteUInt16((ushort)pair.Key);
					writer.WriteVector3(pair.Value, ioType);
				}
			}
			else
			{
				foreach(KeyValuePair<uint, Vector3> pair in dict)
				{
					writer.WriteUInt32(pair.Key);
					writer.WriteVector3(pair.Value, ioType);
				}
			}
		}

		public static void WriteVector3ArrayData(this BinaryObjectWriter writer, KeyframeArray<LabeledArray<Vector3>> dict, ModelOffsetLUT lut)
		{
			foreach(KeyValuePair<uint, LabeledArray<Vector3>> pair in dict)
			{
				writer.WriteUInt32(pair.Key);
				writer.WriteObjectArrayOffset(StructBinaryHelper.WriteVector3, pair.Value, lut);
			}
		}

		public static void WriteColorSet(this BinaryObjectWriter writer, KeyframeArray<Color> dict, ColorIOType ioType)
		{
			foreach(KeyValuePair<uint, Color> pair in dict)
			{
				writer.WriteUInt32(pair.Key);
				writer.WriteObject(pair.Value, ioType);
			}
		}

		public static void WriteSpotlightSet(this BinaryObjectWriter writer, KeyframeArray<Spotlight> dict)
		{
			foreach(KeyValuePair<uint, Spotlight> pair in dict)
			{
				writer.WriteUInt32(pair.Key);
				pair.Value.Write(writer);
			}
		}

		public static void WriteQuaternionSet(this BinaryObjectWriter writer, KeyframeArray<Quaternion> dict)
		{
			foreach(KeyValuePair<uint, Quaternion> pair in dict)
			{
				writer.WriteUInt32(pair.Key);
				writer.WriteQuaternionRe(pair.Value);
			}
		}

	}
}
