using SA3D.Common.IO;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace SA3D.Modeling.File
{
	internal static class NJBlockUtility
	{
		public static Dictionary<uint, uint> GetBlockAddresses(EndianStackReader reader, uint address)
		{
			Dictionary<uint, uint> result = new();
			reader.PushBigEndian(reader.CheckBigEndian32(address + 4));

			uint blockAddress = address;
			while(blockAddress < reader.Length + 8)
			{
				reader.PushBigEndian(false);
				uint blockHeader = reader.ReadUInt(blockAddress);
				reader.PopEndian();

				uint blockSize = reader.ReadUInt(blockAddress + 4);
				if(blockHeader == 0 || blockSize == 0)
				{
					break;
				}

				result.Add(blockAddress, blockHeader);
				blockAddress += 8 + blockSize;
			}

			reader.PopEndian();
			return result;
		}

		public static bool FindBlockAddress(Dictionary<uint, uint> blocks, HashSet<uint> toFind, [MaybeNullWhen(false)] out uint? blockAddress)
		{
			foreach(KeyValuePair<uint, uint> block in blocks)
			{
				if(toFind.Contains(block.Value))
				{
					blockAddress = block.Key;
					return true;
				}
			}

			blockAddress = null;
			return false;
		}

		public static bool FindBlockAddress(EndianStackReader reader, uint address, HashSet<uint> toFind, [MaybeNullWhen(false)] out uint? blockAddress)
		{
			Dictionary<uint, uint> blocks = GetBlockAddresses(reader, address);
			return FindBlockAddress(blocks, toFind, out blockAddress);
		}
	}
}
