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
		public static KeyframeArray<T>? ReadKeyframeArrayAtOffset<T>(this BinaryObjectReader reader, long offset, int count, string labelPrefix, OffsetLUT lut, Func<BinaryObjectReader, KeyframeArray<T>> read)
		{
			if(count == 0)
			{
				return null;
			}

			return reader.ReadLUTItemAtOffset(offset, lut, labelPrefix, r => read(r));
		}

		public static KeyframeArray<float> ReadFloatSet(this BinaryObjectReader reader, int count, FloatIOType type)
		{
			KeyframeArray<float> result = [];
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

			return result;
		}

		public static KeyframeArray<Vector2> ReadVector2Set(this BinaryObjectReader reader, int count, FloatIOType type)
		{
			KeyframeArray<Vector2> result = [];
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

			return result;
		}

		public static KeyframeArray<Vector3> ReadVector3Set(this BinaryObjectReader reader, int count, FloatIOType type)
		{
			KeyframeArray<Vector3> result = [];
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

			return result;
		}

		public static KeyframeArray<LabeledArray<Vector3>> ReadVector3ArraySet(this BinaryObjectReader reader, int count, string labelPrefix, ModelOffsetLUT lut)
		{
			KeyframeArray<LabeledArray<Vector3>> result = [];

			if(count == 0)
			{
				return result;
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
			long size = reader.Length - offsets[0];
			for(int i = 1; i < offsets.Length; i++)
			{
				long newSize = (offsets[i] - offsets[i - 1]) / 12;
				size = long.Min(size, newSize);
			}

			foreach(KeyValuePair<uint, long> item in frameOffsets)
			{
				LabeledArray<Vector3> vectors = reader.ReadLabeledObjectArrayAtOffset(StructBinaryHelper.ReadVector3, item.Value, (int)size, labelPrefix, lut)
					?? throw reader.ReadNullReference(nameof(KeyframeSet), labelPrefix);

				result.Add(item.Key, vectors);
			}

			return result;
		}

		public static KeyframeArray<Color> ReadColorSet(this BinaryObjectReader reader, int count, ColorIOType type)
		{
			KeyframeArray<Color> result = [];

			for(int i = 0; i < count; i++)
			{
				result.Add(
					reader.ReadUInt32(),
					reader.ReadObject<Color, ColorIOType>(type)
				);
			}

			return result;
		}

		public static KeyframeArray<Spotlight> ReadSpotlightSet(this BinaryObjectReader reader, int count)
		{
			KeyframeArray<Spotlight> result = [];

			for(int i = 0; i < count; i++)
			{
				result.Add(
					reader.ReadUInt32(),
					reader.ReadObject<Spotlight>()
				);
			}

			return result;
		}

		public static KeyframeArray<Quaternion> ReadQuaternionSet(this BinaryObjectReader reader, int count)
		{
			KeyframeArray<Quaternion> result = [];

			for(int i = 0; i < count; i++)
			{
				result.Add(
					reader.ReadUInt32(),
					reader.ReadQuaternionRe()
				);
			}

			return result;
		}
	}
}
