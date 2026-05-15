using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Common.IO;
using System.Collections.Generic;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Meta data block containing a list of strings
	/// </summary>
	public abstract class StringListMetaDataBlock : MetaDataBlock
	{
		/// <summary>
		/// Metadata string  values
		/// </summary>
		public List<string> Values { get; set; } = [];

		/// <inheritdoc/>
		protected override void ReadContents(BinaryObjectReader reader)
		{
			long offset = reader.ReadOffsetValue();
			while(offset != uint.MaxValue)
			{
				Values.Add(reader.ReadStringAtOffsetOrEmpty(offset));
				offset = reader.ReadOffsetValue();
			}
		}

		/// <inheritdoc/>
		protected override void WriteContents(BinaryObjectWriter writer)
		{
			SeekToken start = writer.At();
			writer.Skip(Values.Count * sizeof(uint));
			writer.WriteUInt32(uint.MaxValue);

			uint[] offsets = new uint[Values.Count];
			for(int i = 0; i < offsets.Length; i++)
			{
				offsets[i] = (uint)writer.GetPositionOffset();
				writer.WriteString(StringBinaryFormat.NullTerminated, Values[i]);
				writer.Align(4);
			}

			using(writer.At())
			{
				start.Dispose();
				writer.WriteArray(offsets);
			}
		}
	}
}
