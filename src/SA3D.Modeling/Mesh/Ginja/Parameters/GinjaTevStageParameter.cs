using J113D.Json;
using SA3D.Modeling.Mesh.Ginja.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Unknown but not unused.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct GinjaTevStageParameter : IGinjaParameter
	{
		internal class JsonConverter : ChildJsonObjectConverter<GinjaParameterType, GinjaTevStageParameter, IGinjaParameter>
		{
			private const string _tevStage = nameof(TevStage);
			private const string _texCoord = nameof(TexCoord);
			private const string _texMap = nameof(TexMap);
			private const string _colorChannel = nameof(ColorChannel);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<GinjaParameterType, IGinjaParameter> ParentConverter => IGinjaParameter.BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _tevStage, new(PropertyTokenType.String, DefaultValues.TevStage) },
				{ _texCoord, new(PropertyTokenType.String, DefaultValues.TexCoord) },
				{ _texMap, new(PropertyTokenType.String, DefaultValues.TexMap) },
				{ _colorChannel, new(PropertyTokenType.String, DefaultValues.ColorChannel) },
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(GinjaParameterType key)
			{
				return key == GinjaParameterType.TevStage;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _tevStage:
						return JsonSerializer.Deserialize<GinjaTevStageID>(ref reader, options);
					case _texCoord:
						return JsonSerializer.Deserialize<GinjaTexCoordID>(ref reader, options);
					case _texMap:
						return JsonSerializer.Deserialize<GinjaTexMapID>(ref reader, options);
					case _colorChannel:
						return JsonSerializer.Deserialize<GinjaColorChannelID>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override GinjaTevStageParameter CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					TevStage = (GinjaTevStageID)values[_tevStage]!,
					TexCoord = (GinjaTexCoordID)values[_texCoord]!,
					TexMap = (GinjaTexMapID)values[_texMap]!,
					ColorChannel = (GinjaColorChannelID)values[_colorChannel]!,
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, GinjaTevStageParameter value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_tevStage);
				JsonSerializer.Serialize(writer, value.TevStage, options);

				writer.WritePropertyName(_texCoord);
				JsonSerializer.Serialize(writer, value.TexCoord, options);

				writer.WritePropertyName(_texMap);
				JsonSerializer.Serialize(writer, value.TexMap, options);

				writer.WritePropertyName(_colorChannel);
				JsonSerializer.Serialize(writer, value.ColorChannel, options);

			}
		}

		/// <summary>
		/// Default values parameter.
		/// </summary>
		public static readonly GinjaTevStageParameter DefaultValues = new()
		{
			TevStage = GinjaTevStageID.TevStage0,
			TexCoord = GinjaTexCoordID.TexCoord0,
			TexMap = GinjaTexMapID.TexMap0,
			ColorChannel = GinjaColorChannelID.Color0A0
		};

		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.TevStage;

		/// <inheritdoc/>
		public uint Data { get; set; }


		/// <summary>
		/// Tev stage to use
		/// </summary>
		public GinjaTevStageID TevStage
		{
			readonly get => (GinjaTevStageID)((Data >> 12) & 0xF);
			set => Data = (Data & ~0xFu) | byte.Clamp((byte)value, 0, 0xF);
		}

		/// <summary>
		/// Texcoord id to use
		/// </summary>
		public GinjaTexCoordID TexCoord
		{
			readonly get
			{
				byte value = (byte)((Data >> 8) & 0xF);
				return value < 8 ? (GinjaTexCoordID)value : GinjaTexCoordID.TexCoordNull;
			}
			set
			{
				byte val = value is >= GinjaTexCoordID.TexCoordMax ? (byte)GinjaTexCoordID.TexCoordNull : (byte)value;
				Data = (Data & ~0xF00u) | ((uint)val << 8);
			}
		}

		/// <summary>
		/// Texmap to use. Setting this to <see cref="GinjaTexMapID.TexMapNull"/> will clear the tev stage instead of "modulo-ing" to it.
		/// </summary>
		public GinjaTexMapID TexMap
		{
			readonly get
			{
				byte value = (byte)((Data >> 4) & 0xF);
				return value < 8 ? (GinjaTexMapID)value : GinjaTexMapID.TexMapNull;
			}
			set
			{
				byte val = value is >= GinjaTexMapID.TexMapMax ? (byte)GinjaTexMapID.TexMapNull : (byte)value;
				Data = (Data & ~0xF0u) | ((uint)val << 4);
			}
		}

		/// <summary>
		/// Color channel to write to.
		/// </summary>
		public GinjaColorChannelID ColorChannel
		{
			readonly get => (GinjaColorChannelID)(Data & 0xF);
			set => Data = (Data & ~0xFu) | byte.Clamp((byte)value, 0, 0xF);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Unknown: {TevStage} - {TexCoord} - {TexMap} - {ColorChannel}";
		}
	}
}
