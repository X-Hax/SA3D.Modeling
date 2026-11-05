using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Common;
using SA3D.Common.Ascii;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.ObjectData.Structs;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Vertex chunk array
	/// </summary>
	public class VertexChunkArray : LabeledArray<VertexChunk>, IBinarySerializable<IOContext>, IAsciiSerializable<ModelAsciiIOContext>
	{
		private const string _labelPrefix = "vertex_";

		/// <inheritdoc/>
		public override string LabelPrefix => _labelPrefix;


		/// <summary>
		/// Creates a new, empty vertex chunk array
		/// </summary>
		public VertexChunkArray() : base(_labelPrefix.GenerateIdentifier()) { }

		/// <summary>
		/// Creates a new, empty vertex chunk array
		/// </summary>
		public VertexChunkArray(int size) : base(_labelPrefix.GenerateIdentifier(), size) { }

		/// <summary>
		/// Creates a new vertex chunk array with preexisting chunks
		/// </summary>
		public VertexChunkArray(IEnumerable<VertexChunk> chunks) : base(_labelPrefix.GenerateIdentifier(), [.. chunks]) { }


		void IBinarySerializable<IOContext>.Read(BinaryObjectReader reader, IOContext context)
		{
			VertexChunkType peekType()
			{
				using SeekToken token = reader.At();
				return (VertexChunkType)(reader.ReadUInt32() & 0xFF);
			}

			List<VertexChunk> chunks = [];
			while(peekType() != VertexChunkType.End)
			{
				chunks.Add(reader.ReadObject<VertexChunk, IOContext>(context));
			}

			reader.Skip(sizeof(int) * 2);

			Array = [.. chunks];
		}

		void IBinarySerializable<IOContext>.Write(BinaryObjectWriter writer, IOContext context)
		{
			writer.WriteObjectArray(this, context);

			// End chunk
			writer.WriteUInt32((uint)VertexChunkType.End);
			writer.WriteUInt32(0);
		}

		void IAsciiSerializable<ModelAsciiIOContext>.Write(AsciiWriter writer, ModelAsciiIOContext context)
		{
			using AsciiWriterBlockToken? block = writer.WriteStructBlock("VLIST", this);

			if(block == null)
			{
				return;
			}

			foreach(VertexChunk chunk in this)
			{
				writer.WriteObject(chunk, context);
			}

			writer.WriteLine("\tCnkEnd()");
		}
	}
}
