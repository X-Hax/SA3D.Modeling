using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using Amicitia.IO.Streams;
using J113D.Json;
using SA3D.Common.IO;
using SA3D.Modeling.File.MetaData.Blocks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.File.MetaData
{
	/// <summary>
	/// Base meta data block class
	/// </summary>
	[JsonConverter(typeof(BaseJsonConverter))]
	public abstract class MetaDataBlock : IBinarySerializable<MetaDataIOContext>
	{
		internal class BaseJsonConverter : ParentJsonObjectConverter<MetaDataBlockType, MetaDataBlock>
		{
			public static readonly BaseJsonConverter instance = new();

			public const string _type = nameof(Type);
			public const string _data = nameof(UnknownMetaDataBlock.Data);

			/// <inheritdoc/>
			protected override string KeyPropertyName => _type;

			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _type, new(PropertyTokenType.String, null) },
				{ _data, new(PropertyTokenType.String, null) }
			});


			/// <inheritdoc/>
			protected override object? ReadBaseValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _type:
						return JsonSerializer.Deserialize<MetaDataBlockType>(ref reader, options);
					case _data:
						return Convert.FromBase64String(reader.GetString()!);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override MetaDataBlock CreateBase(ReadOnlyDictionary<string, object?> values)
			{
				byte[] data = (byte[]?)values[_data]
					?? throw new InvalidDataException($"Unknown metadata blocks require a \"{_data}\" property");

				return new UnknownMetaDataBlock()
				{
					RawType = (uint)(MetaDataBlockType?)values[_type]!,
					Data = data
				};
			}

			/// <inheritdoc/>
			protected override void WriteBaseValues(Utf8JsonWriter writer, MetaDataBlock value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_type);
				JsonSerializer.Serialize(writer, value.Type, options);
			}

			/// <inheritdoc/>
			protected override MetaDataBlockType GetKeyFromValue(MetaDataBlock value)
			{
				return value.Type;
			}

			/// <inheritdoc/>
			protected override Dictionary<MetaDataBlockType, IChildJsonConverter<MetaDataBlock>> CreateConverters()
			{
				return new()
				{
					{ MetaDataBlockType.Label, new LabelsMetaDataBlock.JsonConverter() },
					{ MetaDataBlockType.Animation, new AnimationFilesMetaDataBlock.JsonConverter() },
					{ MetaDataBlockType.Morph, new MorphFilesMetaDataBlock.JsonConverter() },
					{ MetaDataBlockType.Author, new AuthorMetaDataBlock.JsonConverter() },
					{ MetaDataBlockType.Description, new DescriptionMetaDataBlock.JsonConverter() },
					{ MetaDataBlockType.Tool, new ToolMetaDataBlock.JsonConverter() },
					{ MetaDataBlockType.ActionName, new ActionNameMetaDataBlock.JsonConverter() },
					{ MetaDataBlockType.ObjectName, new ObjectNameMetaDataBlock.JsonConverter() },
					{ MetaDataBlockType.Weight, new WeightsMetaDataBlock.JsonConverter() },
				};
			}
		}

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

			reader.Skip(sizeof(uint)); // skipping type
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
			writer.WriteUInt32((uint)Type);
			SeekToken blockSizeOffset = writer.At();
			writer.WriteUInt32(0); // placeholder

			long start = writer.Position;

			using(writer.WithOffsetOrigin())
			{
				WriteContents(writer);
			}

			long end = writer.Position;

			using(writer.At())
			{
				blockSizeOffset.Dispose();
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
