using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Modeling.Mesh.Ginja.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Base interface for all Ginja parameter types. 
	/// <br/> Used to store geometry information (like materials).
	/// </summary>
	[JsonConverter(typeof(BaseJsonConverter))]
	public interface IGinjaParameter : IBinarySerializable
	{
		internal class BaseJsonConverter : ParentJsonObjectConverter<GinjaParameterType, IGinjaParameter>
		{
			public static readonly BaseJsonConverter instance = new();

			public const string _type = nameof(IGinjaParameter.Type);

			/// <inheritdoc/>
			protected override string KeyPropertyName => _type;

			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _type, new(PropertyTokenType.String, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadBaseValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _type:
						return JsonSerializer.Deserialize<GinjaParameterType>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override IGinjaParameter CreateBase(ReadOnlyDictionary<string, object?> values)
			{
				throw new NotSupportedException("Cannot create typeless gc parameter!");
			}

			/// <inheritdoc/>
			protected override void WriteBaseValues(Utf8JsonWriter writer, IGinjaParameter value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_type);
				JsonSerializer.Serialize(writer, value.Type, options);
			}

			/// <inheritdoc/>
			protected override GinjaParameterType GetKeyFromValue(IGinjaParameter value)
			{
				return value.Type;
			}

			/// <inheritdoc/>
			protected override Dictionary<GinjaParameterType, IChildJsonConverter<IGinjaParameter>> CreateConverters()
			{
				return new()
				{
					{ GinjaParameterType.VertexFormat, new GinjaVertexFormatParameter.JsonConverter() },
					{ GinjaParameterType.IndexFormat, new GinjaIndexFormatParameter.JsonConverter() },
					{ GinjaParameterType.StripFlags, new GinjaStripFlagsParameter.JsonConverter() },
					{ GinjaParameterType.BlendAlpha, new GinjaBlendAlphaParameter.JsonConverter() },
					{ GinjaParameterType.AmbientColor, new GinjaAmbientColorParameter.JsonConverter() },
					{ GinjaParameterType.DiffuseColor, new GinjaDiffuseColorParameter.JsonConverter() },
					{ GinjaParameterType.SpecularColor, new GinjaSpecularColorParameter.JsonConverter() },
					{ GinjaParameterType.Texture, new GinjaTextureParameter.JsonConverter() },
					{ GinjaParameterType.TevStage, new GinjaTevStageParameter.JsonConverter() },
					{ GinjaParameterType.TexGen, new GinjaTexGenParameter.JsonConverter() },
				};
			}
		}

		/// <summary>
		/// The type of parameter.
		/// </summary>
		public GinjaParameterType Type { get; }

		/// <summary>
		/// All parameter data is stored in these 4 bytes.
		/// </summary>
		public uint Data { get; set; }

		/// <inheritdoc/>
		void IBinarySerializable.Read(BinaryObjectReader reader)
		{
			reader.Skip(4);
			Data = reader.ReadUInt32();
		}

		/// <inheritdoc/>
		void IBinarySerializable.Write(BinaryObjectWriter writer)
		{
			writer.WriteByte((byte)Type);
			writer.WriteBytes([0, 0, 0]);
			writer.WriteUInt32(Data);
		}

		/// <summary>
		/// Reads a parameter from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <returns>The parameter that was read.</returns>
		public static IGinjaParameter ReadParameter(BinaryObjectReader reader)
		{
			GinjaParameterType paramType = (GinjaParameterType)reader.ReadByte();
			reader.Skip(3);

			IGinjaParameter result = paramType switch
			{
				GinjaParameterType.VertexFormat => new GinjaVertexFormatParameter(),
				GinjaParameterType.IndexFormat => new GinjaIndexFormatParameter(),
				GinjaParameterType.StripFlags => new GinjaStripFlagsParameter(),
				GinjaParameterType.BlendAlpha => new GinjaBlendAlphaParameter(),
				GinjaParameterType.DiffuseColor => new GinjaDiffuseColorParameter(),
				GinjaParameterType.AmbientColor => new GinjaAmbientColorParameter(),
				GinjaParameterType.SpecularColor => new GinjaSpecularColorParameter(),
				GinjaParameterType.Texture => new GinjaTextureParameter(),
				GinjaParameterType.TevStage => new GinjaTevStageParameter(),
				GinjaParameterType.TexGen => new GinjaTexGenParameter(),
				_ => throw new NotSupportedException($"Ginja parameter type {paramType} not supported.")
			};

			result.Data = reader.ReadUInt32();

			return result;
		}
	}
}
