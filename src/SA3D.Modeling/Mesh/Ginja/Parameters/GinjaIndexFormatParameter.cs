using J113D.Json;
using SA3D.Modeling.Mesh.Ginja.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Holds information about the triangle lists of geometry.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct GinjaIndexFormatParameter : IGinjaParameter
	{
		internal class JsonConverter : ChildJsonObjectConverter<GinjaParameterType, GinjaIndexFormatParameter, IGinjaParameter>
		{
			private const string _indexFormat = nameof(IndexFormat);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<GinjaParameterType, IGinjaParameter> ParentConverter => IGinjaParameter.BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _indexFormat, new(PropertyTokenType.String | PropertyTokenType.Number, default(GinjaIndexFormat)) }
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(GinjaParameterType key)
			{
				return key == GinjaParameterType.IndexFormat;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _indexFormat:
						return JsonSerializer.Deserialize<GinjaIndexFormat>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override GinjaIndexFormatParameter CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					IndexFormat = (GinjaIndexFormat)values[_indexFormat]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, GinjaIndexFormatParameter value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_indexFormat);
				JsonSerializer.Serialize(writer, value.IndexFormat, options);
			}
		}

		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.IndexFormat;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Holds information about the triangle lists of geometry.
		/// </summary>
		public GinjaIndexFormat IndexFormat
		{
			readonly get => (GinjaIndexFormat)Data;
			set => Data = (uint)value;
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Index Format: {(uint)IndexFormat}";
		}
	}
}
