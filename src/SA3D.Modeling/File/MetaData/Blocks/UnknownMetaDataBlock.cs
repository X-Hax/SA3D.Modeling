using Amicitia.IO.Binary;
using System;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing unknown data
	/// </summary>
	public class UnknownMetaDataBlock : MetaDataBlock
	{
		/// <inheritdoc/>
		public override MetaDataBlockType Type => (MetaDataBlockType)RawType;

		/// <summary>
		/// Raw, unknown metadata block type
		/// </summary>
		public uint RawType { get; set; }

		/// <summary>
		/// Data stored in the block
		/// </summary>
		public byte[] Data { get; set; } = [];

		/// <inheritdoc/>
		protected override void ReadContents(BinaryObjectReader reader)
		{
			reader.Seek(sizeof(uint) * -2, System.IO.SeekOrigin.Current);
			RawType = reader.ReadUInt32();
			int size = reader.ReadInt32();
			Data = reader.ReadArray<byte>(size);
		}

		/// <inheritdoc/>
		protected override void WriteContents(BinaryObjectWriter writer)
		{
			throw new NotImplementedException();
		}
	}
}
