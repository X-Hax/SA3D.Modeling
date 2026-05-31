using J113D.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Sets the specular exponent of the following strip chunks
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public class SpecularExponentChunk : BitsChunk
	{
		internal class JsonConverter : ChildJsonObjectConverter<PolyChunkType, SpecularExponentChunk, PolyChunk>
		{
			private const string _specularExponent = nameof(SpecularExponent);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<PolyChunkType, PolyChunk> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _specularExponent, new(PropertyTokenType.Number, (byte)0) }
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(PolyChunkType key)
			{
				return key == PolyChunkType.SpecularExponent;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _specularExponent:
						return reader.GetByte();
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override SpecularExponentChunk CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					SpecularExponent = (byte)values[_specularExponent]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, SpecularExponentChunk value, JsonSerializerOptions options)
			{
				writer.WriteNumber(_specularExponent, value.SpecularExponent);
			}
		}

		/// <summary>
		/// Specular exponent <br/>
		/// Ranges from 0 to 16
		/// </summary>
		public byte SpecularExponent
		{
			get => (byte)(Attributes & 0x1F);
			set => Attributes = (byte)((Attributes & ~0x1F) | Math.Min(value, (byte)16));
		}

		/// <summary>
		/// Creates a new Specular exponent chunk.
		/// </summary>
		public SpecularExponentChunk() : base(PolyChunkType.SpecularExponent) { }

		/// <inheritdoc/>
		protected override string GetAsciiBits()
		{
			return $"FEXP_{SpecularExponent:D2}";
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Specular Exponent - {SpecularExponent}";
		}
	}
}
