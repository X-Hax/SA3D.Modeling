using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using Amicitia.IO.Streams;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.File.MetaData.Blocks;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SA3D.Modeling.File.MetaData
{
	/// <summary>
	/// Metadata block container
	/// </summary>
	public class MetaDataBlocks : IBinarySerializable<MetaDataIOContext>
	{
		/// <summary>
		/// Metadata blocks
		/// </summary>
		public List<MetaDataBlock> Blocks { get; set; } = [];

		private void AddBlock(MetaDataBlock? block)
		{
			if(block != null)
			{
				Blocks.Add(block);
			}
		}

		private void ReadVersion0(BinaryObjectReader reader, MetaDataIOContext context)
		{
			AddBlock(reader.ReadObjectOffset<AnimationFilesMetaDataBlock, MetaDataIOContext>(context));
			AddBlock(reader.ReadObjectOffset<MorphFilesMetaDataBlock, MetaDataIOContext>(context));
		}

		private void ReadVersion1(BinaryObjectReader reader, MetaDataIOContext context)
		{
			if(context.HasAnimMorphFiles)
			{
				ReadVersion0(reader, context);
			}

			AddBlock(reader.ReadObjectOffset<LabelsMetaDataBlock, MetaDataIOContext>(context));
		}

		private void ReadVersion2Up(BinaryObjectReader reader, MetaDataIOContext context)
		{
			MetaDataBlockType type;

			using(reader.At())
			{
				type = (MetaDataBlockType)reader.ReadUInt32();
			}

			while(type != MetaDataBlockType.End)
			{
				AddBlock(type switch
				{
					MetaDataBlockType.Label => reader.ReadObject<LabelsMetaDataBlock, MetaDataIOContext>(context),
					MetaDataBlockType.Animation => reader.ReadObject<AnimationFilesMetaDataBlock, MetaDataIOContext>(context),
					MetaDataBlockType.Morph => reader.ReadObject<MorphFilesMetaDataBlock, MetaDataIOContext>(context),
					MetaDataBlockType.Author => reader.ReadObject<AuthorMetaDataBlock, MetaDataIOContext>(context),
					MetaDataBlockType.Description => reader.ReadObject<DescriptionMetaDataBlock, MetaDataIOContext>(context),
					MetaDataBlockType.ActionName => reader.ReadObject<ActionNameMetaDataBlock, MetaDataIOContext>(context),
					MetaDataBlockType.ObjectName => reader.ReadObject<ObjectNameMetaDataBlock, MetaDataIOContext>(context),
					MetaDataBlockType.Weight => reader.ReadObject<WeightsMetaDataBlock, MetaDataIOContext>(context),
					MetaDataBlockType.End => throw new UnreachableException(),
					_ => reader.ReadObject<UnknownMetaDataBlock, MetaDataIOContext>(context),
				});

				using(reader.At())
				{
					type = (MetaDataBlockType)reader.ReadUInt32();
				}
			}
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, MetaDataIOContext context)
		{
			using OffsetOriginToken offsetOriginToken = reader.WithOffsetOrigin(0);

			switch(context.Version)
			{
				case 0:
					ReadVersion0(reader, context);
					break;
				case 1:
					ReadVersion1(reader, context);
					break;
				default:
					reader.ReadOffset(() => ReadVersion2Up(reader, context));
					break;
			}
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, MetaDataIOContext context)
		{
			writer.WriteOffset(() =>
			{
				writer.WriteObjectArray(Blocks, context);
				writer.WriteUInt32((uint)MetaDataBlockType.End);
				writer.WriteUInt32(0);
			});
		}

		/// <summary>
		/// Write metadata; Appropriately flushes the writer and provides an update callback to update the metadata before writing
		/// </summary>
		/// <param name="writer">Writer to write to</param>
		/// <param name="newLabels">Labels to insert into the metadata</param>
		/// <param name="metadataPosition">Seektoken to the position at which the metadata should be written</param>
		/// <param name="metadataUpdate">Update to perform after flushing the writer and before writing the metadata</param>
		public void Write(BinaryObjectWriter writer, LabelDictionary? newLabels, SeekToken? metadataPosition, Action? metadataUpdate)
		{
			metadataPosition ??= ReserveWrite(writer);
			writer.Flush();

			Blocks.RemoveAll(x => x.Type is MetaDataBlockType.Label);
			if(newLabels != null && newLabels.Count > 0)
			{
				Blocks.Add(new LabelsMetaDataBlock()
				{
					Labels = newLabels
				});
			}

			metadataUpdate?.Invoke();

			using(writer.At())
			{
				metadataPosition.Value.Dispose();
				writer.WriteObject(this);
			}
		}

		/// <summary>
		/// Reserves writing space for metadata and returns a <see cref="SeekToken"/> to the reserved spot
		/// </summary>
		/// <param name="writer">The writer to reserve space for</param>
		/// <returns></returns>
		public static SeekToken ReserveWrite(BinaryObjectWriter writer)
		{
			SeekToken result = writer.At();
			writer.WriteOffsetValue(0);
			return result;
		}

		/// <summary>
		/// Tries to get a block of a reflection type
		/// </summary>
		/// <typeparam name="T">Type of the block to look for</typeparam>
		/// <param name="block">Resulting block</param>
		/// <returns>Whether a block was found</returns>
		public bool TryGetBlock<T>([MaybeNullWhen(false)] out T? block) where T : MetaDataBlock
		{
			if(Blocks.OfType<T>().FirstOrDefault() is T found)
			{
				block = found;
				return true;
			}

			block = null;
			return false;
		}

		/// <summary>
		/// Tries to get the first block of type
		/// </summary>
		/// <param name="type">Type of the block to look for</param>
		/// <param name="block">Resulting block</param>
		/// <returns>Whether a block was found</returns>
		public bool TryGetBlock(MetaDataBlockType type, [MaybeNullWhen(false)] out MetaDataBlock? block)
		{
			if(Blocks.FirstOrDefault(x => x.Type == type) is MetaDataBlock found)
			{
				block = found;
				return true;
			}

			block = null;
			return false;
		}

	}
}
