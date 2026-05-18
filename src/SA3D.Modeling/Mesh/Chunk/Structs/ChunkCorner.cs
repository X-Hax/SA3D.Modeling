using J113D.Json;
using SA3D.Common.Converters;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	/// <summary>
	/// A single polygon corner for chunk models.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct ChunkCorner : IEquatable<ChunkCorner>
	{
		private class JsonConverter : SimpleJsonObjectConverter<ChunkCorner>
		{
			private const string _index = nameof(Index);
			private const string _texcoord = nameof(Texcoord);
			private const string _texcoord2 = nameof(Texcoord2);
			private const string _normal = nameof(Normal);
			private const string _color = nameof(Color);
			private const string _attributes1 = nameof(Attributes1);
			private const string _attributes2 = nameof(Attributes2);
			private const string _attributes3 = nameof(Attributes3);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>() {
				{ _index, new(PropertyTokenType.Number, DefaultValues.Index) },
				{ _texcoord, new(PropertyTokenType.String, DefaultValues.Texcoord) },
				{ _texcoord2, new(PropertyTokenType.String, DefaultValues.Texcoord2) },
				{ _normal, new(PropertyTokenType.String, DefaultValues.Normal ) },
				{ _color, new(PropertyTokenType.String, DefaultValues.Color) },
				{ _attributes1, new(PropertyTokenType.String, DefaultValues.Attributes1) },
				{ _attributes2, new(PropertyTokenType.String, DefaultValues.Attributes2) },
				{ _attributes3, new(PropertyTokenType.String, DefaultValues.Attributes3) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _index:
						return reader.GetUInt16();
					case _texcoord:
					case _texcoord2:
						return JsonSerializer.Deserialize<Vector2>(ref reader, options);
					case _normal:
						return JsonSerializer.Deserialize<Vector3>(ref reader, options);
					case _color:
						return JsonSerializer.Deserialize<Color>(ref reader, options);
					case _attributes1:
					case _attributes2:
					case _attributes3:
						return UInt16HexConverter.ConvertFrom(reader.GetString()!, propertyName);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override ChunkCorner Create(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					Index = (ushort)values[_index]!,
					Texcoord = (Vector2)values[_texcoord]!,
					Texcoord2 = (Vector2)values[_texcoord2]!,
					Normal = (Vector3)values[_normal]!,
					Color = (Color)values[_color]!,
					Attributes1 = (ushort)values[_attributes1]!,
					Attributes2 = (ushort)values[_attributes2]!,
					Attributes3 = (ushort)values[_attributes3]!,
				};
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, ChunkCorner value, JsonSerializerOptions options)
			{
				writer.WriteNumber(_index, value.Index);

				if(value.Texcoord != DefaultValues.Texcoord)
				{
					writer.WritePropertyName(_texcoord);
					JsonSerializer.Serialize(writer, value.Texcoord, options);
				}

				if(value.Texcoord2 != DefaultValues.Texcoord2)
				{
					writer.WritePropertyName(_texcoord2);
					JsonSerializer.Serialize(writer, value.Texcoord2, options);
				}

				if(value.Normal != DefaultValues.Normal)
				{
					writer.WritePropertyName(_normal);
					JsonSerializer.Serialize(writer, value.Normal, options);
				}

				if(value.Color != DefaultValues.Color)
				{
					writer.WritePropertyName(_color);
					JsonSerializer.Serialize(writer, value.Color, options);
				}

				if(value.Attributes1 != DefaultValues.Attributes1)
				{
					writer.WriteString(_attributes1, UInt32HexConverter.ConvertTo(value.Attributes1));
				}

				if(value.Attributes2 != DefaultValues.Attributes2)
				{
					writer.WriteString(_attributes2, UInt32HexConverter.ConvertTo(value.Attributes2));
				}

				if(value.Attributes3 != DefaultValues.Attributes3)
				{
					writer.WriteString(_attributes3, UInt32HexConverter.ConvertTo(value.Attributes3));
				}
			}
		}

		/// <summary>
		/// Chunk corner with default values
		/// </summary>
		public static readonly ChunkCorner DefaultValues = new()
		{
			Normal = Vector3.UnitY,
			Color = Color.ColorWhite
		};

		/// <summary>
		/// Vertex Cache index.
		/// </summary>
		public ushort Index { get; set; }

		/// <summary>
		/// Texture coordinates.
		/// </summary>
		public Vector2 Texcoord { get; set; }

		/// <summary>
		/// Second set of texture coordinates.
		/// </summary>
		public Vector2 Texcoord2 { get; set; }

		/// <summary>
		/// Normalized direction.
		/// </summary>
		public Vector3 Normal { get; set; }

		/// <summary>
		/// Color.
		/// </summary>
		public Color Color { get; set; }

		/// <summary>
		/// First set of attributes for the triangle that this corner closes.
		/// </summary>
		public ushort Attributes1 { get; set; }

		/// <summary>
		/// Second set of attributes for the triangle that this corner closes.
		/// </summary>
		public ushort Attributes2 { get; set; }

		/// <summary>
		/// Third set of attributes for the triangle that this corner closes.
		/// </summary>
		public ushort Attributes3 { get; set; }


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is ChunkCorner corner &&
				   Index == corner.Index &&
				   Texcoord.Equals(corner.Texcoord) &&
				   Normal.Equals(corner.Normal) &&
				   Color.Equals(corner.Color) &&
				   Attributes1 == corner.Attributes1 &&
				   Attributes2 == corner.Attributes2 &&
				   Attributes3 == corner.Attributes3;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(Index, Texcoord, Normal, Color, Attributes1, Attributes2, Attributes3);
		}

		readonly bool IEquatable<ChunkCorner>.Equals(ChunkCorner other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two chunk corners for equality.
		/// </summary>
		/// <param name="left">Lefthand corner.</param>
		/// <param name="right">Righthand corner.</param>
		/// <returns>Wether the corners are equal.</returns>
		public static bool operator ==(ChunkCorner left, ChunkCorner right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two chunk corners for inequality.
		/// </summary>
		/// <param name="left">Lefthand corner.</param>
		/// <param name="right">Righthand corner.</param>
		/// <returns>Wether the corners are inequal.</returns>
		public static bool operator !=(ChunkCorner left, ChunkCorner right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{Index} : {Texcoord.DebugString()}, {Color}";
		}
	}
}
