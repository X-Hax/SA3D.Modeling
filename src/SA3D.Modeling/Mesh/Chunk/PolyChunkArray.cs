using Amicitia.IO;
using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Common;
using SA3D.Common.Ascii;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Chunk.PolyChunks;
using SA3D.Modeling.ObjectData.Structs;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;

namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Polychunk array
	/// </summary>
	public class PolyChunkArray : LabeledArray<PolyChunk>, IBinarySerializable<IOContext>, IAsciiSerializable<ModelAsciiIOContext>
	{
		private const string _labelPrefix = "poly_";

		/// <inheritdoc/>
		public override string LabelPrefix => _labelPrefix;


		/// <summary>
		/// Creates a new, empty poly chunk array
		/// </summary>
		public PolyChunkArray() : base(_labelPrefix.GenerateIdentifier()) { }

		/// <summary>
		/// Creates a new, empty poly chunk array
		/// </summary>
		public PolyChunkArray(int size) : base(_labelPrefix.GenerateIdentifier(), size) { }

		/// <summary>
		/// Creates a new poly chunk array with preexisting chunks
		/// </summary>
		public PolyChunkArray(IEnumerable<PolyChunk> chunks) : base(_labelPrefix.GenerateIdentifier(), [.. chunks]) { }


		void IBinarySerializable<IOContext>.Read(BinaryObjectReader reader, IOContext context)
		{
			PolyChunkType peekType()
			{
				using SeekToken token = reader.At();
				return (PolyChunkType)(reader.ReadUInt16() & 0xFF);
			}

			List<PolyChunk> chunks = [];

			while(true)
			{
				PolyChunk chunk;
				long offset = reader.GetPositionOffset();
				switch(peekType())
				{
					case PolyChunkType.BlendAlpha:
						chunk = reader.ReadObject<BlendAlphaChunk>();
						break;
					case PolyChunkType.MipmapDistanceMultiplier:
						chunk = reader.ReadObject<MipmapDistanceMultiplierChunk>();
						break;
					case PolyChunkType.SpecularExponent:
						chunk = reader.ReadObject<SpecularExponentChunk>();
						break;
					case PolyChunkType.CacheList:
						chunk = reader.ReadObject<CacheListChunk>();
						break;
					case PolyChunkType.DrawList:
						chunk = reader.ReadObject<DrawListChunk>();
						break;
					case PolyChunkType.TextureID:
					case PolyChunkType.TextureID2:
						chunk = reader.ReadObject<TextureChunk>();
						break;
					case PolyChunkType.Material_Empty:
					case PolyChunkType.Material_Diffuse:
					case PolyChunkType.Material_Ambient:
					case PolyChunkType.Material_DiffuseAmbient:
					case PolyChunkType.Material_Specular:
					case PolyChunkType.Material_DiffuseSpecular:
					case PolyChunkType.Material_AmbientSpecular:
					case PolyChunkType.Material_DiffuseAmbientSpecular:
					case PolyChunkType.Material_Diffuse2:
					case PolyChunkType.Material_Ambient2:
					case PolyChunkType.Material_DiffuseAmbient2:
					case PolyChunkType.Material_Specular2:
					case PolyChunkType.Material_DiffuseSpecular2:
					case PolyChunkType.Material_AmbientSpecular2:
					case PolyChunkType.Material_DiffuseAmbientSpecular2:
						chunk = reader.ReadObject<MaterialChunk>();
						break;
					case PolyChunkType.Material_Bump:
						chunk = reader.ReadObject<MaterialBumpChunk>();
						break;
					case PolyChunkType.Volume_Triangle:
					case PolyChunkType.Volume_Quad:
					case PolyChunkType.Volume_Strip:
						chunk = reader.ReadObject<VolumeChunk>();
						break;
					case PolyChunkType.Strip_Blank:
					case PolyChunkType.Strip_Tex:
					case PolyChunkType.Strip_HDTex:
					case PolyChunkType.Strip_Normal:
					case PolyChunkType.Strip_TexNormal:
					case PolyChunkType.Strip_HDTexNormal:
					case PolyChunkType.Strip_Color:
					case PolyChunkType.Strip_TexColor:
					case PolyChunkType.Strip_HDTexColor:
					case PolyChunkType.Strip_BlankDouble:
					case PolyChunkType.Strip_TexDouble:
					case PolyChunkType.Strip_HDTexDouble:
						chunk = reader.ReadObject<StripChunk>();
						break;
					case PolyChunkType.Null:
						reader.Skip(sizeof(ushort));
						continue;
					case PolyChunkType.End:
						reader.Skip(sizeof(ushort));
						goto End;
					default:
						throw new InvalidOperationException(); // cant be reached
				}

				chunks.Add(chunk);
				context.OffsetLUT.PolyChunks.Add(offset, chunk);
			}

			End:
			Array = [.. chunks];
		}

		void IBinarySerializable<IOContext>.Write(BinaryObjectWriter writer, IOContext context)
		{
			long start = writer.Position;

			foreach(PolyChunk chunk in this)
			{
				long offset = chunk.AlignWithFour
					? AlignmentHelper.Align(writer.Position, 4)
					: writer.Position;

				offset = writer.OffsetHandler.CalculateOffset(offset);

				writer.WriteObject(chunk);
				context.OffsetLUT.PolyChunks.Add(offset, chunk);
			}

			// End chunk
			writer.WriteUInt16((ushort)PolyChunkType.End);

			if((writer.Position - start) % 4 == 2)
			{
				writer.WriteUInt16(0);
			}
		}

		void IAsciiSerializable<ModelAsciiIOContext>.Write(AsciiWriter writer, ModelAsciiIOContext context)
		{
			using AsciiWriterBlockToken? block = writer.WriteStructBlockWithReference("PLIST", this);

			if(block == null)
			{
				return;
			}

			int offset = 0;

			foreach(PolyChunk chunk in this)
			{
				if(chunk.AlignWithFour && offset % 4 != 0)
				{
					offset += 2;
					writer.WriteLine("\tCnkNull(),");
				}

				writer.WriteObject(chunk, context);
				offset += 2;

				if(chunk is not BitsChunk)
				{
					offset += 2;

					if(chunk is SizedChunk sizedChunk)
					{
						offset += sizedChunk.Size * 2;
					}
				}
			}

			writer.WriteLine("\tCnkEnd()");
		}

	}
}
