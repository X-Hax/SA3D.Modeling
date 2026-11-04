using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.Ascii;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Basic
{
	/// <summary>
	/// BASIC format material
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct BasicMaterial : IBinarySerializable, IAsciiSerializable
	{
		private class JsonConverter : SimpleJsonObjectConverter<BasicMaterial>
		{
			private const string _diffuseColor = nameof(DiffuseColor);
			private const string _specularColor = nameof(SpecularColor);
			private const string _specularExponent = nameof(SpecularExponent);
			private const string _textureID = nameof(TextureID);
			private const string _textureAttributes = nameof(TextureAttributes);
			private const string _attributes = nameof(Attributes);

			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _diffuseColor, new(PropertyTokenType.String, DefaultValues.DiffuseColor ) },
				{ _specularColor, new(PropertyTokenType.String, DefaultValues.SpecularColor) },
				{ _specularExponent, new(PropertyTokenType.Number, DefaultValues.SpecularExponent) },
				{ _textureID, new(PropertyTokenType.Number, DefaultValues.TextureID) },
				{ _textureAttributes, new(PropertyTokenType.Number, DefaultValues.TextureAttributes) },
				{ _attributes, new(PropertyTokenType.Number, DefaultValues.Attributes) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				return propertyName switch
				{
					_diffuseColor
					or _specularColor => JsonSerializer.Deserialize<Color>(ref reader, options),

					_textureID => reader.GetUInt32(),
					_textureAttributes => reader.GetByte(),

					_attributes => JsonSerializer.Deserialize<BasicMaterialAttributes>(ref reader, options),

					_ => throw new InvalidPropertyException(),
				};
			}

			/// <inheritdoc/>
			protected override BasicMaterial Create(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					DiffuseColor = (Color)values[_diffuseColor]!,
					SpecularColor = (Color)values[_specularColor]!,
					SpecularExponent = (float)values[_specularExponent]!,
					TextureID = (uint)values[_textureID]!,
					TextureAttributes = (byte)values[_textureAttributes]!,
					Attributes = (BasicMaterialAttributes)values[_attributes]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, BasicMaterial value, JsonSerializerOptions options)
			{
				void serialize<T>(string name, T value, T def) where T : notnull
				{
					if(!value.Equals(def))
					{
						writer.WritePropertyName(name);
						JsonSerializer.Serialize<T>(writer, value, options);
					}
				}


				serialize(_diffuseColor, value.DiffuseColor, DefaultValues.DiffuseColor);
				serialize(_specularColor, value.SpecularColor, DefaultValues.SpecularColor);

				if(value.SpecularExponent != DefaultValues.SpecularExponent)
				{
					writer.WriteNumber(_specularExponent, value.SpecularExponent);
				}

				if(value.TextureID != DefaultValues.TextureID)
				{
					writer.WriteNumber(_textureID, value.TextureID);
				}

				if(value.TextureAttributes != DefaultValues.TextureAttributes)
				{
					writer.WriteNumber(_textureAttributes, value.TextureAttributes);
				}

				writer.WritePropertyName(_attributes);
				JsonSerializer.Serialize(writer, value.Attributes, options);
			}
		}

		/// <summary>
		/// Material with default values.
		/// </summary>
		public static readonly BasicMaterial DefaultValues = new()
		{
			DiffuseColor = Color.ColorWhite,
			SpecularColor = Color.ColorWhite,
			SpecularExponent = 11,
			Attributes = BasicMaterialAttributes.DefaultValues
		};

		/// <summary>
		/// Diffuse color.
		/// </summary>
		public Color DiffuseColor { get; set; }

		/// <summary>
		/// Specular color.
		/// </summary>
		public Color SpecularColor { get; set; }

		/// <summary>
		/// Specular exponent.
		/// </summary>
		public float SpecularExponent { get; set; }

		/// <summary>
		/// Texture attributes (unused) and ID
		/// </summary>
		public uint TextureData { get; set; }

		/// <summary>
		/// Attributes containing various information.
		/// </summary>
		public BasicMaterialAttributes Attributes { get; set; }


		#region Texture data

		/// <summary>
		/// Texture ID.
		/// </summary>
		public uint TextureID
		{
			readonly get => TextureData & 0x1FFFFFFFu;
			set => TextureData = (TextureData & ~0x1FFFFFFFu) | (value & 0x1FFFFFFFu);
		}

		/// <summary>
		/// Texture attributes (3 bits, unused)
		/// </summary>
		public byte TextureAttributes
		{
			readonly get => (byte)(TextureData >> 29);
			set => TextureData = (TextureData & 0x1FFFFFFFu) | ((uint)value << 29);
		}

		#endregion


		void IBinarySerializable.Read(BinaryObjectReader reader)
		{
			DiffuseColor = reader.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_32);
			SpecularColor = reader.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_32);
			SpecularExponent = reader.ReadSingle();
			TextureID = reader.ReadUInt32();
			Attributes = reader.ReadUInt32();
		}

		readonly void IBinarySerializable.Write(BinaryObjectWriter writer)
		{
			writer.WriteObject(DiffuseColor, ColorIOType.ARGB8_32);
			writer.WriteObject(SpecularColor, ColorIOType.ARGB8_32);
			writer.WriteSingle(SpecularExponent);
			writer.WriteUInt32(TextureID);
			writer.WriteUInt32(Attributes);
		}

		readonly void IAsciiSerializable.Write(AsciiWriter writer)
		{
			using(writer.WriteBlock("MAT"))
			{
				writer.WritePropertyLine("Diffuse", $"( {DiffuseColor.Alpha}, {DiffuseColor.Red}, {DiffuseColor.Green}, {DiffuseColor.Blue} )");
				writer.WritePropertyLine("Specular", $"( {SpecularColor.Alpha}, {SpecularColor.Red}, {SpecularColor.Green}, {SpecularColor.Blue} )");
				writer.WritePropertyLine("Exponent", $"( {SpecularExponent.ToAscii()} )");
				writer.WritePropertyLine("AttrTexId", $"( {(TextureData & ~0x1FFFFFFFu).ToAsciiHex()}, {TextureID} )");
				writer.WritePropertyLine("AttrFlags", $"( {Attributes.Data.ToAsciiHex()} )");
			}
		}


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is BasicMaterial material &&
				   DiffuseColor == material.DiffuseColor &&
				   SpecularColor == material.SpecularColor &&
				   SpecularExponent == material.SpecularExponent &&
				   TextureID == material.TextureID &&
				   Attributes == material.Attributes;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(DiffuseColor, SpecularColor, SpecularExponent, TextureID, Attributes);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Texture: {TextureID} / Use Alpha: {Attributes.UseAlpha}";
		}

		/// <summary>
		/// Compares two materials for equality.
		/// </summary>
		/// <param name="left">Lefthand material</param>
		/// <param name="right">Righthand material</param>
		/// <returns>Whether the materials are equal.</returns>
		public static bool operator ==(BasicMaterial left, BasicMaterial right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two materials for inequality.
		/// </summary>
		/// <param name="left">Lefthand material</param>
		/// <param name="right">Righthand material</param>
		/// <returns>Whether the materials are inequal.</returns>
		public static bool operator !=(BasicMaterial left, BasicMaterial right)
		{
			return !(left == right);
		}
	}
}
