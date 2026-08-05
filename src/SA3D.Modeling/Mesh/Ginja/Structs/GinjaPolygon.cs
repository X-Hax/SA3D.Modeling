using J113D.Json;
using SA3D.Modeling.Mesh.Ginja.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Ginja.Structs
{
	/// <summary>
	/// A collection of corners forming polygons
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct GinjaPolygon : ICloneable
	{
		private class JsonConverter : SimpleJsonObjectConverter<GinjaPolygon>
		{
			private const string _type = nameof(GinjaPolygon.Type);
			private const string _corners = nameof(Corners);

			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _type, new(PropertyTokenType.String, null) },
				{ _corners, new(PropertyTokenType.Array, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _type:
						return JsonSerializer.Deserialize<GinjaPolyType>(ref reader, options);
					case _corners:
						return JsonSerializer.Deserialize<GinjaCorner[]>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override GinjaPolygon Create(ReadOnlyDictionary<string, object?> values)
			{
				GinjaPolyType type = (GinjaPolyType?)values[_type]
					?? throw new InvalidDataException($"GinjaPolygon requires property \"{_type}\"!");

				GinjaCorner[] corners = (GinjaCorner[]?)values[_corners]
					?? throw new InvalidDataException($"GinjaPolygon requires property \"{_corners}\"!");

				return new(type, corners);
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, GinjaPolygon value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_type);
				JsonSerializer.Serialize(writer, value.Type, options);

				writer.WritePropertyName(_corners);
				JsonSerializer.Serialize(writer, value.Corners, options);
			}
		}

		/// <summary>
		/// The way in which polygons are being stored.
		/// </summary>
		public GinjaPolyType Type { get; set; }

		/// <summary>
		/// Corners making up the polygons.
		/// </summary>
		public GinjaCorner[] Corners { get; set; }

		/// <summary>
		/// Create a new empty Primitive
		/// </summary>
		/// <param name="type">The type of primitive.</param>
		/// <param name="corners">Corners making up the polygons.</param>
		public GinjaPolygon(GinjaPolyType type, GinjaCorner[] corners)
		{
			Type = type;
			Corners = corners;
		}


		readonly object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the polygon.
		/// </summary>
		/// <returns>The cloned polygon</returns>
		public readonly GinjaPolygon Clone()
		{
			return new(Type, (GinjaCorner[])Corners.Clone());
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{Type}: {Corners.Length}";
		}
	}
}
