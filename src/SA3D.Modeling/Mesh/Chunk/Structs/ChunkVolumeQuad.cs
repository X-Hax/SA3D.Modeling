using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.Ascii;
using SA3D.Common.Converters;
using SA3D.Modeling.ObjectData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	/// <summary>
	/// Quad polygon for volume chunks.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct ChunkVolumeQuad : IChunkVolumePolygon
	{
		private class JsonConverter : SimpleJsonObjectConverter<ChunkVolumeQuad>
		{
			private const string _indices = "Indices";
			private const string _attributes = "Attributes";

			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _indices, new(PropertyTokenType.Array, null) },
				{ _attributes, new(PropertyTokenType.Array, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _indices:
						return JsonSerializer.Deserialize<ushort[]>(ref reader, options);
					case _attributes:
						string[] attributes = JsonSerializer.Deserialize<string[]>(ref reader, options)!;

						ushort[] result = new ushort[3];
						for(int i = 0; i < attributes.Length && i < 3; i++)
						{
							result[i] = UInt16HexConverter.ConvertFrom(attributes[i], $"{propertyName}[{i}]");
						}

						return result;
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override ChunkVolumeQuad Create(ReadOnlyDictionary<string, object?> values)
			{
				ushort[] indices = (ushort[]?)values[_indices]
					?? throw new InvalidDataException("Chunk volume quad requires indices!");

				if(indices.Length < 4)
				{
					throw new InvalidDataException("Chunk volume quad requires 4 indices!");
				}

				ChunkVolumeQuad result = new(indices[0], indices[1], indices[2], indices[3]);

				if(values[_attributes] is ushort[] attributes)
				{
					result.Attribute1 = attributes[0];
					result.Attribute2 = attributes[1];
					result.Attribute3 = attributes[2];
				}

				return result;
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, ChunkVolumeQuad value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_indices);
				JsonSerializer.Serialize(writer, new ushort[] { value.Index1, value.Index2, value.Index3, value.Index4 }, options);

				if(value.Attribute1 != 0 || value.Attribute2 != 0 || value.Attribute3 != 0)
				{
					writer.WritePropertyName(_attributes);
					ushort[] attributes = [value.Attribute1, value.Attribute2, value.Attribute3];
					JsonSerializer.Serialize(writer, attributes.Select(x => UInt16HexConverter.ConvertTo(x)), options);
				}
			}
		}

		/// <inheritdoc/>
		public readonly int NumIndices => 4;


		/// <summary>
		/// First vertex index.
		/// </summary>
		public ushort Index1 { get; set; }

		/// <summary>
		/// Second vertex index.
		/// </summary>
		public ushort Index2 { get; set; }

		/// <summary>
		/// Third vertex index.
		/// </summary>
		public ushort Index3 { get; set; }

		/// <summary>
		/// Fourth vertex index.
		/// </summary>
		public ushort Index4 { get; set; }


		/// <summary>
		/// First polygon attribute.
		/// </summary>
		public ushort Attribute1 { get; set; }

		/// <summary>
		/// Second polygon attribute.
		/// </summary>
		public ushort Attribute2 { get; set; }

		/// <summary>
		/// Third polygon attribute.
		/// </summary>
		public ushort Attribute3 { get; set; }


		/// <inheritdoc/>
		public ushort this[int index]
		{
			readonly get => index switch
			{
				0 => Index1,
				1 => Index2,
				2 => Index3,
				4 => Index4,
				_ => throw new IndexOutOfRangeException(),
			};
			set
			{
				switch(index)
				{
					case 0:
						Index1 = value;
						break;
					case 1:
						Index2 = value;
						break;
					case 2:
						Index3 = value;
						break;
					case 3:
						Index4 = value;
						break;
					default:
						throw new IndexOutOfRangeException();
				}
			}
		}


		/// <summary>
		/// Creates a new chunk volume quad.
		/// </summary>
		/// <param name="index1">First vertex index.</param>
		/// <param name="index2">Second vertex index.</param>
		/// <param name="index3">Third vertex index.</param>
		/// <param name="index4">Third vertex index.</param>
		/// <param name="attribute1">First polygon attribute.</param>
		/// <param name="attribute2">Second polygon attribute.</param>
		/// <param name="attribute3">Third polygon attribute.</param>
		public ChunkVolumeQuad(ushort index1, ushort index2, ushort index3, ushort index4, ushort attribute1, ushort attribute2, ushort attribute3)
		{
			Index1 = index1;
			Index2 = index2;
			Index3 = index3;
			Index4 = index4;
			Attribute1 = attribute1;
			Attribute2 = attribute2;
			Attribute3 = attribute3;
		}

		/// <summary>
		/// Creates a new chunk volume quad.
		/// </summary>
		/// <param name="index1">First vertex index.</param>
		/// <param name="index2">Second vertex index.</param>
		/// <param name="index3">Third vertex index.</param>
		/// <param name="index4">Third vertex index.</param>
		public ChunkVolumeQuad(ushort index1, ushort index2, ushort index3, ushort index4) : this()
		{
			Index1 = index1;
			Index2 = index2;
			Index3 = index3;
			Index4 = index4;
		}


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, int polygonAttributeCount)
		{
			Index1 = reader.ReadUInt16();
			Index2 = reader.ReadUInt16();
			Index3 = reader.ReadUInt16();
			Index4 = reader.ReadUInt16();

			if(polygonAttributeCount > 0)
			{
				Attribute1 = reader.ReadUInt16();

				if(polygonAttributeCount > 1)
				{
					Attribute2 = reader.ReadUInt16();

					if(polygonAttributeCount > 2)
					{
						Attribute3 = reader.ReadUInt16();
					}
				}
			}
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer, int polygonAttributeCount)
		{
			writer.WriteUInt16(Index1);
			writer.WriteUInt16(Index2);
			writer.WriteUInt16(Index3);
			writer.WriteUInt16(Index4);

			if(polygonAttributeCount > 0)
			{
				writer.WriteUInt16(Attribute1);

				if(polygonAttributeCount > 1)
				{
					writer.WriteUInt16(Attribute2);

					if(polygonAttributeCount > 0)
					{
						writer.WriteUInt16(Attribute3);
					}
				}
			}
		}


		readonly object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a clone of the quad.
		/// </summary>
		/// <returns>The clonsed quad.</returns>
		public readonly ChunkVolumeQuad Clone()
		{
			return this;
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Quad - {{ {Index1}, {Index2}, {Index3}, {Index4} }}";
		}

		/// <inheritdoc/>
		public readonly void Write(AsciiWriter writer, (ModelAsciiContext context, int attributeCount) context)
		{
			writer.Write($"\t\t{Index1}, {Index2}, {Index3}, {Index4}, ");
			writer.WritePolygonUserflags(context.attributeCount, Attribute1, Attribute2, Attribute3, context.context.BaseContext.PolygonAttributesAsColor);
			writer.WriteLine();
		}
	}
}
