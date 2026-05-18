using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.IO;
using SA3D.Modeling.File.MetaData.Weights;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing vertex welding information
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public class WeightsMetaDataBlock : MetaDataBlock
	{
		internal class JsonConverter : ChildJsonObjectConverter<MetaDataBlockType, WeightsMetaDataBlock, MetaDataBlock>
		{
			private const string _weights = nameof(Weights);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<MetaDataBlockType, MetaDataBlock> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _weights, new(PropertyTokenType.Array, null ) }
			});


			protected override bool CheckTypeMatches(MetaDataBlockType key)
			{
				return key == MetaDataBlockType.Weight;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _weights:
						return JsonSerializer.Deserialize<List<MetaWeightNode>>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override WeightsMetaDataBlock CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				List<MetaWeightNode> weights = (List<MetaWeightNode>?)values[_weights]
					?? throw new InvalidDataException($"Weights metadata block requires a \"{_weights}\" property");

				return new()
				{
					Weights = weights
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, WeightsMetaDataBlock value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_weights);
				JsonSerializer.Serialize(writer, value.Weights, options);
			}
		}

		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.Weight;

		/// <summary>
		/// Weights
		/// </summary>
		public List<MetaWeightNode> Weights { get; set; } = [];


		/// <inheritdoc/>
		protected override void ReadContents(BinaryObjectReader reader)
		{
			uint peek;
			using(reader.At())
			{
				peek = reader.ReadUInt32();
			}

			while(peek != uint.MaxValue)
			{
				Weights.Add(reader.ReadObject<MetaWeightNode>());

				using(reader.At())
				{
					peek = reader.ReadUInt32();
				}
			}
		}

		/// <inheritdoc/>
		protected override void WriteContents(BinaryObjectWriter writer)
		{
			writer.WriteObjectArray(Weights);
			writer.WriteUInt32(uint.MaxValue);
		}
	}
}
