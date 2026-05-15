using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Chunk.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Set of vertex data of a chunk model
	/// </summary>
	public class VertexChunk : ICloneable, IBinarySerializable
	{
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
		/// Indicates that the chunks vertex data is not yet finished.
		/// <br/>
		/// <br/> In a complete implementation of the Ninja SDK, enabling this prevents checking all vertices affected by a vertex chunk against clip space. 
		/// <br/>If all vertices of a vertex chunk are outside clip space, then the attaches polygon chunks will be ignored.
		/// <br/>
		/// <br/>(Implemented in Dreamcast, but not in any ports)
		/// </summary>
		public bool VertexCalculationContinue
		{
			get => (Attributes & 0x40) != 0;
			set => Attributes = (byte)((Attributes & ~0x40) | (value ? 0x40 : 0));
		}

		/// <summary>
		/// Ninja2: compact shape motions
		/// </summary>
		public bool CompactShape
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

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer)
		{
			if(Vertices.Length > short.MaxValue)
			{
				throw new InvalidOperationException($"Vertex count ({Vertices.Length}) exceeds maximum vertex count ({short.MaxValue})");
			}

			int vertSize = Type.GetIntegerSize();
			ushort vertexLimitPerChunk = (ushort)((ushort.MaxValue - 1) / vertSize); // -1 because header2 also counts as part of the size, which is always there

			uint header1Base = (uint)Type | (uint)(Attributes << 8);
			ushort offset = 0;

			Action<BinaryObjectWriter, ChunkVertex> vertexWrite = ChunkVertex.GetWriteCallback(Type);

			while(offset < Vertices.Length)
			{
				ushort vertCount = ushort.Min((ushort)(Vertices.Length - offset), vertexLimitPerChunk);
				ushort size = (ushort)((vertCount * vertSize) + 1);
				ushort indexOffset = (ushort)(IndexOffset + (Type.CheckHasWeights() ? 0 : offset));

				writer.WriteUInt32(header1Base | (uint)(size << 16));
				writer.WriteUInt32((uint)(indexOffset | (vertCount << 16)));
				writer.WriteObjectArray(vertexWrite, Vertices.Skip(offset).Take(vertCount));
				offset += vertCount;
			}
		}


		internal static void WriteArray(BinaryObjectWriter writer, IEnumerable<VertexChunk> chunks)
		{
			writer.WriteObjectArray(chunks);

			// End chunk
			writer.WriteUInt32((uint)VertexChunkType.End);
			writer.WriteUInt32(0);
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

