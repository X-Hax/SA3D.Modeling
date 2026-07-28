using J113D.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Adjusts the mipmap distance of the following strip chunks
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class MipmapDistanceMultiplierChunk : BitsChunk
	{
		internal class JsonConverter : ChildJsonObjectConverter<PolyChunkType, MipmapDistanceMultiplierChunk, PolyChunk>
		{
			private const string _mipmapDistanceMultiplier = nameof(MipmapDistanceMultiplier);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<PolyChunkType, PolyChunk> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _mipmapDistanceMultiplier, new(PropertyTokenType.Number, 1f) }
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(PolyChunkType key)
			{
				return key == PolyChunkType.MipmapDistanceMultiplier;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _mipmapDistanceMultiplier:
						return reader.GetSingle();
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override MipmapDistanceMultiplierChunk CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					MipmapDistanceMultiplier = (float)values[_mipmapDistanceMultiplier]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, MipmapDistanceMultiplierChunk value, JsonSerializerOptions options)
			{
				writer.WriteNumber(_mipmapDistanceMultiplier, value.MipmapDistanceMultiplier);
			}
		}

		/// <summary>
		/// The mipmap distance multiplier <br/>
		/// Ranges from 0 to 3.75f in increments of 0.25
		/// </summary>
		public float MipmapDistanceMultiplier
		{
			get => byte.Max(1, (byte)(Attributes & 0xF)) * 0.25f;
			set => Attributes = (byte)((Attributes & 0xF0) | (byte)Math.Max(1, Math.Min(0xF, Math.Round(value / 0.25, MidpointRounding.AwayFromZero))));
		}

		/// <summary>
		/// Creates a new mipmap distance multiplier chunk.
		/// </summary>
		public MipmapDistanceMultiplierChunk() : base(PolyChunkType.MipmapDistanceMultiplier) { }

		/// <inheritdoc/>
		protected override string GetAsciiAttributes()
		{
			if((Attributes & 0xF) == 0)
			{
				return "FDA_100";
			}

			return $"FDA_{(Attributes & 0xF) * 25:D3}";
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"MMDM - {MipmapDistanceMultiplier}";
		}
	}
}
