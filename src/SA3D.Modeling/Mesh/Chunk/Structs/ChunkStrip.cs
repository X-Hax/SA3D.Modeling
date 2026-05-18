using J113D.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	/// <summary>
	/// Triangle string structure for strip chunks.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct ChunkStrip : ICloneable
	{
		private class JsonConverter : SimpleJsonObjectConverter<ChunkStrip>
		{
			private const string _reversed = nameof(Reversed);
			private const string _corners = nameof(Corners);

			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _reversed, new(PropertyTokenType.Bool, false) },
				{ _corners, new(PropertyTokenType.Array, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _reversed:
						return reader.GetBoolean();
					case _corners:
						return JsonSerializer.Deserialize<ChunkCorner[]>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override ChunkStrip Create(ReadOnlyDictionary<string, object?> values)
			{
				ChunkCorner[] corners = (ChunkCorner[]?)values[_corners]
					?? throw new InvalidDataException($"Chunk strip requires \"{_corners}\" property!");

				return new(corners, (bool)values[_reversed]!);
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, ChunkStrip value, JsonSerializerOptions options)
			{
				if(value.Reversed)
				{
					writer.WriteBoolean(_reversed, value.Reversed);
				}

				writer.WritePropertyName(_corners);
				JsonSerializer.Serialize(writer, value.Corners, options);
			}
		}

		/// <summary>
		/// Maximum allowed size of a (collection of) strip chunk(s)
		/// </summary>
		public const uint MaxByteSize = (ushort.MaxValue * 2) - 2;

		/// <summary>
		/// Triangle corners. 
		/// <br/> The first two corners are only used for their index.
		/// </summary>
		public ChunkCorner[] Corners { get; private set; }

		/// <summary>
		/// Whether to inverse the culling direction of the triangles.
		/// </summary>
		public bool Reversed { get; private set; }


		/// <summary>
		/// Creates a new strip.
		/// </summary>
		/// <param name="corners">Triangle corners.</param>
		/// <param name="reverse">Whether to inverse the culling direction of the triangles</param>
		public ChunkStrip(ChunkCorner[] corners, bool reverse)
		{
			Reversed = reverse;
			Corners = corners;
		}


		readonly object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the strip.
		/// </summary>
		/// <returns>The cloned strip.</returns>
		public readonly ChunkStrip Clone()
		{
			return new((ChunkCorner[])Corners.Clone(), Reversed);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{Reversed} : {Corners.Length}";
		}
	}
}
