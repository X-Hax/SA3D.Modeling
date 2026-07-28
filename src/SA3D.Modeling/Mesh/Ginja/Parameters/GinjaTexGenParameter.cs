using J113D.Json;
using SA3D.Modeling.Mesh.Ginja.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Determines where or how the geometry gets the texture coordinates.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct GinjaTexGenParameter : IGinjaParameter
	{
		internal class JsonConverter : ChildJsonObjectConverter<GinjaParameterType, GinjaTexGenParameter, IGinjaParameter>
		{
			private const string _texCoord = nameof(TexCoord);
			private const string _texGenType = nameof(TexGenType);
			private const string _texGenSource = nameof(TexGenSource);
			private const string _matrixID = nameof(MatrixID);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<GinjaParameterType, IGinjaParameter> ParentConverter => IGinjaParameter.BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _texCoord, new(PropertyTokenType.String, DefaultValues.TexCoord) },
				{ _texGenType, new(PropertyTokenType.String, DefaultValues.TexGenType) },
				{ _texGenSource, new(PropertyTokenType.String, DefaultValues.TexGenSource) },
				{ _matrixID, new(PropertyTokenType.String, DefaultValues.MatrixID) },
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(GinjaParameterType key)
			{
				return key == GinjaParameterType.TexGen;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _texCoord:
						return JsonSerializer.Deserialize<GinjaTexCoordID>(ref reader, options);
					case _texGenType:
						return JsonSerializer.Deserialize<GinjaTexGenType>(ref reader, options);
					case _texGenSource:
						return JsonSerializer.Deserialize<GinjaTexGenSource>(ref reader, options);
					case _matrixID:
						return JsonSerializer.Deserialize<GinjaTexGenMatrix>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override GinjaTexGenParameter CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					TexCoord = (GinjaTexCoordID)values[_texCoord]!,
					TexGenType = (GinjaTexGenType)values[_texGenType]!,
					TexGenSource = (GinjaTexGenSource)values[_texGenSource]!,
					MatrixID = (GinjaTexGenMatrix)values[_matrixID]!,
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, GinjaTexGenParameter value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_texCoord);
				JsonSerializer.Serialize(writer, value.TexCoord, options);

				writer.WritePropertyName(_texGenType);
				JsonSerializer.Serialize(writer, value.TexGenType, options);

				writer.WritePropertyName(_texGenSource);
				JsonSerializer.Serialize(writer, value.TexGenSource, options);

				writer.WritePropertyName(_matrixID);
				JsonSerializer.Serialize(writer, value.MatrixID, options);

			}
		}

		/// <summary>
		/// Default values parameter.
		/// </summary>
		public static readonly GinjaTexGenParameter DefaultValues = new()
		{
			TexCoord = GinjaTexCoordID.TexCoord0,
			TexGenType = GinjaTexGenType.Matrix2x4,
			TexGenSource = GinjaTexGenSource.TexCoord0,
			MatrixID = GinjaTexGenMatrix.Identity
		};

		/// <summary>
		/// Environment mapping parameter.
		/// </summary>
		public static readonly GinjaTexGenParameter EnvironmentMapValues = new()
		{
			TexCoord = GinjaTexCoordID.TexCoord0,
			TexGenType = GinjaTexGenType.Matrix3x4,
			TexGenSource = GinjaTexGenSource.Normal,
			MatrixID = GinjaTexGenMatrix.Matrix4
		};


		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.TexGen;

		/// <inheritdoc/>
		public uint Data { get; set; }


		/// <summary>
		/// Output channel to which calculated texture coordinates should be written to.
		/// </summary>
		public GinjaTexCoordID TexCoord
		{
			readonly get => (GinjaTexCoordID)((Data >> 16) & 0xFF);
			set => Data = (Data & 0xFF00FFFF) | ((uint)value << 16);
		}

		/// <summary>
		/// The function type used to generate the texture coordinates.
		/// </summary>
		public GinjaTexGenType TexGenType
		{
			readonly get => (GinjaTexGenType)((Data >> 12) & 0xF);
			set => Data = (Data & 0xFFFF0FFF) | ((uint)value << 12);
		}

		/// <summary>
		/// Input values to use for when calculating texture coordinates.
		/// </summary>
		public GinjaTexGenSource TexGenSource
		{
			readonly get => (GinjaTexGenSource)((Data >> 4) & 0xFF);
			set => Data = (Data & 0xFFFFF00F) | ((uint)value << 4);
		}

		/// <summary>
		/// Matrix slot to use when using <see cref="GinjaTexGenType.Matrix2x4"/> or <see cref="GinjaTexGenType.Matrix3x4"/>.
		/// </summary>
		public GinjaTexGenMatrix MatrixID
		{
			readonly get => (GinjaTexGenMatrix)(Data & 0xF);
			set => Data = (Data & 0xFFFFFFF0) | (uint)value;
		}


		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Texcoord: {TexCoord} - {TexGenType} - {TexGenSource} - {MatrixID}";
		}
	}
}
