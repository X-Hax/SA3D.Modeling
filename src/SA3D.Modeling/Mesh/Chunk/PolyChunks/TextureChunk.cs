using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common;
using SA3D.Common.Ascii;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Contains texture information.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public class TextureChunk : PolyChunk
	{
		internal class JsonConverter : ChildJsonObjectConverter<PolyChunkType, TextureChunk, PolyChunk>
		{
			private const string _mipmapDistanceMultiplier = nameof(MipmapDistanceMultiplier);
			private const string _clampV = nameof(ClampV);
			private const string _clampU = nameof(ClampU);
			private const string _mirrorV = nameof(MirrorV);
			private const string _mirrorU = nameof(MirrorU);
			private const string _textureID = nameof(TextureID);
			private const string _superSample = nameof(SuperSample);
			private const string _filterMode = nameof(FilterMode);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<PolyChunkType, PolyChunk> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
		{
			{ _mipmapDistanceMultiplier, new(PropertyTokenType.Number, 1f) },
			{ _clampV, new(PropertyTokenType.Bool, false) },
			{ _clampU, new(PropertyTokenType.Bool, false) },
			{ _mirrorV, new(PropertyTokenType.Bool, false) },
			{ _mirrorU, new(PropertyTokenType.Bool, false) },
			{ _textureID, new(PropertyTokenType.Number, (ushort)0) },
			{ _superSample, new(PropertyTokenType.Bool, false) },
			{ _filterMode, new(PropertyTokenType.String, FilterMode.Bilinear) },

		});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(PolyChunkType key)
			{
				return key is PolyChunkType.TextureID or PolyChunkType.TextureID2;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _mipmapDistanceMultiplier:
						return reader.GetSingle();
					case _clampV:
					case _clampU:
					case _mirrorV:
					case _mirrorU:
					case _superSample:
						return reader.GetBoolean();
					case _textureID:
						return reader.GetUInt16();
					case _filterMode:
						return JsonSerializer.Deserialize<FilterMode>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override TextureChunk CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					Second = values[BaseJsonConverter._type] is PolyChunkType.TextureID2,
					MipmapDistanceMultiplier = (float)values[_mipmapDistanceMultiplier]!,
					ClampV = (bool)values[_clampV]!,
					ClampU = (bool)values[_clampU]!,
					MirrorV = (bool)values[_mirrorV]!,
					MirrorU = (bool)values[_mirrorU]!,
					TextureID = (ushort)values[_textureID]!,
					SuperSample = (bool)values[_superSample]!,
					FilterMode = (FilterMode)values[_filterMode]!,
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, TextureChunk value, JsonSerializerOptions options)
			{
				void writeBoolean(string name, bool value)
				{
					if(value)
					{
						writer.WriteBoolean(name, value);
					}
				}

				if(value.MipmapDistanceMultiplier != 1f)
				{
					writer.WriteNumber(_mipmapDistanceMultiplier, value.MipmapDistanceMultiplier);
				}

				writeBoolean(_clampV, value.ClampV);
				writeBoolean(_clampU, value.ClampU);
				writeBoolean(_mirrorV, value.MirrorV);
				writeBoolean(_mirrorU, value.MirrorU);
				writer.WriteNumber(_textureID, value.TextureID);
				writeBoolean(_superSample, value.SuperSample);

				writer.WritePropertyName(_filterMode);
				JsonSerializer.Serialize(writer, value.FilterMode, options);
			}
		}

		/// <inheritdoc/>
		protected override bool AlignWithFour => true;

		/// <summary>
		/// Whether the chunktype is <see cref="PolyChunkType.TextureID2"/>.
		/// </summary>
		public bool Second
		{
			get => Type == PolyChunkType.TextureID2;
			set => Type = value ? PolyChunkType.TextureID2 : PolyChunkType.TextureID;
		}

		/// <summary>
		/// The mipmap distance multiplier.
		/// <br/> Ranges from 0 to 3.75f in increments of 0.25.
		/// </summary>
		public float MipmapDistanceMultiplier
		{
			get => byte.Max(1, (byte)(Attributes & 0xF)) * 0.25f;
			set => Attributes = (byte)((Attributes & 0xF0) | (byte)Math.Max(1, Math.Min(0xF, Math.Round(value / 0.25, MidpointRounding.AwayFromZero))));
		}

		/// <summary>
		/// Clamps texture corrdinates on the vertical axis between -1 and 1.
		/// </summary>
		public bool ClampV
		{
			get => (Attributes & 0x10) != 0;
			set => _ = value ? Attributes |= 0x10 : Attributes &= 0xEF;
		}

		/// <summary>
		/// Clamps texture corrdinates on the horizontal axis between -1 and 1.
		/// </summary>
		public bool ClampU
		{
			get => (Attributes & 0x20) != 0;
			set => _ = value ? Attributes |= 0x20 : Attributes &= 0xDF;
		}

		/// <summary>
		/// Mirrors the texture every second time the texture is repeated along the vertical axis.
		/// </summary>
		public bool MirrorV
		{
			get => (Attributes & 0x40) != 0;
			set => _ = value ? Attributes |= 0x40 : Attributes &= 0xBF;
		}

		/// <summary>
		/// Mirrors the texture every second time the texture is repeated along the horizontal axis.
		/// </summary>
		public bool MirrorU
		{
			get => (Attributes & 0x80) != 0;
			set => _ = value ? Attributes |= 0x80 : Attributes &= 0x7F;
		}


		/// <summary>
		/// Second set of data bytes.
		/// </summary>
		public ushort Data { get; private set; }

		/// <summary>
		/// Texture ID to use.
		/// </summary>
		public ushort TextureID
		{
			get => (ushort)(Data & 0x1FFFu);
			set => Data = (ushort)((Data & ~0x1FFF) | Math.Min(value, (ushort)0x1FFF));
		}

		/// <summary>
		/// Whether to use super sampling (anisotropic filtering).
		/// </summary>
		public bool SuperSample
		{
			get => (Data & 0x2000) != 0;
			set => _ = value ? Data |= 0x2000 : Data &= 0xDFFF;
		}

		/// <summary>
		/// Texture pixel filtering mode.
		/// </summary>
		public FilterMode FilterMode
		{
			get => (FilterMode)(Data >> 14);
			set => Data = (ushort)((Data & ~0xC000) | ((ushort)value << 14));
		}


		/// <summary>
		/// Creates a new texture chunk.
		/// </summary>
		public TextureChunk() : base(PolyChunkType.TextureID) { }

		/// <inheritdoc/>
		protected override bool IsTypeApplicable(PolyChunkType type)
		{
			return type is PolyChunkType.TextureID or PolyChunkType.TextureID2;
		}

		/// <inheritdoc/>
		public override void Read(BinaryObjectReader reader)
		{
			base.Read(reader);
			Data = reader.ReadUInt16();
		}

		/// <inheritdoc/>
		public override void Write(BinaryObjectWriter writer)
		{
			base.Write(writer);
			writer.WriteUInt16(Data);
		}

		/// <inheritdoc/>
		protected override string GetAsciiBits()
		{
			string result;

			if((Attributes & 0xF0) == 0)
			{
				result = "0x0";
			}
			else
			{
				result = string.Empty;
				if(ClampU)
				{
					result += "|FCL_U";
				}

				if(ClampV)
				{
					result += "|FCL_V";
				}

				if(MirrorU)
				{
					result += "|FFL_U";
				}

				if(MirrorV)
				{
					result += "|FFL_V";
				}

				result = result[1..];
			}

			if((Attributes & 0xF) == 0)
			{
				result += "|FDA_100";
			}
			else
			{
				result += $"|FDA_{(Attributes & 0xF) * 25:D3}";
			}

			return result;
		}

		/// <inheritdoc/>
		public override void Write(AsciiWriter writer, ModelAsciiContext context)
		{
			base.Write(writer, context);

			string filterMode = AsciiMaps.FilterModeMap.FindKey(FilterMode);
			if(SuperSample)
			{
				filterMode += "|FSS";
			}

			writer.WriteLine($" _TID({filterMode}, {TextureID}),");
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Type} - {TextureID}";
		}
	}
}
