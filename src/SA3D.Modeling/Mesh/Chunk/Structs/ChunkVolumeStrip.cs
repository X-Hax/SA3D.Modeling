using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.Ascii;
using SA3D.Common.Converters;
using SA3D.Modeling.ObjectData;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	/// <summary>
	/// Triangle strip polygon for volume chunks.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct ChunkVolumeStrip : IChunkVolumePolygon
	{
		private class JsonConverter : SimpleJsonObjectConverter<ChunkVolumeStrip>
		{
			private const string _reversed = nameof(Reversed);
			private const string _indices = nameof(Indices);
			private const string _triangleAttributes = nameof(TriangleAttributes);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _reversed, new(PropertyTokenType.Bool, false) },
				{ _indices, new(PropertyTokenType.Array, null) },
				{ _triangleAttributes, new(PropertyTokenType.Array, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _reversed:
						return reader.GetBoolean();
					case _indices:
						return JsonSerializer.Deserialize<ushort[]>(ref reader, options);
					case _triangleAttributes:
						return JsonSerializer.Deserialize<string[][]>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override ChunkVolumeStrip Create(ReadOnlyDictionary<string, object?> values)
			{
				ushort[] indices = (ushort[]?)values[_indices]
					?? throw new InvalidDataException("Chunk volume strip requires indices!");

				ChunkVolumeStrip result = new(indices, (bool)values[_reversed]!);

				if(values[_triangleAttributes] is string[][] attributes)
				{
					for(int i = 0; i < attributes.Length && i < result.TriangleAttributes.Length; i++)
					{
						string[] attributeArray = attributes[i];
						for(int j = 0; j < attributeArray.Length && j < 3; j++)
						{
							result.TriangleAttributes[i, j] = UInt16HexConverter.ConvertFrom(attributeArray[j], $"{_triangleAttributes}[{i}][{j}]");
						}
					}
				}

				return result;
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, ChunkVolumeStrip value, JsonSerializerOptions options)
			{
				if(value.Reversed)
				{
					writer.WriteBoolean(_reversed, value.Reversed);
				}

				writer.WritePropertyName(_indices);
				JsonSerializer.Serialize(writer, value.Indices, options);

				bool hasAttributes = false;
				for(int i = 0; i < value.TriangleAttributes.Length; i++)
				{
					for(int j = 0; j < 3; j++)
					{
						if(value.TriangleAttributes[i, j] != 0)
						{
							hasAttributes = true;
							break;
						}
					}

					if(hasAttributes)
					{
						break;
					}
				}

				if(hasAttributes)
				{
					string[][] attributes = new string[value.TriangleAttributes.Length][];

					for(int i = 0; i < value.TriangleAttributes.Length; i++)
					{
						string[] attributeArray = new string[3];
						for(int j = 0; j < 3; j++)
						{
							attributeArray[j] = UInt16HexConverter.ConvertTo(value.TriangleAttributes[i, j]);
						}
					}

					writer.WritePropertyName(_triangleAttributes);
					JsonSerializer.Serialize(writer, attributes, options);
				}
			}
		}

		/// <summary>
		/// Vertex indices.
		/// </summary>
		public ushort[] Indices { get; set; }

		/// <inheritdoc/>
		public readonly int NumIndices => Indices.Length;

		/// <summary>
		/// Triangle attributes for each triangle. [triangle index, attribute index]
		/// </summary>
		public ushort[,] TriangleAttributes { get; set; }

		/// <summary>
		/// Whether the triangles use reversed culling direction.
		/// </summary>
		public bool Reversed { get; set; }

		/// <inheritdoc/>
		public readonly ushort this[int index]
		{
			get => Indices[index];
			set => Indices[index] = value;
		}

		private ChunkVolumeStrip(ushort[] indices, ushort[,] triangleAttributes, bool reversed)
		{
			Indices = indices;
			TriangleAttributes = triangleAttributes;
			Reversed = reversed;
		}

		/// <summary>
		/// Creates a new empty chunk volume strip.
		/// </summary>
		/// <param name="size">Number of vertex indices.</param>
		/// <param name="reversed">Whether the triangles use reversed culling direction.</param>
		public ChunkVolumeStrip(int size, bool reversed)
		{
			Indices = new ushort[size];
			TriangleAttributes = new ushort[size - 2, 3];
			Reversed = reversed;
		}

		/// <summary>
		/// Creates a new empty chunk volume strip.
		/// </summary>
		/// <param name="indices">Vertex indices to use.</param>
		/// <param name="reversed">Whether the triangles use reversed culling direction.</param>
		public ChunkVolumeStrip(ushort[] indices, bool reversed)
		{
			Indices = indices;
			TriangleAttributes = new ushort[Indices.Length - 2, 3];
			Reversed = reversed;
		}


		/// <summary>
		/// Verifies this strips polygon data
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		public readonly void VerifyPolygonData()
		{
			if(Indices.Length < 3)
			{
				throw new InvalidOperationException("Volume strips require at least 3 indices!");
			}

			if(TriangleAttributes.Length != Indices.Length - 2)
			{
				throw new InvalidOperationException("Triangle attributes on volume strips are required to have 2 less than the length of the same polygons indices!");
			}

			if(TriangleAttributes.GetLength(1) != 3)
			{
				throw new InvalidOperationException("Triangle attributes on volume strips need to be 3 deep!");
			}
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, int polygonAttributeCount)
		{
			short header = reader.ReadInt16();
			Reversed = header < 0;
			Indices = new ushort[Math.Abs(header)];
			TriangleAttributes = new ushort[Indices.Length - 2, 3];

			Indices[0] = reader.ReadUInt16();
			Indices[1] = reader.ReadUInt16();

			for(int i = 2; i < Indices.Length; i++)
			{
				Indices[i] = reader.ReadUInt16();

				for(int j = 0; j < polygonAttributeCount; j++)
				{
					TriangleAttributes[i - 2, j] = reader.ReadUInt16();
				}
			}
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer, int polygonAttributeCount)
		{
			VerifyPolygonData();

			short count = (short)Math.Min(Indices.Length, short.MaxValue);
			writer.WriteInt16(Reversed ? (short)-count : count);

			writer.WriteUInt16(Indices[0]);
			writer.WriteUInt16(Indices[1]);
			for(int i = 2; i < count; i++)
			{
				writer.WriteUInt16(Indices[i]);

				for(int j = 0; j < polygonAttributeCount; j++)
				{
					writer.WriteUInt16(TriangleAttributes[i - 2, j]);
				}
			}
		}

		/// <inheritdoc/>
		public readonly void Write(AsciiWriter writer, (ModelAsciiContext context, int attributeCount) context)
		{
			writer.Write($"\tStrip{(Reversed ? 'R' : 'L')}({Indices.Length}), ");

			if(context.attributeCount == 0)
			{
				if(Indices.Length > 10)
				{
					writer.WriteLine();
					writer.Write("\t\t");
				}

				for(int i = 0; i < Indices.Length; i++)
				{
					writer.Write($"{Indices[i]}, ");

					if(i > 0 && i % 10 == 0)
					{
						writer.WriteLine();
						writer.Write("\t\t");
					}
				}

				writer.WriteLine();
			}
			else
			{

				for(int i = 0; i < Indices.Length; i++)
				{
					writer.Write($"\t{Indices[i]},");

					if(i > 1)
					{
						int triangleIndex = i - 2;
						writer.WritePolygonUserflags(context.attributeCount, TriangleAttributes[triangleIndex, 0], TriangleAttributes[triangleIndex, 1], TriangleAttributes[triangleIndex, 2], context.context.BaseContext.PolygonAttributesAsColor);
					}

					writer.WriteLine();
				}
			}
		}


		readonly object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Returns a deep clone of the chunk volume strip.
		/// </summary>
		/// <returns>The cloned strip.</returns>
		public readonly ChunkVolumeStrip Clone()
		{
			return new(
				(ushort[])Indices.Clone(),
				(ushort[,])TriangleAttributes.Clone(),
				Reversed);
		}
	}
}
