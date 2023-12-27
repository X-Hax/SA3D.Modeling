using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Animation.Utilities
{
	internal static class KeyframeRead
	{
		public static void ReadVector3Set(this EndianStackReader reader, uint address, uint count, SortedDictionary<uint, Vector3> dictionary, FloatIOType type)
		{
			if(type == FloatIOType.BAMS16)
			{
				for(int i = 0; i < count; i++)
				{
					uint frame = reader.ReadUShort(address);
					address += 2;
					dictionary.Add(frame, reader.ReadVector3(ref address, type));
				}
			}
			else
			{
				for(int i = 0; i < count; i++)
				{
					uint frame = reader.ReadUInt(address);
					address += 4;
					dictionary.Add(frame, reader.ReadVector3(ref address, type));
				}
			}

		}

		public static void ReadVector3ArraySet(this EndianStackReader reader, uint address, uint count, string labelPrefix, SortedDictionary<uint, ILabeledArray<Vector3>> dictionary, PointerLUT lut)
		{
			if(count == 0)
			{
				return;
			}

			uint startAddr = address;

			// <frame, address>
			SortedDictionary<uint, uint> frameAddresses = new();
			for(int i = 0; i < count; i++)
			{
				uint frame = reader.ReadUInt(address);
				uint ptr = reader.ReadPointer(address += 4);
				address += 4;

				frameAddresses.Add(frame, ptr);
			}

			uint[] addresses = frameAddresses.Values.Distinct().Order().ToArray();
			// get the smallest array size
			uint size = (startAddr - addresses[^1]) / 12;
			for(int i = 1; i < addresses.Length; i++)
			{
				for(int j = 0; j < i; j++)
				{
					uint newSize = (addresses[i] - addresses[j]) / 12;
					if(newSize < size)
					{
						size = newSize;
					}
				}
			}

			foreach(KeyValuePair<uint, uint> item in frameAddresses)
			{
				ILabeledArray<Vector3> vectors = lut.GetAddLabeledValue(item.Value, labelPrefix, () =>
				{
					LabeledArray<Vector3> result = new(size);

					uint ptr = item.Value;
					for(int j = 0; j < size; j++)
					{
						result[j] = reader.ReadVector3(ref ptr);
					}

					return result;
				});

				dictionary.Add(item.Key, vectors);
			}
		}

		public static void ReadVector2Set(this EndianStackReader reader, uint address, uint count, SortedDictionary<uint, Vector2> dictionary, FloatIOType type)
		{
			for(int i = 0; i < count; i++)
			{
				uint frame = reader.ReadUInt(address);
				address += 4;
				dictionary.Add(frame, reader.ReadVector2(ref address, type));
			}
		}

		public static void ReadColorSet(this EndianStackReader reader, uint address, uint count, SortedDictionary<uint, Color> dictionary, ColorIOType type)
		{
			for(int i = 0; i < count; i++)
			{
				uint frame = reader.ReadUInt(address);
				address += 4;
				dictionary.Add(frame, reader.ReadColor(ref address, type));
			}
		}

		public static void ReadFloatSet(this EndianStackReader reader, uint address, uint count, SortedDictionary<uint, float> dictionary, bool BAMS)
		{
			for(int i = 0; i < count; i++)
			{
				uint frame = reader.ReadUInt(address);
				float value = BAMS
					? BAMSFHelper.BAMSFToRad(reader.ReadInt(address + 4))
					: reader.ReadFloat(address + 4);
				address += 8;
				dictionary.Add(frame, value);
			}
		}

		public static void ReadSpotSet(this EndianStackReader reader, uint address, uint count, SortedDictionary<uint, Spotlight> dictionary)
		{
			for(int i = 0; i < count; i++)
			{
				uint frame = reader.ReadUInt(address);
				Spotlight value = Spotlight.Read(reader, address + 4);
				address += 8 + Spotlight.StructSize;
				dictionary.Add(frame, value);
			}
		}

		public static void ReadQuaternionSet(this EndianStackReader reader, uint address, uint count, SortedDictionary<uint, Quaternion> dictionary)
		{
			for(int i = 0; i < count; i++)
			{
				uint frame = reader.ReadUInt(address);
				address += 4;
				dictionary.Add(frame, reader.ReadQuaternion(ref address));
			}
		}
	}
}
