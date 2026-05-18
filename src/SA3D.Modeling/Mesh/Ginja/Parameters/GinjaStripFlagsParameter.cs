using J113D.Json;
using SA3D.Modeling.Mesh.Ginja.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Holds lighting information
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct GinjaStripFlagsParameter : IGinjaParameter
	{
		internal class JsonConverter : ChildJsonObjectConverter<GinjaParameterType, GinjaStripFlagsParameter, IGinjaParameter>
		{
			private const string _channelCount = nameof(ChannelCount);
			private const string _texGenCount = nameof(TexGenCount);
			private const string _ignoreLight = nameof(IgnoreLight);
			private const string _ignoreSpecular = nameof(IgnoreSpecular);
			private const string _ignoreAmbient = nameof(IgnoreAmbient);
			private const string _useVertexColorForDiffuse = nameof(UseVertexColorForDiffuse);
			private const string _useVertexColorForAmbient = nameof(UseVertexColorForAmbient);
			private const string _useAlpha = nameof(UseAlpha);
			private const string _noPunchThrough = nameof(NoPunchThrough);
			private const string _doubleSided = nameof(DoubleSided);
			private const string _tevStageCount = nameof(TevStageCount);

			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<GinjaParameterType, IGinjaParameter> ParentConverter => IGinjaParameter.BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _channelCount, new(PropertyTokenType.Number, 0) },
				{ _texGenCount, new(PropertyTokenType.Number, 0) },
				{ _ignoreLight, new(PropertyTokenType.Bool, false) },
				{ _ignoreSpecular, new(PropertyTokenType.Bool, false) },
				{ _ignoreAmbient, new(PropertyTokenType.Bool, false) },
				{ _useVertexColorForDiffuse, new(PropertyTokenType.Bool, false) },
				{ _useVertexColorForAmbient, new(PropertyTokenType.Bool, false) },
				{ _useAlpha, new(PropertyTokenType.Bool, false) },
				{ _noPunchThrough, new(PropertyTokenType.Bool, false) },
				{ _doubleSided, new(PropertyTokenType.Bool, false) },
				{ _tevStageCount, new(PropertyTokenType.Number, 0) }
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(GinjaParameterType key)
			{
				return key == GinjaParameterType.StripFlags;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _channelCount:
					case _texGenCount:
					case _tevStageCount:
						return reader.GetByte();
					case _ignoreLight:
					case _ignoreSpecular:
					case _ignoreAmbient:
					case _useVertexColorForDiffuse:
					case _useVertexColorForAmbient:
					case _useAlpha:
					case _noPunchThrough:
					case _doubleSided:
						return reader.GetBoolean();
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override GinjaStripFlagsParameter CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					ChannelCount = (byte)values[_channelCount]!,
					TexGenCount = (byte)values[_texGenCount]!,
					IgnoreLight = (bool)values[_ignoreLight]!,
					IgnoreSpecular = (bool)values[_ignoreSpecular]!,
					IgnoreAmbient = (bool)values[_ignoreAmbient]!,
					UseVertexColorForDiffuse = (bool)values[_useVertexColorForDiffuse]!,
					UseVertexColorForAmbient = (bool)values[_useVertexColorForAmbient]!,
					UseAlpha = (bool)values[_useAlpha]!,
					NoPunchThrough = (bool)values[_noPunchThrough]!,
					DoubleSided = (bool)values[_doubleSided]!,
					TevStageCount = (byte)values[_tevStageCount]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, GinjaStripFlagsParameter value, JsonSerializerOptions options)
			{
				void writeByte(string name, byte value)
				{
					if(value != 0)
					{
						writer.WriteNumber(name, value);
					}
				}

				void writeBoolean(string name, bool value)
				{
					if(value)
					{
						writer.WriteBoolean(name, value);
					}
				}

				writeByte(_channelCount, value.ChannelCount);
				writeByte(_texGenCount, value.TexGenCount);
				writeBoolean(_ignoreLight, value.IgnoreLight);
				writeBoolean(_ignoreSpecular, value.IgnoreSpecular);
				writeBoolean(_ignoreAmbient, value.IgnoreAmbient);
				writeBoolean(_useVertexColorForDiffuse, value.UseVertexColorForDiffuse);
				writeBoolean(_useVertexColorForAmbient, value.UseVertexColorForAmbient);
				writeBoolean(_useAlpha, value.UseAlpha);
				writeBoolean(_noPunchThrough, value.NoPunchThrough);
				writeBoolean(_doubleSided, value.DoubleSided);
				writeByte(_tevStageCount, value.TevStageCount);
			}
		}

		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.StripFlags;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Number of output channels to be used.
		/// <br/> Ranges from 0 - 2
		/// </summary>
		public byte ChannelCount
		{
			readonly get => (byte)(Data & 0x3);
			set => Data = (Data & ~0x3u) | byte.Clamp(value, 0, 2);
		}

		/// <summary>
		/// Number of <see cref="GinjaTexGenParameter"/> used.
		/// <br/> Ranges from 0 - 15
		/// </summary>
		public byte TexGenCount
		{
			readonly get => (byte)((Data >> 4) & 0xF);
			set => Data = (Data & ~0xF0u) | (uint)(byte.Clamp(value, 0, 15) << 4);
		}

		/// <summary>
		/// Enables fullbright (no diffuse lighting &amp; ambient light set to white. Priority over <see cref="IgnoreAmbient"/>)
		/// </summary>
		public bool IgnoreLight
		{
			readonly get => GetFlag(0x100u);
			set => SetFlag(0x100, value);
		}

		/// <summary>
		/// Ignores specular lighting.
		/// </summary>
		public bool IgnoreSpecular
		{
			readonly get => GetFlag(0x200);
			set => SetFlag(0x200, value);
		}

		/// <summary>
		/// Ignores ambient lighting.
		/// </summary>
		public bool IgnoreAmbient
		{
			readonly get => GetFlag(0x400);
			set => SetFlag(0x400, value);
		}

		/// <summary>
		/// Use vertex colors for diffuse color, instead of from <see cref="GinjaDiffuseColorParameter"/>
		/// </summary>
		public bool UseVertexColorForDiffuse
		{
			readonly get => GetFlag(0x800);
			set => SetFlag(0x800, value);
		}

		/// <summary>
		/// Use vertex colors for ambient color, instead of from <see cref="GinjaAmbientColorParameter"/>
		/// </summary>
		public bool UseVertexColorForAmbient
		{
			readonly get => GetFlag(0x1000);
			set => SetFlag(0x1000, value);
		}

		/// <summary>
		/// Enables transparency
		/// </summary>
		public bool UseAlpha
		{
			readonly get => GetFlag(0x2000);
			set => SetFlag(0x2000, value);
		}

		/// <summary>
		/// Disables punchthrough rendering
		/// </summary>
		public bool NoPunchThrough
		{
			readonly get => GetFlag(0x4000);
			set => SetFlag(0x4000, value);
		}

		/// <summary>
		/// Disables backface culling.
		/// </summary>
		public bool DoubleSided
		{
			readonly get => GetFlag(0x8000);
			set => SetFlag(0x8000, value);
		}

		/// <summary>
		/// Number of TevStages used.
		/// <br/> Ranges from 0 - 15
		/// </summary>
		public byte TevStageCount
		{
			readonly get => (byte)((Data >> 16) & 0xF);
			set => Data = (Data & ~0xF0000u) | (uint)(byte.Clamp(value, 0, 15) << 16);
		}


		private readonly bool GetFlag(uint mask)
		{
			return (Data & mask) != 0;
		}

		private void SetFlag(uint mask, bool value)
		{
			if(value)
			{
				Data |= mask;
			}
			else
			{
				Data &= ~mask;
			}
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			string flagString =
				(IgnoreLight ? "X" : "-")
				+ (IgnoreSpecular ? 'X' : '-')
				+ (IgnoreAmbient ? 'X' : '-')
				+ (UseVertexColorForDiffuse ? 'X' : '-')
				+ "_"
				+ (UseVertexColorForAmbient ? 'X' : '-')
				+ (UseAlpha ? 'X' : '-')
				+ (NoPunchThrough ? 'X' : '-')
				+ (DoubleSided ? 'X' : '-');

			return $"Strip flags: {ChannelCount} - {TexGenCount} - {TevStageCount} - {flagString}";
		}
	}
}
