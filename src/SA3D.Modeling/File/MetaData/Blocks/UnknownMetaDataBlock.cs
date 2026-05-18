using Amicitia.IO.Binary;
using J113D.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing unknown data
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public class UnknownMetaDataBlock : MetaDataBlock
	{
		internal class JsonConverter : ChildJsonObjectConverter<MetaDataBlockType, UnknownMetaDataBlock, MetaDataBlock>
		{
			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<MetaDataBlockType, MetaDataBlock> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>());


			protected override bool CheckTypeMatches(MetaDataBlockType key)
			{
				return key
					is not MetaDataBlockType.Label
					and not MetaDataBlockType.Animation
					and not MetaDataBlockType.Morph
					and not MetaDataBlockType.Author
					and not MetaDataBlockType.Description
					and not MetaDataBlockType.ActionName
					and not MetaDataBlockType.ObjectName
					and not MetaDataBlockType.Weight
					and not MetaDataBlockType.End;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override UnknownMetaDataBlock CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				byte[] data = (byte[]?)values[BaseJsonConverter._data]
					?? throw new InvalidDataException($"Unknown metadata blocks require a \"{BaseJsonConverter._data}\" property");

				return new UnknownMetaDataBlock()
				{
					RawType = (uint)(MetaDataBlockType?)values[BaseJsonConverter._type]!,
					Data = data
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, UnknownMetaDataBlock value, JsonSerializerOptions options)
			{
				writer.WriteString(BaseJsonConverter._data, Convert.ToBase64String(value.Data));
			}
		}

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
