using J113D.Json;
using SA3D.Modeling.Mesh.Ginja.Enums;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Specular color of the geometry.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct GinjaSpecularColorParameter : IGinjaParameter
	{
		internal class JsonConverter : ChildJsonObjectConverter<GinjaParameterType, GinjaSpecularColorParameter, IGinjaParameter>
		{
			private const string _specularColor = nameof(SpecularColor);

			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<GinjaParameterType, IGinjaParameter> ParentConverter => IGinjaParameter.BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _specularColor, new(PropertyTokenType.String, Color.ColorBlack ) }
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(GinjaParameterType key)
			{
				return key == GinjaParameterType.SpecularColor;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _specularColor:
						return JsonSerializer.Deserialize<Color>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override GinjaSpecularColorParameter CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					SpecularColor = (Color)values[_specularColor]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, GinjaSpecularColorParameter value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_specularColor);
				JsonSerializer.Serialize(writer, value.SpecularColor, options);
			}
		}

		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.SpecularColor;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Specular color of the mesh.
		/// </summary>
		public Color SpecularColor
		{
			get => new() { RGBA = Data };
			set => Data = value.RGBA;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Specular color: {SpecularColor}";
		}
	}
}
