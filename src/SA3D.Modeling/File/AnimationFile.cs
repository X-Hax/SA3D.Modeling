using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using Amicitia.IO.Streams;
using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Modeling.AnimationData;
using SA3D.Modeling.File.MetaData;
using SA3D.Modeling.File.MetaData.Blocks;
using System;
using System.Collections.Generic;
using System.IO;
using static SA3D.Modeling.File.FileHeaders;

namespace SA3D.Modeling.File
{
	/// <summary>
	/// Animation file contents.
	/// </summary>
	public class AnimationFile : IFile<AnimationFileIOContext>
	{
		/// <summary>
		/// Whether the file is an NJ binary.
		/// </summary>
		public bool NJFile { get; set; }

		/// <summary>
		/// Animation of the file.
		/// </summary>
		public Animation Animation { get; set; }

		/// <summary>
		/// Metadata in the file.
		/// </summary>
		public MetaDataBlocks MetaData { get; set; }


		/// <summary>
		/// Creates a new, blank animation file
		/// </summary>
		public AnimationFile() : this(new()) { }

		/// <summary>
		/// Creates a new animation file
		/// </summary>
		/// <param name="animation">Animation of the file.</param>
		/// <param name="metaData">Metadata in the file.</param>
		public AnimationFile(Animation animation)
		{
			Animation = animation;
			MetaData = new();
		}


		/// <inheritdoc/>
		public bool Check(BinaryObjectReader reader)
		{
			return CheckIsSAAnimFile(reader) || CheckIsNJAnimFile(reader);
		}

		private bool CheckIsSAAnimFile(BinaryObjectReader reader)
		{
			using SeekToken seekToken = reader.At();
			using EndiannessToken endiannessToken = reader.WithEndian(Endianness.Little);
			return (reader.ReadUInt64() & HeaderMask) == SAANIM;
		}

		private bool CheckIsNJAnimFile(BinaryObjectReader reader)
		{
			return NJBlockUtility.FindBlockOffset(reader, AnimationBlockHeaders, out _);
		}


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, AnimationFileIOContext context)
		{
			if(CheckIsSAAnimFile(reader))
			{
				ReadSA(reader, context);
			}
			else if(CheckIsNJAnimFile(reader))
			{
				ReadNJ(reader, context);
			}
			else
			{
				throw new FormatException("File is not an animation file");
			}
		}

		private void ReadSA(BinaryObjectReader reader, AnimationFileIOContext context)
		{
			using EndiannessToken endiannesToken = reader.WithEndian(Endianness.Little);

			ulong headerVersion = reader.ReadUInt64();
			byte version = (byte)(headerVersion >> 56);
			if(version > CurrentModelVersion)
			{
				throw new FormatException($"File invalid; Unsupported version {version}; Maximum supported version: {CurrentAnimVersion}");
			}

			using(reader.At())
			{
				long animationOffset = reader.ReadOffsetValue();

				if(version >= 2)
				{
					// Animation files of version 2 use metadata version 3
					MetaData = reader.ReadObject<MetaDataBlocks, MetaDataIOContext>(new()
					{
						Version = version == 2 ? 3 : version
					});
				}
				else
				{
					// Version 0 and 1 only had an animation name instead of full metadata
					MetaData = new();

					if(reader.ReadStringOffset(StringBinaryFormat.NullTerminated) is string animationName)
					{
						LabelsMetaDataBlock labelsBlock = new();
						labelsBlock.Labels.Add(animationOffset, animationName);
						MetaData.Blocks.Add(labelsBlock);
					}
				}

				// Version 1 onwards, we got node count and shortrot information after the metadata
				if(version >= 1)
				{
					const uint shortRotMask = (uint)Flag32.B31;
					uint fileNodeCount = reader.ReadUInt32();

					context = new()
					{
						ShortRotations = (fileNodeCount & shortRotMask) != 0,
						KeyframeSetCount = fileNodeCount & ~shortRotMask
					};
				}

			}

			if(context.KeyframeSetCount <= 0)
			{
				throw new ArgumentException("Cannot open version 0 animations without providing node count!");
			}

			Dictionary<long, string> labels = [];
			if(MetaData.TryGetBlock(out LabelsMetaDataBlock? labelBlock))
			{
				labels = labelBlock!.Labels.GetDictFrom();
			}

			AnimationIOContext ioContext = new()
			{
				BaseContext = new()
				{
					PointerLUT = new()
				},

				FileContext = context
			};

			Animation = reader.ReadObjectOffset<Animation, AnimationIOContext>(ioContext, ioContext.BaseContext.PointerLUT)
				?? throw reader.ReadNullReference(nameof(AnimationFile), nameof(Animation));

			NJFile = false;
		}

		private void ReadNJ(BinaryObjectReader reader, AnimationFileIOContext context)
		{
			if(context.KeyframeSetCount <= 0)
			{
				throw new ArgumentException("Cannot read NJ animations without providing node count!");
			}

			using EndiannessToken endiannesToken = reader.WithEndian(reader.CheckEndianness32(4, SeekOrigin.Current));
			Dictionary<long, string> blocks = NJBlockUtility.GetBlockOffsets(reader);

			if(!NJBlockUtility.FindBlockOffset(blocks, AnimationBlockHeaders, out long? animationBlockAddress))
			{
				throw new InvalidOperationException("NJ animation file has no animation block!");
			}

			long modelOffset = animationBlockAddress!.Value + (sizeof(uint) * 2);
			using SeekToken seekToken = reader.At(modelOffset, SeekOrigin.Begin);
			using OffsetOriginToken offsetOriginToken = reader.WithOffsetOrigin();

			AnimationIOContext ioContext = new()
			{
				BaseContext = new()
				{
					PointerLUT = new()
				},

				FileContext = context
			};

			Animation = reader.ReadObjectOffset<Animation, AnimationIOContext>(ioContext, ioContext.BaseContext.PointerLUT)
				?? throw reader.ReadNullReference(nameof(AnimationFile), nameof(Animation));

			NJFile = true;
		}



		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, AnimationFileIOContext context)
		{
			if(NJFile)
			{
				WriteNJ(writer);
			}
			else
			{
				WriteSA(writer);
			}
		}

		private void WriteSA(BinaryObjectWriter writer)
		{
			writer.WriteUInt64(SAANIMVer);

			AnimationIOContext ioContext = new()
			{
				BaseContext = new()
				{
					PointerLUT = new()
				},

				FileContext = new()
				{
					KeyframeSetCount = (uint)Animation.KeyframeSets.Length,
					ShortRotations = Animation.ShortRotations
				}
			};

			writer.WriteObjectOffset(Animation, ioContext);

			MetaData.ReplaceLabels(ioContext.BaseContext.PointerLUT.Labels);
			writer.WriteObject(MetaData);

			uint animFileInfo = (uint)Animation.KeyframeSets.Length;
			if(Animation.ShortRotations)
			{
				animFileInfo |= (uint)Flag32.B31;
			}

			writer.WriteUInt32(animFileInfo);
		}

		private void WriteNJ(BinaryObjectWriter writer)
		{
			throw new NotImplementedException();
		}
	}
}
