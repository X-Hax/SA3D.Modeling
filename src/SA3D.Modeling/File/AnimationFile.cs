using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using Amicitia.IO.Streams;
using J113D.Json;
using SA3D.Common;
using SA3D.Common.Ascii;
using SA3D.Common.IO;
using SA3D.Modeling.AnimationData;
using SA3D.Modeling.File.MetaData;
using SA3D.Modeling.File.MetaData.Blocks;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using static SA3D.Modeling.File.FileHeaders;

namespace SA3D.Modeling.File
{
	/// <summary>
	/// Animation file contents.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public class AnimationFile : IFileSerializable<AnimationFileIOContext>, IAsciiSerializable<AsciiIOContext>
	{
		private class JsonConverter : SimpleJsonObjectConverter<AnimationFile>
		{
			private const string _njFile = nameof(NJFile);
			private const string _animation = nameof(Animation);
			private const string _metaData = nameof(MetaData);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _njFile, new(PropertyTokenType.Bool, false) },
				{ _animation, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _metaData, new(PropertyTokenType.Array, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _njFile:
						return reader.GetBoolean();
					case _animation:
						return JsonSerializer.Deserialize<Animation>(ref reader, options);
					case _metaData:
						return JsonSerializer.Deserialize<MetaDataBlocks>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override AnimationFile Create(ReadOnlyDictionary<string, object?> values)
			{
				Animation animation = (Animation?)values[_animation]
					?? throw new InvalidDataException($"Animationfile requires \"{_animation}\" property");

				return new(animation)
				{
					NJFile = (bool)values[_njFile]!,
					MetaData = (MetaDataBlocks?)values[_metaData] ?? new()
				};
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, AnimationFile value, JsonSerializerOptions options)
			{
				if(value.NJFile)
				{
					writer.WriteBoolean(_njFile, value.NJFile);
				}

				writer.WritePropertyName(_metaData);
				JsonSerializer.Serialize(writer, value.MetaData, options);

				writer.WritePropertyName(_animation);
				JsonSerializer.Serialize(writer, value.Animation, options);
			}
		}

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
						KeyframeSetCount = fileNodeCount & ~shortRotMask,
						BAMSFAngles = context.BAMSFAngles
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
				WriteNJ(writer, context);
			}
			else
			{
				WriteSA(writer, context);
			}
		}

		private void WriteSA(BinaryObjectWriter writer, AnimationFileIOContext context)
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
					ShortRotations = Animation.ShortRotations,
					BAMSFAngles = context.BAMSFAngles
				}
			};

			writer.WriteObjectOffset(Animation, ioContext);
			SeekToken metadataToken = MetaDataBlocks.ReserveWrite(writer);

			uint animFileInfo = (uint)Animation.KeyframeSets.Length;
			if(Animation.ShortRotations)
			{
				animFileInfo |= (uint)Flag32.B31;
			}

			writer.WriteUInt32(animFileInfo);

			MetaData.Write(writer, ioContext.BaseContext.PointerLUT.Labels, metadataToken, null);
		}

		private void WriteNJ(BinaryObjectWriter writer, AnimationFileIOContext context)
		{
			throw new NotImplementedException();
		}

		/// <inheritdoc/>
		public void Write(AsciiWriter writer, AsciiIOContext context)
		{
			string type = "MOTION";

			if(Animation.IsLightAnimation)
			{
				type = "LIGHT_MOTION";
			}
			else if(Animation.IsCameraAnimation)
			{
				type = "CAMERA_MOTION";
			}
			else if(Animation.IsShapeAnimation)
			{
				type = "SHAPE";
			}

			writer.WriteLine($"/* {AsciiHeader} Motion */", 2);
			writer.WriteLine($"/* {type} : {Animation.Label} */", 2);

			writer.WriteObject(Animation, context);

			using(writer.WriteObjectBlock("DEFAULT"))
			{
				writer.WriteLine("#ifndef DEFAULT_MOTION_NAME");
				writer.WriteObjectPropertyLine($"#define DEFAULT_{type}_NAME", Animation);
				writer.WriteLine("#endif", 2);
			}
		}
	}
}
