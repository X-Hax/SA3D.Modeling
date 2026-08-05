using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common;
using SA3D.Common.Ascii;
using SA3D.Common.Ini;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.TexName
{
	/// <summary>
	/// Stores a texture name list.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class TextureNameList : ILabel, IBinarySerializable<OffsetLUT>, IFileSerializable, IAsciiSerializable
	{
		private class JsonConverter : SimpleJsonObjectConverter<TextureNameList>
		{
			private const string _label = nameof(Label);
			private const string _textureNames = nameof(TextureNames);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _label, new(PropertyTokenType.String, string.Empty) },
				{ _textureNames, new(PropertyTokenType.Object | PropertyTokenType.Array, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _label:
						return reader.GetString();
					case _textureNames:
						return JsonSerializer.Deserialize<LabeledArray<TextureName>>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override TextureNameList Create(ReadOnlyDictionary<string, object?> values)
			{
				string label = (string)values[_label]!;
				LabeledArray<TextureName> textureNames = new(0);

				if(values[_textureNames] is LabeledArray<TextureName> readTextureNames)
				{
					textureNames = readTextureNames;
				}

				return new(label, textureNames);
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, TextureNameList value, JsonSerializerOptions options)
			{
				writer.WriteString(_label, value.Label);

				writer.WritePropertyName(_textureNames);
				JsonSerializer.Serialize(writer, value.TextureNames, options);
			}
		}

		private const string _labelPrefix = "texlist_";
		private const string _texturesLabelPrefix = "textures_";

		/// <inheritdoc/>
		public string Label { get; set; }

		/// <inheritdoc/>
		public string LabelPrefix => _labelPrefix;

		/// <summary>
		/// Texture names.
		/// </summary>
		public LabeledArray<TextureName> TextureNames { get; set; }

		/// <summary>
		/// Creates a new, empty texture list
		/// </summary>
		public TextureNameList() : this(
			_labelPrefix.GenerateIdentifier(),
			new LabeledArray<TextureName>(
				_texturesLabelPrefix.GenerateIdentifier(),
				[]
			)
		)
		{ }

		/// <summary>
		/// Creates a new texture name list.
		/// </summary>
		/// <param name="label">Texture list label.</param>
		/// <param name="textureNames">Texture names.</param>
		public TextureNameList(string label, LabeledArray<TextureName> textureNames)
		{
			Label = label;
			TextureNames = textureNames;
		}


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, OffsetLUT lut)
		{
			long texturesOffset = reader.ReadOffsetValue();
			int texturesCount = reader.ReadInt32();

			TextureNames = reader.ReadLabeledObjectArrayAtOffset<TextureName>(texturesOffset, texturesCount, _texturesLabelPrefix, lut)
				?? throw new NullReferenceException($"Texture list has no texture names array (0x{reader.Position})!");
		}

		/// <summary>
		/// Reads a texture name list from from an Ini or Satex file.
		/// </summary>
		/// <param name="filepath">The path to the file.</param>
		/// <returns>The read texture name list.</returns>
		/// <exception cref="FormatException"></exception>
		public static TextureNameList ReadFromTextFile(string filepath)
		{
			string[] lines = System.IO.File.ReadAllLines(filepath);

			if(lines.Length > 0 && lines[0].Contains('='))
			{
				IniTexturenameList ini = IniSerializer.DeserializeFromFile<IniTexturenameList>(filepath)
										?? throw new FormatException("File not correctly formated as an Ini");

				TextureName[] textureNames = new TextureName[ini.NumTextures];
				for(int i = 0; i < textureNames.Length; i++)
				{
					textureNames[i] = new(ini.TextureNames[i], 0, 0);
				}

				return new(ini.Name, new LabeledArray<TextureName>(ini.TexnameArrayName, textureNames));
			}
			else
			{
				TextureName[] textureNames = new TextureName[lines.Length];

				for(int i = 0; i < lines.Length; i++)
				{
					textureNames[i] = new TextureName(Path.GetFileNameWithoutExtension(lines[i]), 0, 0);
				}

				return new TextureNameList(string.Empty, new LabeledArray<TextureName>(textureNames));
			}
		}


		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, OffsetLUT lut)
		{
			writer.WriteObjectArrayOffset(TextureNames, lut);
			writer.WriteInt32(TextureNames.Length);
		}

		/// <inheritdoc/>
		public void Write(AsciiWriter writer)
		{
			using(writer.WriteObjectBlock("TEXTURE_"))
			{
				writer.WriteArray("TEXTURENAME", TextureNames, 0);

				using(writer.WriteStructBlock("TEXTURELIST", this))
				{
					writer.WriteObjectPropertyLine($"TextureList", TextureNames);
					writer.WritePropertyLine("TextureNum", TextureNames.Length.ToString());
				}
			}
		}


		/// <summary>
		/// Saves the texture list as a plain text document.
		/// </summary>
		/// <param name="filePath">The path to write the file to.</param>
		/// <param name="extension">The file extension to add to every texture name. without dot.</param>
		public void WriteAsListToTextFile(string filePath, string extension = "pvr")
		{
			string lines = string.Empty;
			foreach(TextureName texName in TextureNames)
			{
				lines += (texName.Name ?? "empty") + $".{extension}\n";
			}

			System.IO.File.WriteAllText(filePath, lines);
		}

		/// <summary>
		/// Writes the texture name list to an Ini/Satex file.
		/// </summary>
		/// <param name="filepath">The path to write the file to.</param>
		public void WriteAsIniToTextFile(string filepath)
		{
			string[] textureNames = TextureNames.Select(x => x.Name ?? "NULL").ToArray();
			IniTexturenameList ini = new(Label, TextureNames.Label, (uint)textureNames.Length, textureNames);

			IniSerializer.SerializeToFile(ini, filepath);
		}

		/// <summary>
		/// Writes the texture list as a C compilable struct.
		/// </summary>
		/// <param name="writer">the text writer to write it to</param>
		/// <param name="labels">Used labels</param>
		public void WriteAsStruct(TextWriter writer, List<string>? labels = null)
		{
			labels ??= [];

			if(labels.Contains(TextureNames.Label))
			{
				writer.WriteLine($"NJS_TEXNAME {TextureNames.Label}[] =");
				writer.WriteLine("{");
				for(int i = 0; i < TextureNames.Length; i++)
				{
					writer.Write($"\t{{ \"{TextureNames[i].Name}\" }}");
					if(i < TextureNames.Length - 1)
					{
						writer.Write(',');
					}

					writer.WriteLine();
				}

				writer.WriteLine("};");
				labels.Add(TextureNames.Label);
			}

			if(labels.Contains(Label))
			{
				writer.WriteLine($"NjsTexList {Label}[] = {{ arrayptrandlength ({TextureNames.Label}) }};");
				labels.Add(Label);
			}
		}



		/// <inheritdoc/>
		void IBinarySerializable.Read(BinaryObjectReader reader)
		{
			Read(reader, new OffsetLUT());
		}

		/// <inheritdoc/>
		void IBinarySerializable.Write(BinaryObjectWriter writer)
		{
			Write(writer, new OffsetLUT());
		}

	}
}
