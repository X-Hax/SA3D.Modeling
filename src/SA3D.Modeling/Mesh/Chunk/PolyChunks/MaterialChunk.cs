using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Material information for the following strip chunks
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public class MaterialChunk : SizedChunk
	{
		internal class JsonConverter : ChildJsonObjectConverter<PolyChunkType, MaterialChunk, PolyChunk>
		{
			private const string _sourceAlpha = nameof(SourceAlpha);
			private const string _destinationAlpha = nameof(DestinationAlpha);
			private const string _diffuse = nameof(Diffuse);
			private const string _ambient = nameof(Ambient);
			private const string _specular = nameof(Specular);
			private const string _specularExponent = nameof(SpecularExponent);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<PolyChunkType, PolyChunk> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _sourceAlpha, new(PropertyTokenType.String, BlendMode.Zero) },
				{ _destinationAlpha, new(PropertyTokenType.String, BlendMode.Zero) },
				{ _diffuse, new(PropertyTokenType.String, null, true) },
				{ _ambient, new(PropertyTokenType.String, null, true) },
				{ _specular, new(PropertyTokenType.String, null, true) },
				{ _specularExponent, new(PropertyTokenType.Number, (byte)0) },
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(PolyChunkType key)
			{
				return key is not PolyChunkType.Material_Bump
					and >= PolyChunkType.Material_Diffuse
					and <= PolyChunkType.Material_DiffuseAmbientSpecular2;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _sourceAlpha:
					case _destinationAlpha:
						return JsonSerializer.Deserialize<BlendMode>(ref reader, options);
					case _diffuse:
					case _ambient:
					case _specular:
						return JsonSerializer.Deserialize<Color?>(ref reader, options);
					case _specularExponent:
						return reader.GetByte();
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override MaterialChunk CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				PolyChunkType type = (PolyChunkType)values[BaseJsonConverter._type]!;

				bool second = type
					is PolyChunkType.Material_Diffuse2
					or PolyChunkType.Material_Ambient2
					or PolyChunkType.Material_DiffuseAmbient2
					or PolyChunkType.Material_Specular2
					or PolyChunkType.Material_DiffuseSpecular2
					or PolyChunkType.Material_AmbientSpecular2
					or PolyChunkType.Material_DiffuseAmbientSpecular2;

				return new()
				{
					SourceAlpha = (BlendMode)values[_sourceAlpha]!,
					DestinationAlpha = (BlendMode)values[_destinationAlpha]!,
					Diffuse = (Color?)values[_diffuse]!,
					Ambient = (Color?)values[_ambient]!,
					Specular = (Color?)values[_specular]!,
					SpecularExponent = (byte)values[_specularExponent]!,
					Second = second,
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, MaterialChunk value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_sourceAlpha);
				JsonSerializer.Serialize(writer, value.SourceAlpha, options);

				writer.WritePropertyName(_destinationAlpha);
				JsonSerializer.Serialize(writer, value.DestinationAlpha, options);

				if(value.Diffuse != null)
				{
					writer.WritePropertyName(_diffuse);
					JsonSerializer.Serialize(writer, value.Diffuse, options);
				}

				if(value.Ambient != null)
				{
					writer.WritePropertyName(_ambient);
					JsonSerializer.Serialize(writer, value.Ambient, options);
				}

				if(value.Specular != null)
				{
					writer.WritePropertyName(_specular);
					JsonSerializer.Serialize(writer, value.Specular, options);
				}

				if(value.SpecularExponent != 0)
				{
					writer.WriteNumber(_specularExponent, value.SpecularExponent);
				}
			}
		}

		/// <summary>
		/// Whether the chunk is for the second material slot
		/// </summary>
		public bool Second
		{
			get => ((byte)Type & 0x08) != 0;
			set => TypeAttribute(0x08, value);
		}

		/// <inheritdoc/>
		public override ushort Size
		{
			get
			{
				byte type = (byte)Type;

				return (ushort)(2 * (
					(type & 1)
					+ ((type >> 1) & 1)
					+ ((type >> 2) & 1)
				));
			}
		}

		/// <summary>
		/// Source blendmode
		/// </summary>
		public BlendMode SourceAlpha
		{
			get => (BlendMode)((Attributes >> 3) & 7);
			set => Attributes = (byte)((Attributes & ~0x38) | ((byte)value << 3));
		}

		/// <summary>
		/// Destination blendmode
		/// </summary>
		public BlendMode DestinationAlpha
		{
			get => (BlendMode)(Attributes & 7);
			set => Attributes = (byte)((Attributes & ~7) | (byte)value);
		}

		/// <summary>
		/// Diffuse color
		/// </summary>
		public Color? Diffuse
		{
			get;
			set
			{
				TypeAttribute(0x01, value.HasValue);
				field = value;
			}
		}

		/// <summary>
		/// Ambient color
		/// </summary>
		public Color? Ambient
		{
			get;
			set
			{
				TypeAttribute(0x02, value.HasValue);
				field = value;
			}
		}

		/// <summary>
		/// Specular color
		/// </summary>
		public Color? Specular
		{
			get;
			set
			{
				TypeAttribute(0x04, value.HasValue);
				field = value;
			}
		}

		/// <summary>
		/// Specular exponent <br/>
		/// Requires <see cref="Specular"/> to be set
		/// </summary>
		public byte SpecularExponent { get; set; }


		/// <summary>
		/// Creates a new, empty material chunk.
		/// </summary>
		public MaterialChunk() : base(PolyChunkType.Material_Empty) { }


		/// <inheritdoc/>
		protected override bool IsTypeApplicable(PolyChunkType type)
		{
			return type is >= PolyChunkType.Material_Empty and <= PolyChunkType.Material_DiffuseAmbientSpecular2;
		}

		private void TypeAttribute(byte val, bool state)
		{
			byte type = (byte)Type;
			Type = (PolyChunkType)(byte)(state
				? type | val
				: type & ~val);
		}

		/// <inheritdoc/>
		public override void Read(BinaryObjectReader reader)
		{
			base.Read(reader);

			byte type = (byte)Type;
			if((type & 0x01) != 0)
			{
				Diffuse = reader.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_16);
			}

			if((type & 0x02) != 0)
			{
				Ambient = reader.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_16);
			}

			if((type & 0x04) != 0)
			{
				Color spec = reader.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_16);
				SpecularExponent = spec.Alpha;
				spec.Alpha = 255;
				Specular = spec;
			}

		}

		/// <inheritdoc/>
		protected override void WriteData(BinaryObjectWriter writer)
		{
			base.WriteData(writer);

			if(Diffuse.HasValue)
			{
				writer.WriteObject(Diffuse.Value, ColorIOType.ARGB8_16);
			}

			if(Ambient.HasValue)
			{
				writer.WriteObject(Ambient.Value, ColorIOType.ARGB8_16);
			}

			if(Specular.HasValue)
			{
				Color wSpecular = Specular.Value;
				wSpecular.Alpha = SpecularExponent;
				writer.WriteObject(wSpecular, ColorIOType.ARGB8_16);
			}
		}
	}
}
