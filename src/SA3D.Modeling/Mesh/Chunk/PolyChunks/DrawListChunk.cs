using J113D.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Draws the polygon chunks cached by a specific index.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public class DrawListChunk : BitsChunk
	{
		internal class JsonConverter : ChildJsonObjectConverter<PolyChunkType, DrawListChunk, PolyChunk>
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
				return key == PolyChunkType.DrawList;
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
			protected override DrawListChunk CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					List = (byte)values[_list]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, DrawListChunk value, JsonSerializerOptions options)
			{
				writer.WriteNumber(_list, value.List);
			}
		}

		/// <summary>
		/// Cache ID
		/// </summary>
		public byte List
		{
			get => Attributes;
			set => Attributes = value;
		}

		/// <summary>
		/// Creates a new draw list chunk.
		/// </summary>
		public DrawListChunk() : base(PolyChunkType.DrawList) { }

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Draw List - {List}";
		}
	}
}
