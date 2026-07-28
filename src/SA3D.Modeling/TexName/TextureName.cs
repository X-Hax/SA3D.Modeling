using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.Ascii;
using SA3D.Common.Converters;
using SA3D.Common.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.TexName
{
	/// <summary>
	/// Stores a texture name and its attributes
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class TextureName : IBinarySerializable, IAsciiSerializable
	{
		private class JsonConverter : SimpleJsonObjectConverter<TextureName>
		{
			private const string _name = nameof(Name);
			private const string _attributes = nameof(Attributes);
			private const string _textureAddress = nameof(TextureAddress);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _name, new(PropertyTokenType.String, null) },
				{ _attributes, new(PropertyTokenType.String, 0u) },
				{ _textureAddress, new(PropertyTokenType.String, 0u) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _name:
						return reader.GetString();
					case _attributes:
					case _textureAddress:
						return UInt32HexConverter.ConvertFrom(reader.GetString()!, propertyName);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override TextureName Create(ReadOnlyDictionary<string, object?> values)
			{
				string? name = (string?)values[_name];
				uint attributes = (uint)values[_attributes]!;
				uint textureAddress = (uint)values[_textureAddress]!;

				return new(name, attributes, textureAddress);
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, TextureName value, JsonSerializerOptions options)
			{
				writer.WriteString(_name, value.Name);

				if(value.Attributes != 0)
				{
					writer.WriteString(_attributes, UInt32HexConverter.ConvertTo(value.Attributes));
				}

				if(value.TextureAddress != 0)
				{
					writer.WriteString(_textureAddress, UInt32HexConverter.ConvertTo(value.TextureAddress));
				}
			}
		}

		/// <summary>
		/// The texture name.
		/// </summary>
		public string? Name { get; set; }

		/// <summary>
		/// Attributes.
		/// </summary>
		public uint Attributes { get; private set; }

		/// <summary>
		/// Texture address.
		/// </summary>
		public uint TextureAddress { get; set; }


		/// <summary>
		/// Creates a new texture name.
		/// </summary>
		/// <param name="name">The texture name.</param>
		/// <param name="attributes">Attributes.</param>
		/// <param name="textureAddress">Texture address.</param>
		public TextureName(string? name, uint attributes, uint textureAddress)
		{
			Name = name;
			Attributes = attributes;
			TextureAddress = textureAddress;
		}

		/// <summary>
		/// Creates a new, empty texture name
		/// </summary>
		public TextureName() : this(null, 0, 0) { }


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			Name = reader.ReadStringOffset(StringBinaryFormat.NullTerminated);
			Attributes = reader.ReadUInt32();
			TextureAddress = reader.ReadUInt32();
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer)
		{
			writer.WriteStringOffset(StringBinaryFormat.NullTerminated, Name, alignment: 4);
			writer.WriteUInt32(Attributes);
			writer.WriteUInt32(TextureAddress);
		}

		/// <inheritdoc/>
		public void Write(AsciiWriter writer)
		{
			writer.WriteLine($"\tTEXN( \"{Name}\" ),");
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Name ?? "!NULL";
		}
	}
}
