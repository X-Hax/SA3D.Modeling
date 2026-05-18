using J113D.Json;
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
	public class BlendAlphaChunk : BitsChunk
	{
		internal class JsonConverter : ChildJsonObjectConverter<PolyChunkType, BlendAlphaChunk, PolyChunk>
		{
			private const string _sourceAlpha = nameof(SourceAlpha);
			private const string _destinationAlpha = nameof(DestinationAlpha);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<PolyChunkType, PolyChunk> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _sourceAlpha, new(PropertyTokenType.String, BlendMode.Zero) },
				{ _destinationAlpha, new(PropertyTokenType.String, BlendMode.Zero) }
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
					case _sourceAlpha:
					case _destinationAlpha:
						return JsonSerializer.Deserialize<BlendMode>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override BlendAlphaChunk CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					SourceAlpha = (BlendMode)values[_sourceAlpha]!,
					DestinationAlpha = (BlendMode)values[_destinationAlpha]!,
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, BlendAlphaChunk value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_sourceAlpha);
				JsonSerializer.Serialize(writer, value.SourceAlpha, options);

				writer.WritePropertyName(_destinationAlpha);
				JsonSerializer.Serialize(writer, value.DestinationAlpha, options);
			}
		}

		/// <summary>
		/// Source blendmode.
		/// </summary>
		public BlendMode SourceAlpha
		{
			get => (BlendMode)((Attributes >> 3) & 7);
			set => Attributes = (byte)((Attributes & ~0x38) | ((byte)value << 3));
		}

		/// <summary>
		/// Destination blendmode.
		/// </summary>
		public BlendMode DestinationAlpha
		{
			get => (BlendMode)(Attributes & 7);
			set => Attributes = (byte)((Attributes & ~7) | (byte)value);
		}

		/// <summary>
		/// Creates a new blendalpha chunk.
		/// </summary>
		public BlendAlphaChunk() : base(PolyChunkType.BlendAlpha) { }

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"BlendAlpha - {SourceAlpha} -> {DestinationAlpha}";
		}
	}
}
