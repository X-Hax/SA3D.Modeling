using J113D.Json;
using SA3D.Common;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Sets the blendmode of the following strip chunks.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class BlendAlphaChunk : BitsChunk
	{
		internal class JsonConverter : ChildJsonObjectConverter<PolyChunkType, BlendAlphaChunk, PolyChunk>
		{
			private const string _sourceBlendMode = nameof(SourceBlendMode);
			private const string _destinationBlendMode = nameof(DestinationBlendMode);
			private const string _sourceSelect = nameof(SourceSelect);
			private const string _destinationSelect = nameof(DestinationSelect);

			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<PolyChunkType, PolyChunk> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _sourceBlendMode, new(PropertyTokenType.String, BlendMode.Zero) },
				{ _destinationBlendMode, new(PropertyTokenType.String, BlendMode.Zero) },
				{ _sourceSelect, new(PropertyTokenType.Bool, false) },
				{ _destinationSelect, new(PropertyTokenType.Bool, false) },
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(PolyChunkType key)
			{
				return key == PolyChunkType.BlendAlpha;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _sourceBlendMode:
					case _destinationBlendMode:
						return JsonSerializer.Deserialize<BlendMode>(ref reader, options);
					case _sourceSelect:
					case _destinationSelect:
						return reader.GetBoolean();
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override BlendAlphaChunk CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					SourceBlendMode = (BlendMode)values[_sourceBlendMode]!,
					DestinationBlendMode = (BlendMode)values[_destinationBlendMode]!,
					SourceSelect = (bool)values[_sourceSelect]!,
					DestinationSelect = (bool)values[_destinationSelect]!,
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, BlendAlphaChunk value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_sourceBlendMode);
				JsonSerializer.Serialize(writer, value.SourceBlendMode, options);

				writer.WritePropertyName(_destinationBlendMode);
				JsonSerializer.Serialize(writer, value.DestinationBlendMode, options);

				if(value.SourceSelect)
				{
					writer.WriteBoolean(_sourceSelect, value.SourceSelect);
				}

				if(value.DestinationSelect)
				{
					writer.WriteBoolean(_destinationSelect, value.DestinationSelect);
				}
			}
		}

		/// <summary>
		/// Source blendmode.
		/// </summary>
		public BlendMode SourceBlendMode
		{
			get => (BlendMode)((Attributes >> 3) & 7);
			set => Attributes = (byte)((Attributes & ~0x38) | ((byte)value << 3));
		}

		/// <summary>
		/// Destination blendmode.
		/// </summary>
		public BlendMode DestinationBlendMode
		{
			get => (BlendMode)(Attributes & 7);
			set => Attributes = (byte)((Attributes & ~7) | (byte)value);
		}

		/// <summary>
		/// Source select flag
		/// </summary>
		public bool SourceSelect
		{
			get => (Attributes & (byte)Flag8.B7) != 0;
			set => Attributes = (byte)(value ? (Attributes | (byte)Flag8.B7) : (Attributes & ~(byte)Flag8.B7));
		}

		/// <summary>
		/// Source select flag
		/// </summary>
		public bool DestinationSelect
		{
			get => (Attributes & (byte)Flag8.B6) != 0;
			set => Attributes = (byte)(value ? (Attributes | (byte)Flag8.B6) : (Attributes & ~(byte)Flag8.B6));
		}

		/// <summary>
		/// Creates a new blendalpha chunk.
		/// </summary>
		public BlendAlphaChunk() : base(PolyChunkType.BlendAlpha) { }

		/// <inheritdoc/>
		protected override string GetAsciiAttributes()
		{
			string result = 
				AsciiMaps.SourceBlendModeMap.FindKey(SourceBlendMode)
				+ "|" + AsciiMaps.DestinationBlendModeMap.FindKey(DestinationBlendMode);

			if(SourceSelect)
			{
				result += "|FBS_SEL";
			}

			if(DestinationSelect)
			{
				result += "|FBD_SEL";
			}

			return result;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"BlendAlpha - {SourceBlendMode} -> {DestinationBlendMode}";
		}
	}
}
