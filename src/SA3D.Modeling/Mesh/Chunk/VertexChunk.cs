using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using J113D.Json;
using SA3D.Common;
using SA3D.Common.Ascii;
using SA3D.Common.Converters;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Chunk.Structs;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Set of vertex data of a chunk model
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public class VertexChunk : ICloneable, IBinarySerializable, IAsciiSerializable<ModelAsciiContext>
	{
		private class JsonConverter : SimpleJsonObjectConverter<VertexChunk>
		{
			private const string _type = nameof(Type);
			private const string _attributes = nameof(Attributes);
			private const string _indexOffset = nameof(IndexOffset);
			private const string _vertices = nameof(Vertices);

			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _type, new(PropertyTokenType.String, null) },
				{ _attributes, new(PropertyTokenType.String, (byte)0) },
				{ _indexOffset, new(PropertyTokenType.Number, (ushort)0u) },
				{ _vertices, new(PropertyTokenType.Array, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _type:
						return JsonSerializer.Deserialize<VertexChunkType>(ref reader, options);
					case _attributes:
						return UInt8HexConverter.ConvertFrom(reader.GetString()!, _attributes);
					case _indexOffset:
						return reader.GetUInt16();
					case _vertices:
						return JsonSerializer.Deserialize<ChunkVertex[]>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override VertexChunk Create(ReadOnlyDictionary<string, object?> values)
			{
				VertexChunkType type = (VertexChunkType?)values[_type]
					?? throw new InvalidDataException($"Vertex chunk requires \"{_type}\" property.");

				ChunkVertex[] vertices = (ChunkVertex[]?)values[_vertices]
					?? throw new InvalidDataException($"Vertex chunk requires \"{_vertices}\" property.");

				return new()
				{
					Type = type,
					Attributes = (byte)values[_attributes]!,
					IndexOffset = (ushort)values[_indexOffset]!,
					Vertices = vertices
				};
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, VertexChunk value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_type);
				JsonSerializer.Serialize(writer, value.Type, options);

				if(value.Attributes != 0)
				{
					writer.WriteString(_attributes, value.Attributes.ToString("X", CultureInfo.InvariantCulture));
				}

				if(value.IndexOffset != 0)
				{
					writer.WriteNumber(_indexOffset, value.IndexOffset);
				}

				writer.WritePropertyName(_vertices);
				JsonSerializer.Serialize(writer, value.Vertices, options);
			}
		}

		/// <summary>
		/// Type of vertex chunk.
		/// </summary>
		public VertexChunkType Type
		{
			get;
			set
			{
				if(!Enum.IsDefined(value) || value is VertexChunkType.End or VertexChunkType.Null)
				{
					throw new ArgumentException($"Vertex chunk type is invalid: {value}", nameof(Type));
				}

				field = value;
			}
		}

		/// <summary>
		/// Various attributes.
		/// </summary>
		public byte Attributes { get; set; }

		/// <summary>
		/// Determines how vertices are applied to the vertex cache.
		/// </summary>
		public WeightMode WeightMode
		{
			get => (WeightMode)(Attributes & 3);
			set => Attributes = (byte)((Attributes & ~0x3) | (byte)value);
		}

		/// <summary>
		/// Compact shape motions
		/// </summary>
		public bool CompactShape
		{
			get => (Attributes & 0x40) != 0;
			set => Attributes = (byte)((Attributes & ~0x40) | (value ? 0x40 : 0));
		}

		/// <summary>
		/// Indicates that the chunks vertex data is not yet finished.
		/// <br/>
		/// <br/> In a complete implementation of the Ninja SDK, enabling this prevents checking all vertices affected by a vertex chunk against clip space. 
		/// <br/>If all vertices of a vertex chunk are outside clip space, then the attaches polygon chunks will be ignored.
		/// <br/>
		/// <br/>(Implemented in Dreamcast, but not in any ports)
		/// </summary>
		public bool VertexCalculationContinue
		{
			get => (Attributes & 0x80) != 0;
			set => Attributes = (byte)((Attributes & ~0x80) | (value ? 0x80 : 0));
		}



		/// <summary>
		/// Index offset value to be added when moving vertices to the global vertex cache
		/// </summary>
		public ushort IndexOffset { get; set; }

		/// <summary>
		/// Vertex data of the chunk
		/// </summary>
		public ChunkVertex[] Vertices { get; set; }


		/// <summary>
		/// Creates a new, empty Vertex chunk with the <see cref="VertexChunkType.Blank"/> type.
		/// </summary>
		public VertexChunk()
		{
			Type = VertexChunkType.Blank;
			Vertices = [];
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			uint header1 = reader.ReadUInt32();
			Attributes = (byte)((header1 >> 8) & 0xFF);
			Type = (VertexChunkType)(header1 & 0xFF);

			uint header2 = reader.ReadUInt32();
			IndexOffset = (ushort)(header2 & 0xFFFF);
			ushort vertexCount = (ushort)(header2 >> 16);

			Vertices = reader.ReadObjectArray(ChunkVertex.GetReadCallback(Type), vertexCount);
		}

		internal static LabeledArray<VertexChunk> ReadArray(BinaryObjectReader reader)
		{
			VertexChunkType peekType()
			{
				using SeekToken token = reader.At();
				return (VertexChunkType)(reader.ReadUInt32() & 0xFF);
			}

			List<VertexChunk> chunks = [];
			while(peekType() != VertexChunkType.End)
			{
				chunks.Add(reader.ReadObject<VertexChunk>());
			}

			reader.Skip(sizeof(int) * 2);

			return new([.. chunks]);
		}

		private void SplitWrite(Action<ushort, ushort, ushort, ushort> write)
		{
			if(Vertices.Length > short.MaxValue)
			{
				throw new InvalidOperationException($"Vertex count ({Vertices.Length}) exceeds maximum vertex count ({short.MaxValue})");
			}

			int vertSize = Type.GetIntegerSize();
			ushort vertexLimitPerChunk = (ushort)((ushort.MaxValue - 1) / vertSize); // -1 because header2 also counts as part of the size, which is always there

			ushort offset = 0;

			while(offset < Vertices.Length)
			{
				ushort vertCount = ushort.Min((ushort)(Vertices.Length - offset), vertexLimitPerChunk);
				ushort size = (ushort)((vertCount * vertSize) + 1);
				ushort indexOffset = (ushort)(IndexOffset + (Type.CheckHasAttributes() ? 0 : offset));

				write(size, indexOffset, vertCount, offset);
				offset += vertCount;
			}
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer)
		{
			uint header1Base = (uint)Type | (uint)(Attributes << 8);
			Action<BinaryObjectWriter, ChunkVertex> vertexWrite = ChunkVertex.GetWriteCallback(Type);

			SplitWrite((size, indexOffset, vertCount, offset) =>
			{
				writer.WriteUInt32(header1Base | (uint)(size << 16));
				writer.WriteUInt32((uint)(indexOffset | (vertCount << 16)));
				writer.WriteObjectArray(vertexWrite, Vertices.Skip(offset).Take(vertCount));
			});
		}

		internal static void WriteArray(BinaryObjectWriter writer, IEnumerable<VertexChunk> chunks)
		{
			writer.WriteObjectArray(chunks);

			// End chunk
			writer.WriteUInt32((uint)VertexChunkType.End);
			writer.WriteUInt32(0);
		}


		/// <inheritdoc/>
		public void Write(AsciiWriter writer, ModelAsciiContext context)
		{
			string chunkType = AsciiMaps.VertexChunkTypeMap.FindKey(Type);
			string chunkFlags = string.Empty;

			if(VertexCalculationContinue)
			{
				chunkFlags += "|FV_CONT";
			}

			if(CompactShape)
			{
				chunkFlags += "|FV_SHAPE";
			}

			if(Type.CheckHasAttributes())
			{
				chunkFlags += '|' + AsciiMaps.WeightModeMap.FindKey(WeightMode);
			}

			chunkFlags = chunkFlags == string.Empty ? "0x0" : chunkFlags[1..];

			Action<AsciiWriter, ChunkVertex> vertexWrite = ChunkVertex.GetAsciiWriteCallback(Type, context);

			SplitWrite((size, indexOffset, vertCount, offset) =>
			{
				writer.WriteLine($"\t{chunkType}({chunkFlags}, {size})");
				writer.WriteLine($"\tOffnbIdx({indexOffset}, {vertCount})");

				foreach(ChunkVertex vertex in Vertices.Skip(offset).Take(vertCount))
				{
					vertexWrite(writer, vertex);
				}
			});
		}

		internal static void WriteArray(AsciiWriter writer, LabeledArray<VertexChunk>? chunks, ModelAsciiContext context)
		{
			if(chunks == null)
			{
				return;
			}

			using(AsciiWriterBlockToken? block = writer.WriteStructBlockWithReference("VLIST", chunks))
			{
				if(block == null)
				{
					return;
				}

				foreach(VertexChunk chunk in chunks)
				{
					chunk.Write(writer, context);
				}

				writer.WriteLine("\tCnkEnd()");
			}
		}


		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the vertex chunk.
		/// </summary>
		/// <returns></returns>
		public VertexChunk Clone()
		{
			return new()
			{
				Type = Type,
				Attributes = Attributes,
				IndexOffset = IndexOffset,
				Vertices = (ChunkVertex[])Vertices.Clone()
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Type}, {WeightMode}, {IndexOffset} : [{Vertices.Length}]";
		}

	}
}

