using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common;
using SA3D.Common.Ascii;
using SA3D.Common.IO;
using SA3D.Modeling.Mesh.Chunk.Structs;
using SA3D.Modeling.ObjectData.Structs;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Chunk holding polygon data for rendering.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class StripChunk : SizedChunk
	{
		internal class JsonConverter : ChildJsonObjectConverter<PolyChunkType, StripChunk, PolyChunk>
		{
			private const string _ignoreLight = nameof(IgnoreLight);
			private const string _ignoreSpecular = nameof(IgnoreSpecular);
			private const string _ignoreAmbient = nameof(IgnoreAmbient);
			private const string _doubleSide = nameof(DoubleSide);
			private const string _flatShading = nameof(FlatShading);
			private const string _environmentMapping = nameof(EnvironmentMapping);
			private const string _alphaMode = nameof(AlphaMode);
			private const string _strips = nameof(Strips);
			private const string _triangleAttributeCount = nameof(TriangleAttributeCount);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<PolyChunkType, PolyChunk> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _ignoreLight, new(PropertyTokenType.Bool, false) },
				{ _ignoreSpecular, new(PropertyTokenType.Bool, false) },
				{ _ignoreAmbient, new(PropertyTokenType.Bool, false) },
				{ _doubleSide, new(PropertyTokenType.Bool, false) },
				{ _flatShading, new(PropertyTokenType.Bool, false) },
				{ _environmentMapping, new(PropertyTokenType.Bool, false) },
				{ _alphaMode, new(PropertyTokenType.String, AlphaMode.Opaque) },
				{ _strips, new(PropertyTokenType.Array, null) },
				{ _triangleAttributeCount, new(PropertyTokenType.Number, 0) },

			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(PolyChunkType key)
			{
				return key is >= PolyChunkType.Strip_Blank and <= PolyChunkType.Strip_HDTexDouble;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _ignoreLight:
					case _ignoreSpecular:
					case _ignoreAmbient:
					case _doubleSide:
					case _flatShading:
					case _environmentMapping:
						return reader.GetBoolean();
					case _alphaMode:
						return JsonSerializer.Deserialize<AlphaMode>(ref reader, options);
					case _strips:
						return JsonSerializer.Deserialize<ChunkStrip[]>(ref reader, options);
					case _triangleAttributeCount:
						return reader.GetInt32();
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override StripChunk CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				ChunkStrip[] strips = (ChunkStrip[]?)values[_strips]
					?? throw new InvalidDataException($"Strip chunk requires \"{_strips}\" property!");

				return new()
				{
					Type = (PolyChunkType)values[BaseJsonConverter._type]!,
					Strips = strips,
					TriangleAttributeCount = (int)values[_triangleAttributeCount]!,
					IgnoreLight = (bool)values[_ignoreLight]!,
					IgnoreSpecular = (bool)values[_ignoreSpecular]!,
					IgnoreAmbient = (bool)values[_ignoreAmbient]!,
					DoubleSide = (bool)values[_doubleSide]!,
					FlatShading = (bool)values[_flatShading]!,
					EnvironmentMapping = (bool)values[_environmentMapping]!,
					AlphaMode = (AlphaMode)values[_alphaMode]!,
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, StripChunk value, JsonSerializerOptions options)
			{
				void writeBoolean(string name, bool value)
				{
					if(value)
					{
						writer.WriteBoolean(name, value);
					}
				}

				writeBoolean(_ignoreLight, value.IgnoreLight);
				writeBoolean(_ignoreSpecular, value.IgnoreSpecular);
				writeBoolean(_ignoreAmbient, value.IgnoreAmbient);
				writeBoolean(_doubleSide, value.DoubleSide);
				writeBoolean(_flatShading, value.FlatShading);
				writeBoolean(_environmentMapping, value.EnvironmentMapping);

				if(value.AlphaMode != AlphaMode.Opaque)
				{
					writer.WritePropertyName(_alphaMode);
					JsonSerializer.Serialize(writer, value.AlphaMode, options);
				}

				if(value.TriangleAttributeCount != 0)
				{
					writer.WriteNumber(_triangleAttributeCount, value.TriangleAttributeCount);
				}

				writer.WritePropertyName(_strips);
				JsonSerializer.Serialize(writer, value.Strips, options);
			}
		}

		#region Attribute Properties

		/// <summary>
		/// Enables fullbright (no diffuse lighting &amp; ambient light set to white. Priority over <see cref="IgnoreAmbient"/>)
		/// <br/> 0x01 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool IgnoreLight
		{
			get => GetAttributeFlag(1);
			set => SetAttributeFlag(0x01, value);
		}

		/// <summary>
		/// Ignores specular lighting.
		/// <br/> 0x02 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool IgnoreSpecular
		{
			get => GetAttributeFlag(2);
			set => SetAttributeFlag(0x02, value);
		}

		/// <summary>
		/// Ignores ambient lighting.
		/// <br/> 0x04 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool IgnoreAmbient
		{
			get => GetAttributeFlag(4);
			set => SetAttributeFlag(0x04, value);
		}

		/// <summary>
		/// Renders polygons with transparency enabled.
		/// <br/> 0x08 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool UseAlpha
		{
			get => GetAttributeFlag(8);
			set => SetAttributeFlag(0x08, value);
		}

		/// <summary>
		/// Disables backface culling.
		/// <br/> 0x10 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool DoubleSide
		{
			get => GetAttributeFlag(0x10);
			set => SetAttributeFlag(0x10, value);
		}

		/// <summary>
		/// Ignore normals and render every polygon flat.
		/// <br/> 0x20 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool FlatShading
		{
			get => GetAttributeFlag(0x20);
			set => SetAttributeFlag(0x20, value);
		}

		/// <summary>
		/// Ignore texture coordinates and use normals for environment (matcap/normal) mapping.
		/// <br/> 0x40 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool EnvironmentMapping
		{
			get => GetAttributeFlag(0x40);
			set => SetAttributeFlag(0x40, value);
		}

		/// <summary>
		/// Extended alpha bit; Used by SA2B and the RenderFix mod
		/// <br/> 0x80 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool ExtendedUseAlpha
		{
			get => GetAttributeFlag(0x80);
			set => SetAttributeFlag(0x80, value);
		}

		/// <summary>
		/// Alpha mode derived from <see cref="UseAlpha"/> and <see cref="ExtendedUseAlpha"/>
		/// </summary>
		public AlphaMode AlphaMode
		{
			get
			{
				if(UseAlpha && ExtendedUseAlpha)
				{
					return AlphaMode.TransparentForceAlphaClipOn;
				}
				else if(ExtendedUseAlpha)
				{
					return AlphaMode.TransparentForceAlphaClipOff;
				}
				else if(UseAlpha)
				{
					return AlphaMode.Transparent;
				}
				else
				{
					return AlphaMode.Opaque;
				}
			}
			set
			{
				UseAlpha = value is AlphaMode.Transparent or AlphaMode.TransparentForceAlphaClipOn;
				ExtendedUseAlpha = value is AlphaMode.TransparentForceAlphaClipOn or AlphaMode.TransparentForceAlphaClipOff;
			}
		}

		private bool GetAttributeFlag(byte bits)
		{
			return (Attributes & bits) != 0;
		}

		private void SetAttributeFlag(byte bits, bool value)
		{
			if(value)
			{
				Attributes |= bits;
			}
			else
			{
				Attributes &= (byte)~bits;
			}
		}

		#endregion


		/// <summary>
		/// Triangle strips making up the polygons.
		/// </summary>
		public ChunkStrip[] Strips { get; set; }

		/// <summary>
		/// Number of custom attributes for each triangle.
		/// </summary>
		public int TriangleAttributeCount
		{
			get => field;
			set
			{
				if(value is < 0 or > 3)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "Value out of range. Must be between 0 and 3.");
				}

				field = value;
			}
		}

		/// <inheritdoc/>
		public override ushort Size
			=> (ushort)uint.Min(CalculateByteSize() / 2, ushort.MaxValue);


		/// <summary>
		/// Creates a new, empty strip chunk.
		/// </summary>
		public StripChunk() : base(PolyChunkType.Strip_Blank)
		{
			Strips = [];
		}


		/// <inheritdoc/>
		protected override bool IsTypeApplicable(PolyChunkType type)
		{
			return type is >= PolyChunkType.Strip_Blank and <= PolyChunkType.Strip_HDTexDouble;
		}

		/// <summary>
		/// Changes the type of the strip chunk.
		/// </summary>
		public void ChangeType(PolyChunkType type)
		{
			Type = type;
		}

		/// <summary>
		/// Calculate the chunks size.
		/// </summary>
		/// <returns></returns>
		public uint CalculateByteSize()
		{
			uint result = 2; // header ushort; strip count and triangle attributes

			uint structSize = (uint)(
				2u // vertex index
				+ (Type.GetStripTexCoordCount() * 4u)
				+ (Type.CheckStripHasNormals() ? 6u : 0u)
				+ (Type.CheckStripHasColors() ? 4u : 0u)
			);

			foreach(ChunkStrip str in Strips)
			{
				result += (uint)(
					2u // strip header
					+ (str.Corners.Length * structSize) // individual corners
					+ ((str.Corners.Length - 2) * TriangleAttributeCount * 2) // triangle attributes
				);
			}

			return result;
		}

		private void WriteCheck()
		{
			if(Strips.Length > 0x3FFF)
			{
				throw new InvalidOperationException($"Strip count ({Strips.Length}) exceeds maximum ({0x3FFF})");
			}

			uint size = CalculateByteSize() / 2;
			if(size > ushort.MaxValue)
			{
				throw new InvalidOperationException($"Strip chunk size ({size}) exceeds maximum size ({ushort.MaxValue}).");
			}
		}

		/// <inheritdoc/>
		protected override void Read(BinaryObjectReader reader)
		{
			base.Read(reader);

			ushort stripData = reader.ReadUInt16();
			Strips = new ChunkStrip[stripData & 0x3FFFu];
			TriangleAttributeCount = (byte)(stripData >> 14);

			bool hasNormals = Type.CheckStripHasNormals();
			bool hasColors = Type.CheckStripHasColors();

			bool hasUV = Type.GetStripTexCoordCount() > 0;
			bool hasUV2 = Type.GetStripTexCoordCount() > 1;

			bool flag1 = TriangleAttributeCount > 0;
			bool flag2 = TriangleAttributeCount > 1;
			bool flag3 = TriangleAttributeCount > 2;

			for(int i = 0; i < Strips.Length; i++)
			{
				short header = reader.ReadInt16();
				bool reverse = header < 0;
				ChunkCorner[] corners = new ChunkCorner[Math.Abs(header)];

				for(int j = 0; j < corners.Length; j++)
				{
					ChunkCorner c = ChunkCorner.DefaultValues;
					c.Index = reader.ReadUInt16();

					if(hasUV)
					{
						c.Texcoord = reader.ReadVector2(FloatIOType.Short);

						if(hasUV2)
						{
							c.Texcoord2 = reader.ReadVector2(FloatIOType.Short);
						}
					}

					if(hasNormals)
					{
						c.Normal = reader.ReadVector3(FloatIOType.NormalizedShort);
					}

					if(hasColors)
					{
						c.Color = reader.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_16);
					}

					if(flag1 && j > 1)
					{
						c.Attributes1 = reader.ReadUInt16();
						if(flag2)
						{
							c.Attributes2 = reader.ReadUInt16();
							if(flag3)
							{
								c.Attributes3 = reader.ReadUInt16();
							}
						}
					}

					corners[j] = c;
				}

				Strips[i] = new ChunkStrip(corners, reverse);
			}
		}

		/// <inheritdoc/>
		protected override void Write(BinaryObjectWriter writer)
		{
			WriteCheck();
			base.Write(writer);

			writer.WriteUInt16((ushort)(Strips.Length | (TriangleAttributeCount << 14)));

			bool hasNormals = Type.CheckStripHasNormals();
			bool hasColors = Type.CheckStripHasColors();

			bool hasUV = Type.GetStripTexCoordCount() > 0;
			bool hasUV2 = Type.GetStripTexCoordCount() > 1;

			bool flag1 = TriangleAttributeCount > 0;
			bool flag2 = TriangleAttributeCount > 1;
			bool flag3 = TriangleAttributeCount > 2;

			foreach(ChunkStrip s in Strips)
			{
				writer.WriteInt16(
					s.Reversed
					? (short)-s.Corners.Length
					: (short)s.Corners.Length
				);

				for(int j = 0; j < s.Corners.Length; j++)
				{
					ChunkCorner c = s.Corners[j];
					writer.WriteUInt16(c.Index);
					if(hasUV)
					{
						writer.WriteVector2(c.Texcoord, FloatIOType.Short);

						if(hasUV2)
						{
							writer.WriteVector2(c.Texcoord2, FloatIOType.Short);
						}
					}

					if(hasNormals)
					{
						writer.WriteVector3(c.Normal, FloatIOType.NormalizedShort);
					}

					if(hasColors)
					{
						writer.WriteObject(c.Color, ColorIOType.ARGB8_16);
					}

					if(flag1 && j > 1)
					{
						writer.WriteUInt16(c.Attributes1);
						if(flag2)
						{
							writer.WriteUInt16(c.Attributes2);
							if(flag3)
							{
								writer.WriteUInt16(c.Attributes3);
							}
						}
					}
				}
			}
		}

		/// <inheritdoc/>
		protected override string GetAsciiAttributes()
		{
			string result = string.Empty;

			if(IgnoreLight)
			{
				result += "|FST_IL";
			}

			if(IgnoreSpecular)
			{
				result += "|FST_IS";
			}

			if(IgnoreAmbient)
			{
				result += "|FST_IA";
			}

			if(UseAlpha)
			{
				result += "|FST_UA";
			}

			if(DoubleSide)
			{
				result += "|FST_DB";
			}

			if(FlatShading)
			{
				result += "|FST_FL";
			}

			if(EnvironmentMapping)
			{
				result += "|FST_ENV";
			}

			if(ExtendedUseAlpha)
			{
				result += "|FST_EUA";
			}

			return result == string.Empty ? "0x0" : result[1..];
		}

		/// <inheritdoc/>
		protected override void Write(AsciiWriter writer, ModelAsciiIOContext context)
		{
			WriteCheck();
			base.Write(writer, context);
			writer.WriteLine($" _NB( UFO_{TriangleAttributeCount}, {Strips.Length} ),");

			bool hasNormals = Type.CheckStripHasNormals();
			bool hasColors = Type.CheckStripHasColors();

			bool hasUV = Type.GetStripTexCoordCount() > 0;
			bool hasUV2 = Type.GetStripTexCoordCount() > 1;
			string uvMode = Type.CheckStripHasHDTexcoords() ? "Uvh" : "Uvn";

			foreach(ChunkStrip strip in Strips)
			{
				writer.Write($"\tStrip{(strip.Reversed ? 'R' : 'L')}({strip.Corners.Length}), ");

				if(Type is PolyChunkType.Strip_Blank or PolyChunkType.Strip_BlankDouble)
				{
					if(strip.Corners.Length > 10)
					{
						writer.WriteLine();
						writer.Write("\t\t");
					}

					for(int i = 0; i < strip.Corners.Length; i++)
					{
						writer.Write($"{strip.Corners[i].Index}, ");

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
					writer.WriteLine();
					for(int i = 0; i < strip.Corners.Length; i++)
					{
						ChunkCorner c = strip.Corners[i];

						writer.Write($"\t{c.Index},".PadRight(5));

						if(hasUV)
						{
							writer.Write($"\t{uvMode}( {c.Texcoord.X}, {c.Texcoord.Y} ),");

							if(hasUV2)
							{
								writer.Write($"\t{uvMode}( {c.Texcoord2.X}, {c.Texcoord2.Y} ),");
							}
						}

						if(hasNormals)
						{
							writer.Write($"\tPvn( {c.Normal.ToAscii()} ),");
						}

						if(hasColors)
						{
							writer.Write($"\tMDiff( {c.Color.Alpha}, {c.Color.Red}, {c.Color.Green}, {c.Color.Blue} ),");
						}

						if(i > 1)
						{
							writer.WritePolygonUserflags(TriangleAttributeCount, c.Attributes1, c.Attributes2, c.Attributes3, context.BaseContext.PolygonAttributesAsColor);
						}

						writer.WriteLine();
					}
				}
			}
		}

		/// <inheritdoc/>
		public override StripChunk Clone()
		{
			return new()
			{
				Type = Type,
				Strips = Strips.ContentClone(),
				Attributes = Attributes,
				TriangleAttributeCount = TriangleAttributeCount
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Type} - 0x{Attributes:X2}, {TriangleAttributeCount} : {Strips.Length}";
		}
	}
}
