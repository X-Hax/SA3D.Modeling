using Amicitia.IO.Binary;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.AnimationData.Utilities
{
	internal static class KeyframeRead
	{
		public static void ReadFloatSet(this BinaryObjectReader reader, int count, SortedDictionary<uint, float> dictionary, FloatIOType type)
		{
			Func<BinaryValueReader, float> read = type.GetReader();

			if(type.GetByteSize() == 2)
			{
				for(int i = 0; i < count; i++)
				{
					dictionary.Add(
						reader.ReadUInt16(),
						read(reader)
					);
				}
			}
			else
			{
				for(int i = 0; i < count; i++)
				{
					dictionary.Add(
						reader.ReadUInt32(),
						read(reader)
					);
				}
			}
		}

		public static void ReadVector2Set(this BinaryObjectReader reader, int count, SortedDictionary<uint, Vector2> dictionary, FloatIOType type)
		{
			Func<BinaryValueReader, Vector2> read = type.GetVector2Reader();

			if(type.GetByteSize() == 2)
			{
				for(int i = 0; i < count; i++)
				{
					dictionary.Add(
						reader.ReadUInt16(),
						read(reader)
					);
				}
			}
			else
			{
				for(int i = 0; i < count; i++)
				{
					dictionary.Add(
						reader.ReadUInt32(),
						read(reader)
					);
				}
			}
		}

		public static void ReadVector3Set(this BinaryObjectReader reader, int count, SortedDictionary<uint, Vector3> dictionary, FloatIOType type)
		{
			Func<BinaryValueReader, Vector3> read = type.GetVector3Reader();

			if(type.GetByteSize() == 2)
			{
				for(int i = 0; i < count; i++)
				{
					dictionary.Add(
						reader.ReadUInt16(),
						read(reader)
					);
				}
			}
			else
			{
				for(int i = 0; i < count; i++)
				{
					dictionary.Add(
						reader.ReadUInt32(),
						read(reader)
					);
				}
			}

		}

		public static void ReadVector3ArraySet(this BinaryObjectReader reader, int count, string labelPrefix, SortedDictionary<uint, LabeledArray<Vector3>> dictionary, PointerLUT lut)
		{
			if(count == 0)
			{
				return;
			}

			long startOffset = reader.GetPositionOffset();

			// <frame, offset>
			SortedDictionary<uint, long> frameOffsets = [];
			for(int i = 0; i < count; i++)
			{
				frameOffsets.Add(reader.ReadUInt32(), reader.ReadOffsetValue());
			}

			long[] offsets = [startOffset, .. frameOffsets.Values.Distinct()];
			Array.Sort(offsets);

			// get the smallest array size; Start with the largest possible size
			long size = long.MaxValue;
			for(int i = 1; i < offsets.Length; i++)
			{
				long newSize = (offsets[i] - offsets[i - 1]) / 12;
				size = long.Min(size, newSize);
			}

			foreach(KeyValuePair<uint, long> item in frameOffsets)
			{
				LabeledArray<Vector3> vectors = reader.ReadLabeledObjectArrayAtOffset(StructBinaryHelper.ReadVector3, item.Value, (int)size, labelPrefix, lut)
					?? throw reader.ReadNullReference(nameof(KeyframeSet), labelPrefix);

				dictionary.Add(item.Key, vectors);
			}
		}

		public static void ReadColorSet(this BinaryObjectReader reader, int count, SortedDictionary<uint, Color> dictionary, ColorIOType type)
		{
			for(int i = 0; i < count; i++)
			{
				dictionary.Add(
					reader.ReadUInt32(),
					reader.ReadObject<Color, ColorIOType>(type)
				);
			}
		}

		public static void ReadSpotSet(this BinaryObjectReader reader, int count, SortedDictionary<uint, Spotlight> dictionary)
		{
			for(int i = 0; i < count; i++)
			{
				dictionary.Add(
					reader.ReadUInt32(),
					reader.ReadObject<Spotlight>()
				);
			}
		}

		public static void ReadQuaternionSet(this BinaryObjectReader reader, int count, SortedDictionary<uint, Quaternion> dictionary)
		{
			for(int i = 0; i < count; i++)
			{
				dictionary.Add(
					reader.ReadUInt32(),
					reader.ReadQuaternion()
				);
			}
		}
	}
}
