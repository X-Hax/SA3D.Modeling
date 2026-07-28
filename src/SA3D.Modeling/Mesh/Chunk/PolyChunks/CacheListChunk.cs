using J113D.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Caches the succeeding polygon chunks of the same attach into specified index.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class CacheListChunk : BitsChunk
	{
		internal class JsonConverter : ChildJsonObjectConverter<PolyChunkType, CacheListChunk, PolyChunk>
		{
			private const string _list = nameof(List);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<PolyChunkType, PolyChunk> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _list, new(PropertyTokenType.Number, (byte)0) }
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(PolyChunkType key)
			{
				return key == PolyChunkType.CacheList;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _list:
						return reader.GetByte();
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override CacheListChunk CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					List = (byte)values[_list]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, CacheListChunk value, JsonSerializerOptions options)
			{
				writer.WriteNumber(_list, value.List);
			}
		}

		/// <summary>
		/// Cache ID.
		/// </summary>
		public byte List
		{
			get => Attributes;
			set => Attributes = value;
		}

		/// <summary>
		/// Creates a new cache list chunk.
		/// </summary>
		public CacheListChunk() : base(PolyChunkType.CacheList) { }

		/// <inheritdoc/>
		protected override string GetAsciiAttributes()
		{
			return List.ToString();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Cache list - {List}";
		}
	}
}
