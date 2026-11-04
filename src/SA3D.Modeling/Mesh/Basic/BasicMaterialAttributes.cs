using J113D.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;

namespace SA3D.Modeling.Mesh.Basic
{
	/// <summary>
	/// Basic mesh material attributes
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct BasicMaterialAttributes
	{
		private class JsonConverter : SimpleJsonObjectConverter<BasicMaterialAttributes>
		{
			private const string _userAttributes = nameof(UserAttributes);
			private const string _pickStatus = nameof(PickStatus);
			private const string _mipmapDistanceMultiplier = nameof(MipmapDistanceMultiplier);
			private const string _superSample = nameof(SuperSample);
			private const string _filterMode = nameof(FilterMode);
			private const string _clampV = nameof(ClampV);
			private const string _clampU = nameof(ClampU);
			private const string _mirrorV = nameof(MirrorV);
			private const string _mirrorU = nameof(MirrorU);
			private const string _ignoreSpecular = nameof(IgnoreSpecular);
			private const string _useAlpha = nameof(UseAlpha);
			private const string _useTexture = nameof(UseTexture);
			private const string _environmentMap = nameof(EnvironmentMap);
			private const string _doubleSided = nameof(DoubleSided);
			private const string _flatShading = nameof(FlatShading);
			private const string _ignoreLighting = nameof(IgnoreLighting);
			private const string _destinationAlpha = nameof(DestinationAlpha);
			private const string _sourceAlpha = nameof(SourceAlpha);

			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
		{
			{ _userAttributes, new(PropertyTokenType.Number, DefaultValues.UserAttributes) },
			{ _pickStatus, new(PropertyTokenType.Bool, DefaultValues.PickStatus) },
			{ _mipmapDistanceMultiplier, new(PropertyTokenType.Number, DefaultValues.MipmapDistanceMultiplier) },
			{ _superSample, new(PropertyTokenType.Bool, DefaultValues.SuperSample) },
			{ _filterMode, new(PropertyTokenType.String, DefaultValues.FilterMode) },
			{ _clampV, new(PropertyTokenType.Bool, DefaultValues.ClampV) },
			{ _clampU, new(PropertyTokenType.Bool, DefaultValues.ClampU) },
			{ _mirrorV, new(PropertyTokenType.Bool, DefaultValues.MirrorV) },
			{ _mirrorU, new(PropertyTokenType.Bool, DefaultValues.MirrorU) },
			{ _ignoreSpecular, new(PropertyTokenType.Bool, DefaultValues.IgnoreSpecular) },
			{ _useAlpha, new(PropertyTokenType.Bool, DefaultValues.UseAlpha) },
			{ _useTexture, new(PropertyTokenType.Bool, DefaultValues.UseTexture) },
			{ _environmentMap, new(PropertyTokenType.Bool, DefaultValues.EnvironmentMap) },
			{ _doubleSided, new(PropertyTokenType.Bool, DefaultValues.DoubleSided) },
			{ _flatShading, new(PropertyTokenType.Bool, DefaultValues.FlatShading) },
			{ _ignoreLighting, new(PropertyTokenType.Bool, DefaultValues.IgnoreLighting) },
			{ _destinationAlpha, new(PropertyTokenType.String, DefaultValues.DestinationAlpha) },
			{ _sourceAlpha, new(PropertyTokenType.String, DefaultValues.SourceAlpha) },
		});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				return propertyName switch
				{
					_filterMode => JsonSerializer.Deserialize<FilterMode>(ref reader, options),

					_pickStatus
					or _superSample
					or _clampV
					or _clampU
					or _mirrorV
					or _mirrorU
					or _ignoreSpecular
					or _useAlpha
					or _useTexture
					or _environmentMap
					or _doubleSided
					or _flatShading
					or _ignoreLighting => reader.GetBoolean(),

					_destinationAlpha
					or _sourceAlpha => JsonSerializer.Deserialize<BlendMode>(ref reader, options),
					_ => throw new InvalidPropertyException(),
				};
			}

			/// <inheritdoc/>
			protected override BasicMaterialAttributes Create(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					UserAttributes = (byte)values[_userAttributes]!,
					PickStatus = (bool)values[_pickStatus]!,
					MipmapDistanceMultiplier = (float)values[_mipmapDistanceMultiplier]!,
					SuperSample = (bool)values[_superSample]!,
					FilterMode = (FilterMode)values[_filterMode]!,
					ClampV = (bool)values[_clampV]!,
					ClampU = (bool)values[_clampU]!,
					MirrorV = (bool)values[_mirrorV]!,
					MirrorU = (bool)values[_mirrorU]!,
					IgnoreSpecular = (bool)values[_ignoreSpecular]!,
					UseAlpha = (bool)values[_useAlpha]!,
					UseTexture = (bool)values[_useTexture]!,
					EnvironmentMap = (bool)values[_environmentMap]!,
					DoubleSided = (bool)values[_doubleSided]!,
					FlatShading = (bool)values[_flatShading]!,
					IgnoreLighting = (bool)values[_ignoreLighting]!,
					DestinationAlpha = (BlendMode)values[_destinationAlpha]!,
					SourceAlpha = (BlendMode)values[_sourceAlpha]!,
				};
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, BasicMaterialAttributes value, JsonSerializerOptions options)
			{
				void serialize<T>(string name, T value, T def) where T : notnull
				{
					if(!value.Equals(def))
					{
						writer.WritePropertyName(name);
						JsonSerializer.Serialize<T>(writer, value, options);
					}
				}

				void writeBoolean(string name, bool value, bool def)
				{
					if(value != def)
					{
						writer.WriteBoolean(name, value);
					}
				}

				if(value.UserAttributes != DefaultValues.UserAttributes)
				{
					writer.WriteNumber(_userAttributes, value.UserAttributes);
				}

				writeBoolean(_pickStatus, value.PickStatus, DefaultValues.PickStatus);

				if(value.MipmapDistanceMultiplier != DefaultValues.MipmapDistanceMultiplier)
				{
					writer.WriteNumber(_mipmapDistanceMultiplier, value.MipmapDistanceMultiplier);
				}

				writeBoolean(_superSample, value.SuperSample, DefaultValues.SuperSample);

				serialize(_filterMode, value.FilterMode, DefaultValues.FilterMode);

				writeBoolean(_clampV, value.ClampV, DefaultValues.ClampV);
				writeBoolean(_clampU, value.ClampU, DefaultValues.ClampU);
				writeBoolean(_mirrorV, value.MirrorV, DefaultValues.MirrorV);
				writeBoolean(_mirrorU, value.MirrorU, DefaultValues.MirrorU);
				writeBoolean(_ignoreSpecular, value.IgnoreSpecular, DefaultValues.IgnoreSpecular);
				writeBoolean(_useAlpha, value.UseAlpha, DefaultValues.UseAlpha);
				writeBoolean(_useTexture, value.UseTexture, DefaultValues.UseTexture);
				writeBoolean(_environmentMap, value.EnvironmentMap, DefaultValues.EnvironmentMap);
				writeBoolean(_doubleSided, value.DoubleSided, DefaultValues.DoubleSided);
				writeBoolean(_flatShading, value.FlatShading, DefaultValues.FlatShading);
				writeBoolean(_ignoreLighting, value.IgnoreLighting, DefaultValues.IgnoreLighting);

				serialize(_destinationAlpha, value.DestinationAlpha, DefaultValues.DestinationAlpha);
				serialize(_sourceAlpha, value.SourceAlpha, DefaultValues.SourceAlpha);
			}
		}


		/// <summary>
		/// Default material attribute values
		/// </summary>
		public static readonly BasicMaterialAttributes DefaultValues = new()
		{
			MipmapDistanceMultiplier = 1,
			FilterMode = FilterMode.Bilinear,
			UseTexture = true,
			DoubleSided = true,
			DestinationAlpha = BlendMode.SrcAlphaInverted,
			SourceAlpha = BlendMode.SrcAlpha,
		};

		/// <summary>
		/// Attributes containing various information.
		/// </summary>
		public uint Data { get; set; }


		/// <summary>
		/// User defined attributes.
		/// <br/> <see cref="Data"/> | 0x0000007F
		/// </summary>
		public byte UserAttributes
		{
			readonly get => (byte)(Data & 0x7Fu);
			set => Data = (Data & ~0x7Fu) | (value & 0x7Fu);
		}

		/// <summary>
		/// Editor property (?).
		/// <br/> <see cref="Data"/> | 0x00000080
		/// </summary>
		public bool PickStatus
		{
			readonly get => (Data & 0x80u) != 0;
			set => SetAttributeBit(0x80u, value);
		}

		/// <summary>
		/// Mipmad distance multiplier.
		/// <br/> <see cref="Data"/> | 0x00000F00
		/// </summary>
		public float MipmapDistanceMultiplier
		{
			readonly get => ((Data & 0xF00u) >> 8) * 0.25f;
			set => Data = (Data & ~0xF00u) | ((uint)Math.Max(0, Math.Min(0xF, Math.Round(value / 0.25, MidpointRounding.AwayFromZero))) << 8);
		}

		/// <summary>
		/// Super sampling (Anisotropic filtering?).
		/// <br/> <see cref="Data"/> | 0x00001000
		/// </summary>
		public bool SuperSample
		{
			readonly get => GetAttributeBit(0x1000u);
			set => SetAttributeBit(0x1000u, value);
		}

		/// <summary>
		/// Texture filter mode.
		/// <br/> <see cref="Data"/> | 0x00006000
		/// </summary>
		public FilterMode FilterMode
		{
			readonly get => (FilterMode)((Data >> 13) & 3);
			set => Data = (Data & ~0x6000u) | ((uint)value << 13);
		}

		/// <summary>
		/// Texture clamp along the V axis.
		/// <br/> <see cref="Data"/> | 0x00008000
		/// </summary>
		public bool ClampV
		{
			readonly get => GetAttributeBit(0x8000u);
			set => SetAttributeBit(0x8000u, value);
		}

		/// <summary>
		/// Texture clamp along the U axis.
		/// <br/> <see cref="Data"/> | 0x00010000
		/// </summary>
		public bool ClampU
		{
			readonly get => GetAttributeBit(0x10000u);
			set => SetAttributeBit(0x10000u, value);
		}

		/// <summary>
		/// Texture mirror along the V axis.
		/// <br/> <see cref="Data"/> | 0x00020000
		/// </summary>
		public bool MirrorV
		{
			readonly get => GetAttributeBit(0x20000u);
			set => SetAttributeBit(0x20000u, value);
		}

		/// <summary>
		/// Texture mirror along the U axis.
		/// <br/> <see cref="Data"/> | 0x00040000
		/// </summary>
		public bool MirrorU
		{
			readonly get => GetAttributeBit(0x40000u);
			set => SetAttributeBit(0x40000u, value);
		}

		/// <summary>
		/// Disables specular shading.
		/// <br/> <see cref="Data"/> | 0x00080000
		/// </summary>
		public bool IgnoreSpecular
		{
			readonly get => GetAttributeBit(0x80000u);
			set => SetAttributeBit(0x80000u, value);
		}

		/// <summary>
		/// Enables alpha blending.
		/// <br/> <see cref="Data"/> | 0x00100000
		/// </summary>
		public bool UseAlpha
		{
			readonly get => GetAttributeBit(0x100000u);
			set => SetAttributeBit(0x100000u, value);
		}

		/// <summary>
		/// Enables texture rendering.
		/// <br/> <see cref="Data"/> | 0x00200000
		/// </summary>
		public bool UseTexture
		{
			readonly get => GetAttributeBit(0x200000u);
			set => SetAttributeBit(0x200000u, value);
		}

		/// <summary>
		/// Applies the texture based on angle between camera and mesh normals (matcap method).
		/// <br/> <see cref="Data"/> | 0x00400000
		/// </summary>
		public bool EnvironmentMap
		{
			readonly get => GetAttributeBit(0x400000);
			set => SetAttributeBit(0x400000u, value);
		}

		/// <summary>
		/// Disables backface culling.
		/// <br/> <see cref="Data"/> | 0x00800000
		/// </summary>
		public bool DoubleSided
		{
			readonly get => GetAttributeBit(0x800000);
			set => SetAttributeBit(0x800000u, value);
		}

		/// <summary>
		/// Ignores interpolated normals and instead uses polygon-wide normals.
		/// <br/> <see cref="Data"/> | 0x01000000
		/// </summary>
		public bool FlatShading
		{
			readonly get => GetAttributeBit(0x1000000);
			set => SetAttributeBit(0x1000000u, value);
		}

		/// <summary>
		/// Disables shading altogether.
		/// <br/> <see cref="Data"/> | 0x02000000
		/// </summary>
		public bool IgnoreLighting
		{
			readonly get => GetAttributeBit(0x2000000);
			set => SetAttributeBit(0x2000000u, value);
		}

		/// <summary>
		/// Destination blend mode.
		/// <br/> <see cref="Data"/> | 0x1C000000
		/// </summary>
		public BlendMode DestinationAlpha
		{
			readonly get => (BlendMode)((Data >> 26) & 7);
			set => Data = (uint)((Data & ~0x1C000000) | ((uint)value << 26));
		}

		/// <summary>
		/// Source blend mode.
		/// <br/> <see cref="Data"/> | 0xE0000000
		/// </summary>
		public BlendMode SourceAlpha
		{
			readonly get => (BlendMode)((Data >> 29) & 7);
			set => Data = (Data & ~0xE0000000) | ((uint)value << 29);
		}

		private readonly bool GetAttributeBit(uint mask)
		{
			return (Data & mask) != 0;
		}

		private void SetAttributeBit(uint mask, bool value)
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
		public override readonly bool Equals(object? obj)
		{
			return obj is BasicMaterialAttributes attributes &&
				   Data == attributes.Data;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(Data);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return Data.ToString();
		}


		/// <summary>
		/// Implicit attributes from uint
		/// </summary>
		/// <param name="attributes"></param>
		public static implicit operator BasicMaterialAttributes(uint attributes)
		{
			BasicMaterialAttributes result = default;
			result.Data = attributes;
			return result;
		}

		/// <summary>
		/// Implicit uint from attributes
		/// </summary>
		/// <param name="attributes"></param>
		public static implicit operator uint(BasicMaterialAttributes attributes)
		{
			return attributes.Data;
		}

		/// <summary>
		/// Compares two basic material attributes for equality
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(BasicMaterialAttributes left, BasicMaterialAttributes right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two basic material attributes for inequality
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(BasicMaterialAttributes left, BasicMaterialAttributes right)
		{
			return !(left == right);
		}
	}
}
