using SA3D.Common.IO;
using SA3D.Modeling.Mesh.Chunk.PolyChunks;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;

namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Polychunk base class.
	/// </summary>
	public abstract class PolyChunk : ICloneable
	{
		/// <summary>
		/// Type.
		/// </summary>
		public PolyChunkType Type { get; protected set; }

		/// <summary>
		/// Additonal attributes.
		/// </summary>
		public byte Attributes { get; set; }

		/// <summary>
		/// Size of the chunk in bytes.
		/// </summary>
		public abstract uint ByteSize { get; }

		/// <summary>
		/// Base constructor for every poly chunk.
		/// </summary>
		/// <param name="type"></param>
		protected PolyChunk(PolyChunkType type)
		{
			Type = type;
		}


		/// <summary>
		/// Writes the poly chunk to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		public void Write(EndianStackWriter writer, PointerLUT lut)
		{
			lut.PolyChunks.Add(writer.PointerPosition, this);
			writer.WriteUShort((ushort)((byte)Type | (Attributes << 8)));
			InternalWrite(writer);
		}

		/// <summary>
		/// Writes an array of poly chunks to an endian stack writer. Includes NULL and END chunks.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="chunks">Chunks to writ.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns></returns>
		public static uint WriteArray(EndianStackWriter writer, IEnumerable<PolyChunk?> chunks, PointerLUT lut)
		{
			uint result = writer.PointerPosition;

			foreach(PolyChunk? chunk in chunks)
			{
				if(chunk == null)
				{
					writer.WriteEmpty(2);
					continue;
				}

				chunk.Write(writer, lut);
			}

			// end chunk
			writer.WriteUShort(0xFF);

			return result;
		}

		/// <summary>
		/// Writes the poly chunks body to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		protected abstract void InternalWrite(EndianStackWriter writer);

		/// <summary>
		/// Reads a poly chunk off an endian stack reader. Advances the address by the number of bytes read.
		/// </summary>
		/// <param name="reader">Reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The poly chunk that was read.</returns>
		public static PolyChunk Read(EndianStackReader reader, ref uint address, PointerLUT lut)
		{
			uint chunkAddress = address;
			ushort header = reader.ReadUShort(address);
			PolyChunkType type = (PolyChunkType)(header & 0xFF);
			byte attribs = (byte)(header >> 8);

			if(!Enum.IsDefined(type) || type is PolyChunkType.End or PolyChunkType.Null)
			{
				throw new FormatException($"Poly chunk type is invalid: {type}");
			}

			PolyChunk chunk;
			switch(type)
			{
				case PolyChunkType.BlendAlpha:
					chunk = new BlendAlphaChunk();
					address += chunk.ByteSize;
					break;
				case PolyChunkType.MipmapDistanceMultiplier:
					chunk = new MipmapDistanceMultiplierChunk();
					address += chunk.ByteSize;
					break;
				case PolyChunkType.SpecularExponent:
					chunk = new SpecularExponentChunk();
					address += chunk.ByteSize;
					break;
				case PolyChunkType.CacheList:
					chunk = new CacheListChunk();
					address += chunk.ByteSize;
					break;
				case PolyChunkType.DrawList:
					chunk = new DrawListChunk();
					address += chunk.ByteSize;
					break;
				case PolyChunkType.TextureID:
				case PolyChunkType.TextureID2:
					chunk = TextureChunk.Read(reader, address);
					address += chunk.ByteSize;
					break;
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
					chunk = MaterialChunk.Read(reader, ref address);
					break;
				case PolyChunkType.Material_Bump:
					chunk = MaterialBumpChunk.Read(reader, address);
					address += chunk.ByteSize;
					break;
				case PolyChunkType.Volume_Polygon3:
				case PolyChunkType.Volume_Polygon4:
				case PolyChunkType.Volume_Strip:
					chunk = VolumeChunk.Read(reader, ref address);
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
					chunk = StripChunk.Read(reader, ref address);
					break;
				case PolyChunkType.Null:
				case PolyChunkType.End:
				default:
					throw new InvalidOperationException(); // cant be reached
			}

			chunk.Attributes = attribs;
			lut.PolyChunks.Add(chunkAddress, chunk);
			return chunk;
		}

		/// <summary>
		/// Reads an array of poly chunks off an endian stack reader. Respects NULL and END chunks.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The poly chunks that were read.</returns>
		public static PolyChunk?[] ReadArray(EndianStackReader reader, uint address, PointerLUT lut)
		{
			List<PolyChunk?> result = new();

			PolyChunkType readType()
			{
				return (PolyChunkType)(reader.ReadUShort(address) & 0xFF);
			}

			for(PolyChunkType type = readType(); type != PolyChunkType.End; type = readType())
			{
				if(type == PolyChunkType.Null)
				{
					result.Add(null);
					address += 2;
					continue;
				}

				result.Add(Read(reader, ref address, lut));
			}

			return result.ToArray();
		}



		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the poly chunk.
		/// </summary>
		/// <returns>The cloned poly chunk</returns>
		public virtual PolyChunk Clone()
		{
			return (PolyChunk)MemberwiseClone();
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Type.ToString();
		}
	}
}
