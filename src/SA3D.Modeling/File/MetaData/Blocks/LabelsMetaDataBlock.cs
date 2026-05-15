using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using System.Collections.Generic;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing structure labels
	/// </summary>
	public class LabelsMetaDataBlock : MetaDataBlock
	{
		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.Label;

		/// <summary>
		/// Labels stored in the metadata block
		/// </summary>
		public LabelDictionary Labels { get; set; } = new();

		/// <inheritdoc/>
		protected override void ReadContents(BinaryObjectReader reader)
		{
			long labelOffset = reader.ReadOffsetValue();
			while(labelOffset != uint.MaxValue)
			{
				string labelText = reader.ReadStringOffsetOrEmpty();
				Labels.AddSafe(labelOffset, labelText);

				labelOffset = reader.ReadOffsetValue();
			}
		}

		/// <inheritdoc/>
		protected override void WriteContents(BinaryObjectWriter writer)
		{
			SeekToken start = writer.At();
			uint[] offsets = new uint[Labels.Count * 2];
			writer.WriteArray(offsets);
			writer.WriteUInt64(ulong.MaxValue);

			int index = 0;
			foreach(KeyValuePair<long, string> label in Labels.GetDictFrom())
			{
				offsets[index] = (uint)label.Key;
				offsets[index + 1] = (uint)writer.GetPositionOffset();
				index += 2;

				writer.WriteString(StringBinaryFormat.NullTerminated, label.Value);
				writer.Align(4);
			}

			using(writer.At())
			{
				start.Dispose();
				writer.WriteArray(offsets);
			}
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Labels: [{Labels.Count}]";
		}
	}
}
