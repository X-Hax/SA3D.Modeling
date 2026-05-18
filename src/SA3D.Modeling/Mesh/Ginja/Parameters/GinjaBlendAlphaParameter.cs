using J113D.Json;
using SA3D.Modeling.Mesh.Ginja.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// The blending information for the surface of the geometry
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct GinjaBlendAlphaParameter : IGinjaParameter
	{
		internal class JsonConverter : ChildJsonObjectConverter<GinjaParameterType, GinjaBlendAlphaParameter, IGinjaParameter>
		{
			private const string _sourceAlpha = nameof(SourceAlpha);
			private const string _destinationAlpha = nameof(DestinationAlpha);
			private const string _useAlpha = nameof(UseAlpha);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<GinjaParameterType, IGinjaParameter> ParentConverter => IGinjaParameter.BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _sourceAlpha, new(PropertyTokenType.String, DefaultBlendParameter.SourceAlpha) },
				{ _destinationAlpha, new(PropertyTokenType.String, DefaultBlendParameter.DestinationAlpha) },
				{ _useAlpha, new(PropertyTokenType.Bool, DefaultBlendParameter.UseAlpha) },
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(GinjaParameterType key)
			{
				return key == GinjaParameterType.BlendAlpha;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _sourceAlpha:
					case _destinationAlpha:
						return JsonSerializer.Deserialize<BlendMode>(ref reader, options);
					case _useAlpha:
						return reader.GetBoolean();
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override GinjaBlendAlphaParameter CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					SourceAlpha = (BlendMode)values[_sourceAlpha]!,
					DestinationAlpha = (BlendMode)values[_destinationAlpha]!,
					UseAlpha = (bool)values[_useAlpha]!,

				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, GinjaBlendAlphaParameter value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_sourceAlpha);
				JsonSerializer.Serialize(writer, value.SourceAlpha, options);

				writer.WritePropertyName(_destinationAlpha);
				JsonSerializer.Serialize(writer, value.DestinationAlpha, options);

				writer.WriteBoolean(_useAlpha, value.UseAlpha);
			}
		}

		/// <summary>
		/// Blend alpha parameter with default values.
		/// </summary>
		public static readonly GinjaBlendAlphaParameter DefaultBlendParameter
			= new() { SourceAlpha = BlendMode.SrcAlpha, DestinationAlpha = BlendMode.SrcAlphaInverted, UseAlpha = true };

		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.BlendAlpha;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Source pixel blendmode.
		/// </summary>
		public BlendMode SourceAlpha
		{
			readonly get => (BlendMode)((Data >> 11) & 7);
			set => Data = (Data & 0xFFFFC7FF) | (((uint)value & 7) << 11);
		}

		/// <summary>
		/// Destination pixel blendmode.
		/// </summary>
		public BlendMode DestinationAlpha
		{
			readonly get => (BlendMode)((Data >> 8) & 7);
			set => Data = (Data & 0xFFFFF8FF) | (((uint)value & 7) << 8);
		}

		/// <summary>
		/// Whether to use blending.
		/// </summary>
		public bool UseAlpha
		{
			readonly get => (Data & 0x4000u) != 0;
			set => Data = (Data & ~0x4000u) | (value ? 0x4000u : 0);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Blendalpha: {UseAlpha} / {SourceAlpha} -> {DestinationAlpha}";
		}
	}
}
