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
		public static KeyframeArray<T>? ReadKeyframeArrayAtOffset<T>(this BinaryObjectReader reader, long offset, int count, string labelPrefix, OffsetLUT lut, Action<BinaryObjectReader, KeyframeArray<T>> read)
		{
			if(count == 0)
			{
				return null;
			}

			return reader.ReadLUTItemAtOffset<KeyframeArray<T>>(offset, lut, labelPrefix, (r, dst) => read(r, dst));
		}

		public static void ReadFloatSet(this BinaryObjectReader reader, KeyframeArray<float> result, int count, FloatIOType type)
		{
			Func<BinaryValueReader, float> read = type.GetReader();

			if(type.GetByteSize() == 2)
			{
				for(int i = 0; i < count; i++)
				{
					result.Add(
						reader.ReadUInt16(),
						read(reader)
					);
				}
			}
			else
			{
				for(int i = 0; i < count; i++)
				{
					result.Add(
						reader.ReadUInt32(),
						read(reader)
					);
				}
			}
		}

		public static void ReadVector2Set(this BinaryObjectReader reader, KeyframeArray<Vector2> result, int count, FloatIOType type)
		{
			Func<BinaryValueReader, Vector2> read = type.GetVector2Reader();

			if(type.GetByteSize() == 2)
			{
				for(int i = 0; i < count; i++)
				{
					result.Add(
						reader.ReadUInt16(),
						read(reader)
					);
				}
			}
			else
			{
				for(int i = 0; i < count; i++)
				{
					result.Add(
						reader.ReadUInt32(),
						read(reader)
					);
				}
			}
		}

		public static void ReadVector3Set(this BinaryObjectReader reader, KeyframeArray<Vector3> result, int count, FloatIOType type)
		{
			Func<BinaryValueReader, Vector3> read = type.GetVector3Reader();

			if(type.GetByteSize() == 2)
			{
				for(int i = 0; i < count; i++)
				{
					result.Add(
						reader.ReadUInt16(),
						read(reader)
					);
				}
			}
			else
			{
				for(int i = 0; i < count; i++)
				{
					result.Add(
						reader.ReadUInt32(),
						read(reader)
					);
				}
			}
		}

		public static void ReadVector3ArraySet(this BinaryObjectReader reader, KeyframeArray<LabeledArray<Vector3>> result, int count, string labelPrefix, ModelOffsetLUT lut)
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

			long? size = null;
			foreach(long offset in frameOffsets.Values)
			{
				if(lut.TryGetValue(offset, out LabeledArray<Vector3>? arrayCheck))
				{
					size = arrayCheck.Length;
					break;
				}
			}

			if(size == null)
			{
				long[] offsets = [startOffset, .. frameOffsets.Values.Distinct()];
				Array.Sort(offsets);

				// get the smallest array size; Start with the largest possible size
				size = reader.Length - offsets[0];
				for(int i = 1; i < offsets.Length; i++)
				{
					long newSize = (offsets[i] - offsets[i - 1]) / 12;
					size = long.Min(size.Value, newSize);
				}

			}

			foreach(KeyValuePair<uint, long> item in frameOffsets)
			{
				LabeledArray<Vector3> vectors = reader.ReadLabeledObjectArrayAtOffset(StructBinaryHelper.ReadVector3, item.Value, (int)size.Value, labelPrefix, lut)
					?? throw reader.ReadNullReference(nameof(KeyframeSet), labelPrefix);

				result.Add(item.Key, vectors);
			}
		}

		public static void ReadColorSet(this BinaryObjectReader reader, KeyframeArray<Color> result, int count, ColorIOType type)
		{
			for(int i = 0; i < count; i++)
			{
				result.Add(
					reader.ReadUInt32(),
					reader.ReadObject<Color, ColorIOType>(type)
				);
			}
		}

		public static void ReadSpotlightSet(this BinaryObjectReader reader, KeyframeArray<Spotlight> result, int count)
		{
			for(int i = 0; i < count; i++)
			{
				result.Add(
					reader.ReadUInt32(),
					reader.ReadObject<Spotlight>()
				);
			}
		}

		public static void ReadQuaternionSet(this BinaryObjectReader reader, KeyframeArray<Quaternion> result, int count)
		{
			for(int i = 0; i < count; i++)
			{
				result.Add(
					reader.ReadUInt32(),
					reader.ReadQuaternionRe()
				);
			}
		}
	}
}
