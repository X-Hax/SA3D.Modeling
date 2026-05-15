using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Common.IO;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace SA3D.Modeling.File
{
	internal static class NJBlockUtility
	{
		public static Dictionary<long, string> GetBlockOffsets(BinaryObjectReader reader)
		{
			Dictionary<long, string> result = [];

			using EndiannessToken endiannessToken = reader.WithEndian(reader.CheckEndianness32(4, SeekOrigin.Current));
			using SeekToken seekToken = reader.At();

			while(reader.Position < reader.Length + 8)
			{
				long offset = reader.Position;
				string blockheader = reader.ReadString(StringBinaryFormat.FixedLength, 4);
				uint blockSize = reader.ReadUInt32();

				if(string.IsNullOrWhiteSpace(blockheader) || blockSize == 0)
				{
					break;
				}

				result.Add(offset, blockheader);
				reader.Seek(blockSize, System.IO.SeekOrigin.Current);
			}

			return result;
		}

		public static bool FindBlockOffset(Dictionary<long, string> blocks, HashSet<string> toFind, [MaybeNullWhen(false)] out long? blockOffset)
		{
			foreach(KeyValuePair<long, string> block in blocks)
			{
				if(toFind.Contains(block.Value))
				{
					blockOffset = block.Key;
					return true;
				}
			}

			blockOffset = null;
			return false;
		}

		public static bool FindBlockOffset(BinaryObjectReader reader, HashSet<string> toFind, [MaybeNullWhen(false)] out long? blockOffset)
		{
			Dictionary<long, string> blocks = GetBlockOffsets(reader);
			return FindBlockOffset(blocks, toFind, out blockOffset);
		}
	}
}
