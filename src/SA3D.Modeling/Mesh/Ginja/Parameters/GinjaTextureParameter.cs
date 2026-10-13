using J113D.Json;
using SA3D.Modeling.Mesh.Ginja.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Texture information for the geometry
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct GinjaTextureParameter : IGinjaParameter
	{
		internal class JsonConverter : ChildJsonObjectConverter<GinjaParameterType, GinjaTextureParameter, IGinjaParameter>
		{
			private const string _textureID = nameof(TextureID);
			private const string _textureSlot = nameof(TextureSlot);
			private const string _tiling = nameof(Tiling);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<GinjaParameterType, IGinjaParameter> ParentConverter => IGinjaParameter.BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _textureID, new(PropertyTokenType.Number, (ushort)0u) },
				{ _textureSlot, new(PropertyTokenType.Number, (byte)0) },
				{ _tiling, new(PropertyTokenType.String | PropertyTokenType.Number, default(GinjaTileMode)) },
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(GinjaParameterType key)
			{
				return key == GinjaParameterType.Texture;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _textureID:
						return reader.GetUInt16();
					case _textureSlot:
						return reader.GetByte();
					case _tiling:
						return JsonSerializer.Deserialize<GinjaTileMode>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override GinjaTextureParameter CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					TextureID = (ushort)values[_textureID]!,
					TextureSlot = (byte)values[_textureSlot]!,
					Tiling = (GinjaTileMode)values[_tiling]!,
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, GinjaTextureParameter value, JsonSerializerOptions options)
			{
				writer.WriteNumber(_textureID, value.TextureID);

				if(value.TextureSlot != 0)
				{
					writer.WriteNumber(_textureSlot, value.TextureSlot);
				}

				writer.WritePropertyName(_tiling);
				JsonSerializer.Serialize(writer, value.Tiling, options);
			}
		}

		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.Texture;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Texture index.
		/// </summary>
		public ushort TextureID
		{
			readonly get => (ushort)(Data & 0x1FFF);
			set => Data = (Data & ~0x1FFFu) | value;
		}

		/// <summary>
		/// Index of the texture slot that the texture belongs to
		/// </summary>
		public byte TextureSlot
		{
			readonly get => (byte)((Data >> 13) & 3);
			set => Data = (Data & ~0x6000u) | (uint.Clamp(value, 0, 3) << 13);
		}

		/// <summary>
		/// Texture tiling properties.
		/// </summary>
		public GinjaTileMode Tiling
		{
			readonly get => (GinjaTileMode)((Data >> 16) & (uint)GinjaTileMode.Mask);
			set => Data = (Data & ((~(uint)GinjaTileMode.Mask) << 16)) | ((uint)value << 16);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Texture: [{TextureSlot}] {TextureID} - {(uint)Tiling}";
		}
	}
}
