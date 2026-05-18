using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Ginja.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Ginja
{
	/// <summary>
	/// A collection of corners forming polygons
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct GinjaPolygon : ICloneable, IBinarySerializable<GinjaIndexFormat>
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


		internal static int[] GetIndexSizes(GinjaIndexFormat format)
		{
			int[] result = new int[13];
			uint attributes = (uint)format;
			for(uint i = 0, b = 1; b <= 0x2000; i++, b <<= 2)
			{
				if((attributes & (b << 1)) == 0)
				{
					continue;
				}

				result[i] = (attributes & b) == 0 ? 1 : 2;
			}

			return result;
		}

		/// <inheritdoc/>
		public unsafe void Read(BinaryObjectReader reader, GinjaIndexFormat format)
		{
			using EndiannessToken endianness = reader.WithEndian(Endianness.Big);

			Type = (GinjaPolyType)reader.ReadByte();
			Corners = new GinjaCorner[reader.ReadUInt16()];

			int[] indexSize = GetIndexSizes(format);

			fixed(GinjaCorner* corner = &Corners[0])
			{
				ushort* current = (ushort*)corner;
				for(int i = 0; i < Corners.Length; i++)
				{
					foreach(int number in indexSize)
					{
						if(number == 1)
						{
							*current = reader.ReadByte();
						}
						else if(number == 2)
						{
							*current = reader.ReadUInt16();
						}

						current++;
					}
				}
			}
		}

		internal static LabeledArray<GinjaPolygon> ReadArray(BinaryObjectReader reader, int size, GinjaIndexFormat indexFormat)
		{
			List<GinjaPolygon> polygons = [];
			long end_pos = reader.Position + size;

			while(reader.Position < end_pos)
			{
				using(reader.At())
				{
					if(reader.ReadByte() == 0)
					{
						break;
					}
				}

				polygons.Add(reader.ReadObject<GinjaPolygon, GinjaIndexFormat>(indexFormat));
			}

			return new([.. polygons]);
		}

		/// <inheritdoc/>
		public readonly unsafe void Write(BinaryObjectWriter writer, GinjaIndexFormat format)
		{
			using EndiannessToken endianness = writer.WithEndian(Endianness.Big);

			writer.WriteByte((byte)Type);
			writer.WriteUInt16((ushort)Corners.Length);

			int[] indexSize = GetIndexSizes(format);

			fixed(GinjaCorner* corner = &Corners[0])
			{
				ushort* current = (ushort*)corner;
				for(int i = 0; i < Corners.Length; i++)
				{
					foreach(int number in indexSize)
					{
						if(number == 1)
						{
							writer.WriteByte((byte)*current);
						}
						else if(number == 2)
						{
							writer.WriteUInt16(*current);
						}

						current++;
					}
				}
			}
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
