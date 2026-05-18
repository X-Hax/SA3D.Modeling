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
	/// Diffuse color of the geometry.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct GinjaDiffuseColorParameter : IGinjaParameter
	{
		internal class JsonConverter : ChildJsonObjectConverter<GinjaParameterType, GinjaDiffuseColorParameter, IGinjaParameter>
		{
			private const string _diffuseColor = nameof(DiffuseColor);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<GinjaParameterType, IGinjaParameter> ParentConverter => IGinjaParameter.BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _diffuseColor, new(PropertyTokenType.String, Color.ColorBlack ) }
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(GinjaParameterType key)
			{
				return key == GinjaParameterType.DiffuseColor;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _diffuseColor:
						return JsonSerializer.Deserialize<Color>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override GinjaDiffuseColorParameter CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					DiffuseColor = (Color)values[_diffuseColor]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, GinjaDiffuseColorParameter value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_diffuseColor);
				JsonSerializer.Serialize(writer, value.DiffuseColor, options);
			}
		}

		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.DiffuseColor;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Diffuse color of the mesh.
		/// </summary>
		public Color DiffuseColor
		{
			get => new() { RGBA = Data };
			set => Data = value.RGBA;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Diffuse color: {DiffuseColor}";
		}
	}
}
