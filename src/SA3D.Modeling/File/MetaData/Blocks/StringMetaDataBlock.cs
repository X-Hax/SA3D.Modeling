using Amicitia.IO.Binary;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Meta data block containing a string
	/// </summary>
	public abstract class StringMetaDataBlock : MetaDataBlock
	{
		/// <summary>
		/// Metadata string value
		/// </summary>
		public string Value { get; set; } = string.Empty;

		/// <inheritdoc/>
		protected override void ReadContents(BinaryObjectReader reader)
		{
			Value = reader.ReadString(StringBinaryFormat.NullTerminated);
		}

		/// <inheritdoc/>
		protected override void WriteContents(BinaryObjectWriter writer)
		{
			writer.WriteString(StringBinaryFormat.NullTerminated, Value);
			writer.Align(4);
		}
	}
}
