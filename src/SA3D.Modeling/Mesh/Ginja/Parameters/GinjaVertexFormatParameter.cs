using J113D.Json;
using SA3D.Modeling.Mesh.Ginja.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Parameter determining which types of vertex data is used by geometry.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct GinjaVertexFormatParameter : IGinjaParameter
	{
		internal class JsonConverter : ChildJsonObjectConverter<GinjaParameterType, GinjaVertexFormatParameter, IGinjaParameter>
		{
			private const string _vertexType = nameof(VertexType);
			private const string _vertexStructType = nameof(VertexStructType);
			private const string _vertexDataType = nameof(VertexDataType);
			private const string _fractionalBitCount = nameof(FractionalBitCount);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<GinjaParameterType, IGinjaParameter> ParentConverter => IGinjaParameter.BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _vertexType, new(PropertyTokenType.String, null) },
				{ _vertexStructType, new(PropertyTokenType.String, null) },
				{ _vertexDataType, new(PropertyTokenType.String, null) },
				{ _fractionalBitCount, new(PropertyTokenType.Number, (byte)0) },
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(GinjaParameterType key)
			{
				return key == GinjaParameterType.VertexFormat;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _vertexType:
						return JsonSerializer.Deserialize<GinjaVertexType>(ref reader, options);
					case _vertexStructType:
						return JsonSerializer.Deserialize<GinjaStructType>(ref reader, options);
					case _vertexDataType:
						return JsonSerializer.Deserialize<GinjaDataType>(ref reader, options);
					case _fractionalBitCount:
						return reader.GetByte();
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override GinjaVertexFormatParameter CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					VertexType = (GinjaVertexType)values[_vertexType]!,
					VertexStructType = (GinjaStructType)values[_vertexStructType]!,
					VertexDataType = (GinjaDataType)values[_vertexDataType]!,
					FractionalBitCount = (byte)values[_fractionalBitCount]!,

				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, GinjaVertexFormatParameter value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_vertexType);
				JsonSerializer.Serialize(writer, value.VertexType, options);

				writer.WritePropertyName(_vertexStructType);
				JsonSerializer.Serialize(writer, value.VertexStructType, options);

				writer.WritePropertyName(_vertexDataType);
				JsonSerializer.Serialize(writer, value.VertexDataType, options);

				if(value.FractionalBitCount != 0)
				{
					writer.WriteNumber(_fractionalBitCount, value.FractionalBitCount);
				}
			}
		}

		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.VertexFormat;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// The attribute type that this parameter applies for.
		/// </summary>
		public GinjaVertexType VertexType
		{
			readonly get => (GinjaVertexType)((Data >> 16) & 0xFF);
			set => Data = (Data & ~0xFF0000u) | ((uint)value << 16);
		}

		/// <summary>
		/// Vertex struct type being utilized.
		/// </summary>
		public GinjaStructType VertexStructType
		{
			readonly get => (GinjaStructType)((Data >> 12) & 0xF);
			set => Data = (Data & ~0xF000u) | (((uint)value) << 12);
		}

		/// <summary>
		/// Vertex data type being utilized.
		/// </summary>
		public GinjaDataType VertexDataType
		{
			readonly get => (GinjaDataType)((Data >> 8) & 0xF);
			set => Data = (Data & ~0xF00u) | (((uint)value) << 8);
		}

		/// <summary>
		/// Number of fractional bits in integer data
		/// </summary>
		public byte FractionalBitCount
		{
			readonly get => (byte)(Data & 0xFF);
			set => Data = (Data & ~0xFFu) | value;
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Vertex Format: {VertexType} - {VertexStructType} - {VertexDataType} - {FractionalBitCount:X2}";
		}
	}
}
