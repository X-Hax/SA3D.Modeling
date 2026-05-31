using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using J113D.Json;
using SA3D.Common;
using SA3D.Common.Ascii;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Chunk.PolyChunks;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Polychunk base class.
	/// </summary>
	[JsonConverter(typeof(BaseJsonConverter))]
	public abstract class PolyChunk : ICloneable, IBinarySerializable, IAsciiSerializable<ModelAsciiContext>
	{
		internal class BaseJsonConverter : ParentJsonObjectConverter<PolyChunkType, PolyChunk>
		{
			public static readonly BaseJsonConverter instance = new();

			public const string _type = nameof(PolyChunk.Type);

			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _type, new(PropertyTokenType.String, null) }
			});

			/// <inheritdoc/>
			protected override string KeyPropertyName => _type;

			/// <inheritdoc/>
			protected override PolyChunk CreateBase(ReadOnlyDictionary<string, object?> values)
			{
				throw new NotSupportedException();
			}

			/// <inheritdoc/>
			protected override Dictionary<PolyChunkType, IChildJsonConverter<PolyChunk>> CreateConverters()
			{
				TextureChunk.JsonConverter textureConverter = new();
				MaterialChunk.JsonConverter materialConverter = new();
				StripChunk.JsonConverter stripConverter = new();
				VolumeChunk.JsonConverter volumeConverter = new();

				return new()
			{
				{ PolyChunkType.BlendAlpha, new BlendAlphaChunk.JsonConverter()},
				{ PolyChunkType.MipmapDistanceMultiplier, new MipmapDistanceMultiplierChunk.JsonConverter() },
				{ PolyChunkType.SpecularExponent, new SpecularExponentChunk.JsonConverter() },
				{ PolyChunkType.CacheList, new CacheListChunk.JsonConverter() },
				{ PolyChunkType.DrawList, new DrawListChunk.JsonConverter() },
				{ PolyChunkType.TextureID, textureConverter },
				{ PolyChunkType.TextureID2, textureConverter },
				{ PolyChunkType.Material_Diffuse, materialConverter },
				{ PolyChunkType.Material_Ambient, materialConverter },
				{ PolyChunkType.Material_DiffuseAmbient, materialConverter },
				{ PolyChunkType.Material_Specular, materialConverter },
				{ PolyChunkType.Material_DiffuseSpecular, materialConverter },
				{ PolyChunkType.Material_AmbientSpecular, materialConverter },
				{ PolyChunkType.Material_DiffuseAmbientSpecular, materialConverter },
				{ PolyChunkType.Material_Bump, new MaterialBumpChunk.JsonConverter() },
				{ PolyChunkType.Material_Diffuse2, materialConverter },
				{ PolyChunkType.Material_Ambient2, materialConverter },
				{ PolyChunkType.Material_DiffuseAmbient2, materialConverter },
				{ PolyChunkType.Material_Specular2, materialConverter },
				{ PolyChunkType.Material_DiffuseSpecular2, materialConverter },
				{ PolyChunkType.Material_AmbientSpecular2, materialConverter },
				{ PolyChunkType.Material_DiffuseAmbientSpecular2, materialConverter },
				{ PolyChunkType.Volume_Triangle, volumeConverter },
				{ PolyChunkType.Volume_Quad, volumeConverter },
				{ PolyChunkType.Volume_Strip, volumeConverter },
				{ PolyChunkType.Strip_Blank, stripConverter },
				{ PolyChunkType.Strip_Tex, stripConverter },
				{ PolyChunkType.Strip_HDTex, stripConverter },
				{ PolyChunkType.Strip_Normal, stripConverter },
				{ PolyChunkType.Strip_TexNormal, stripConverter },
				{ PolyChunkType.Strip_HDTexNormal, stripConverter },
				{ PolyChunkType.Strip_Color, stripConverter },
				{ PolyChunkType.Strip_TexColor, stripConverter },
				{ PolyChunkType.Strip_HDTexColor, stripConverter },
				{ PolyChunkType.Strip_BlankDouble, stripConverter },
				{ PolyChunkType.Strip_TexDouble, stripConverter },
				{ PolyChunkType.Strip_HDTexDouble, stripConverter },
			};
			}

			/// <inheritdoc/>
			protected override PolyChunkType GetKeyFromValue(PolyChunk value)
			{
				return value.Type;
			}

			/// <inheritdoc/>
			protected override object? ReadBaseValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _type:
						return JsonSerializer.Deserialize<PolyChunkType>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override void WriteBaseValues(Utf8JsonWriter writer, PolyChunk value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_type);
				JsonSerializer.Serialize(writer, value.Type, options);
			}
		}

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
		public virtual void Write(BinaryObjectWriter writer)
		{
			if(AlignWithFour)
			{
				writer.Align(4);
			}

			writer.WriteUInt16((ushort)((byte)Type | (Attributes << 8)));
		}

		internal static void WriteArray(BinaryObjectWriter writer, IEnumerable<PolyChunk> chunks)
		{
			writer.WriteObjectArray(chunks);

			// End chunk
			writer.WriteUInt16((ushort)PolyChunkType.End);
		}

		/// <inheritdoc/>
		public virtual void Write(AsciiWriter writer, ModelAsciiContext context)
		{
			string chunkType = AsciiMaps.PolyChunkTypeMap.FindKey(Type);
			string bits = GetAsciiBits();
			writer.Write($"\t{chunkType}( {bits} ),");
		}

		/// <summary>
		/// 
		/// </summary>
		protected abstract string GetAsciiBits();

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
