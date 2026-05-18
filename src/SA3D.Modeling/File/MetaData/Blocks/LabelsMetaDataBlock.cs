using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using J113D.Json;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing structure labels
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public class LabelsMetaDataBlock : MetaDataBlock
	{
		internal class JsonConverter : ChildJsonObjectConverter<MetaDataBlockType, LabelsMetaDataBlock, MetaDataBlock>
		{
			private const string _labels = nameof(Labels);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<MetaDataBlockType, MetaDataBlock> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _labels, new(PropertyTokenType.Object, null ) }
			});


			protected override bool CheckTypeMatches(MetaDataBlockType key)
			{
				return key == MetaDataBlockType.Label;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _labels:
						return JsonSerializer.Deserialize<Dictionary<long, string>>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override LabelsMetaDataBlock CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				Dictionary<long, string> labels = (Dictionary<long, string>?)values[_labels]
					?? throw new InvalidDataException($"Labels metadata block requires a \"{_labels}\" property");

				return new()
				{
					Labels = new(labels)
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, LabelsMetaDataBlock value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_labels);
				JsonSerializer.Serialize(writer, value.Labels.GetDictFrom(), options);
			}
		}


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
