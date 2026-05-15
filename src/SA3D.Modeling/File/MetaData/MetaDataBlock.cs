using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using Amicitia.IO.Streams;
using SA3D.Common.IO;

namespace SA3D.Modeling.File.MetaData
{
	/// <summary>
	/// Base meta data block class
	/// </summary>
	public abstract class MetaDataBlock : IBinarySerializable<MetaDataIOContext>
	{
		/// <summary>
		/// Type of the metadata block
		/// </summary>
		public abstract MetaDataBlockType Type { get; }

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, MetaDataIOContext context)
		{
			if(context.Version < 2)
			{
				ReadContents(reader);
				return;
			}

			int blockSize = reader.ReadInt32();
			long nextBlockStart = reader.Position + blockSize;

			if(context.Version == 3)
			{
				using(reader.WithOffsetOrigin())
				{
					ReadContents(reader);
				}
			}
			else
			{
				ReadContents(reader);
			}

			reader.SeekPosition(nextBlockStart); // just to be sure...
		}

		/// <summary>
		/// Responsible for reading the blocks content
		/// </summary>
		/// <param name="reader">Reader to read from</param>
		protected abstract void ReadContents(BinaryObjectReader reader);

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, MetaDataIOContext context)
		{
			SeekToken writeStart = writer.At();
			writer.WriteUInt32(0); // placeholder

			long start = writer.Position;

			using(writer.WithOffsetOrigin())
			{
				WriteContents(writer);
			}

			long end = writer.Position;

			using(writer.At())
			{
				writeStart.Dispose();
				writer.WriteUInt32((uint)(end - start));
			}
		}

		/// <summary>
		/// Responsible for writing the blocks content
		/// </summary>
		/// <param name="writer">Writer to write to</param>
		protected abstract void WriteContents(BinaryObjectWriter writer);
	}
}
