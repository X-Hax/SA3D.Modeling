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
	/// Ambient color of the geometry.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct GinjaAmbientColorParameter : IGinjaParameter
	{
		internal class JsonConverter : ChildJsonObjectConverter<GinjaParameterType, GinjaAmbientColorParameter, IGinjaParameter>
		{
			private const string _ambientColor = nameof(AmbientColor);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<GinjaParameterType, IGinjaParameter> ParentConverter => IGinjaParameter.BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _ambientColor, new(PropertyTokenType.String, Color.ColorBlack ) }
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(GinjaParameterType key)
			{
				return key == GinjaParameterType.AmbientColor;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _ambientColor:
						return JsonSerializer.Deserialize<Color>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override GinjaAmbientColorParameter CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					AmbientColor = (Color)values[_ambientColor]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, GinjaAmbientColorParameter value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_ambientColor);
				JsonSerializer.Serialize(writer, value.AmbientColor, options);
			}
		}

		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.AmbientColor;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Ambient color of the mesh.
		/// </summary>
		public Color AmbientColor
		{
			get => new() { RGBA = Data };
			set => Data = value.RGBA;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Ambient color: {AmbientColor}";
		}
	}
}
