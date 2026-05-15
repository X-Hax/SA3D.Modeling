using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Chunk.PolyChunks;
using System;
using System.Collections.Generic;

namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Polychunk base class.
	/// </summary>
	public abstract class PolyChunk : ICloneable, IBinarySerializable
	{
		/// <summary>
		/// Chunk type
		/// </summary>
		public PolyChunkType Type
		{
			get;
			protected set
			{
				if(!Enum.IsDefined(value) || value is PolyChunkType.End or PolyChunkType.Null)
				{
					throw new FormatException($"Poly chunk type is invalid: {value}");
				}

				if(!IsTypeApplicable(value))
				{
					throw new ArgumentException($"Poly chunk type \"{value}\" is not allowed in {GetType()}");
				}

				field = value;
			}
		}

		/// <summary>
		/// Additonal attributes.
		/// </summary>
		public byte Attributes { get; set; }

		/// <summary>
		/// Whether the polygon chunk position and size needs to be a multiple of 4
		/// </summary>
		protected abstract bool AlignWithFour { get; }

		/// <summary>
		/// Base constructor for every poly chunk.
		/// </summary>
		/// <param name="type"></param>
		protected PolyChunk(PolyChunkType type)
		{
			Type = type;
		}


		/// <summary>
		/// Checks whether a given polychunk type can be applied to this polychunk implementation
		/// </summary>
		/// <param name="type">The type to check</param>
		/// <returns></returns>
		protected virtual bool IsTypeApplicable(PolyChunkType type)
		{
			// only allowing type to be set via constructor
			return Type == default || type == Type;
		}

		/// <inheritdoc/>
		public virtual void Read(BinaryObjectReader reader)
		{
			ushort header = reader.ReadUInt16();
			Type = (PolyChunkType)(header & 0xFF);
			Attributes = (byte)(header >> 8);
		}

		internal static LabeledArray<PolyChunk> ReadArray(BinaryObjectReader reader)
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
			}

			End:
			return new([.. chunks]);
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer)
		{
			if(AlignWithFour)
			{
				writer.Align(4);
			}

			writer.WriteUInt16((ushort)((byte)Type | (Attributes << 8)));
			WriteData(writer);
		}

		/// <summary>
		/// Writes additional polychunk data
		/// </summary>
		/// <param name="writer">The writer to write to</param>
		protected virtual void WriteData(BinaryObjectWriter writer) { }

		internal static void WriteArray(BinaryObjectWriter writer, IEnumerable<PolyChunk> chunks)
		{
			writer.WriteObjectArray(chunks);

			// End chunk
			writer.WriteUInt16((ushort)PolyChunkType.End);
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
