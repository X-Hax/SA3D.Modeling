using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using J113D.Json;
using SA3D.Common.IO;
using SA3D.Modeling.File.MetaData;
using SA3D.Modeling.File.MetaData.Blocks;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using static SA3D.Modeling.File.FileHeaders;

namespace SA3D.Modeling.File
{
	/// <summary>
	/// Level geometry file contents.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class LevelFile : IFileSerializable
	{
		private class JsonConverter : SimpleJsonObjectConverter<LevelFile>
		{
			private const string _level = nameof(Level);
			private const string _metaData = nameof(MetaData);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _level, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _metaData, new(PropertyTokenType.Array, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _level:
						return JsonSerializer.Deserialize<Level>(ref reader, options);
					case _metaData:
						return JsonSerializer.Deserialize<MetaDataBlocks>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override LevelFile Create(ReadOnlyDictionary<string, object?> values)
			{
				Level level = (Level?)values[_level]
					?? throw new InvalidDataException($"Levelfile requires \"{_level}\" property");

				return new(level)
				{
					MetaData = (MetaDataBlocks?)values[_metaData] ?? new()
				};
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, LevelFile value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_metaData);
				JsonSerializer.Serialize(writer, value.MetaData, options);

				writer.WritePropertyName(_level);
				JsonSerializer.Serialize(writer, value.Level, options);
			}
		}

		/// <summary>
		/// Original filepath
		/// </summary>
		public string? Filepath { get; set; }

		/// <summary>
		/// Landtable of the file.
		/// </summary>
		public Level Level { get; set; }

		/// <summary>
		/// MetaData in the file.
		/// </summary>
		public MetaDataBlocks MetaData { get; set; }


		/// <summary>
		/// Creates a blank level file
		/// </summary>
		public LevelFile() : this(new()) { }

		/// <summary>
		/// Creates a level file for a level.
		/// </summary>
		/// <param name="level">Level of the file.</param>
		public LevelFile(Level level)
		{
			Level = level;
			MetaData = new();
		}


		/// <inheritdoc/>
		public bool Check(BinaryObjectReader reader)
		{
			using SeekToken seekToken = reader.At();
			using EndiannessToken endiannessToken = reader.WithEndian(Endianness.Little);

			return (reader.ReadUInt64() & HeaderMask) switch
			{
				SA1LVL or SADXLVL or SA2LVL or SA2BLVL => true,
				_ => false,
			};
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, FileContext fileContext)
		{
			Filepath = fileContext.Filepath;

			using EndiannessToken endiannessToken = reader.WithEndian(Endianness.Little);

			ulong headerVersion = reader.ReadUInt64();

			Format format = (headerVersion & HeaderMask) switch
			{
				SA1LVL => Format.Basic,
				SADXLVL => Format.BasicDX,
				SA2LVL => Format.Chunk,
				SA2BLVL => Format.Ginja,
				_ => throw new FormatException("File invalid; Header malformed"),
			};

			byte version = (byte)(headerVersion >> 56);
			if(version > CurrentLandtableVersion)
			{
				throw new FormatException($"File invalid; Unsupported version {version}; Maximum supported version: {CurrentLandtableVersion}");
			}

			using(reader.At(4, SeekOrigin.Current))
			{
				MetaDataIOContext metaDataContext = new()
				{
					Version = version
				};

				MetaData = reader.ReadObject<MetaDataBlocks, MetaDataIOContext>(metaDataContext);
			}

			Dictionary<long, string> labels = [];
			if(MetaData.TryGetBlock(out LabelsMetaDataBlock? labelBlock))
			{
				labels = labelBlock!.Labels.GetDictFrom();
			}

			IOContext context = new()
			{
				LevelFormat = format,
				MeshFormat = format,
				OffsetLUT = new(labels)
			};

			Level = reader.ReadObjectOffset<Level, IOContext>(context, context.OffsetLUT)
				?? throw reader.ReadNullReference(nameof(LevelFile), nameof(Level));
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, FileContext fileContext)
		{
			ulong header = Level.Format switch
			{
				Format.Basic => SA1LVLVer,
				Format.BasicDX => SADXLVLVer,
				Format.Chunk => SA2LVLVer,
				Format.Ginja => SA2BLVLVer,
				_ => throw new ArgumentException($"Level format {Level.Format} not supported for SALVL files"),
			};

			writer.WriteUInt64(header);

			IOContext context = new()
			{
				MeshFormat = Level.Format,
				LevelFormat = Level.Format,
				OffsetLUT = new()
			};

			writer.WriteObjectOffset(Level, context, context.OffsetLUT);
			MetaData.Write(writer, context.OffsetLUT.Labels, null, null);
		}
	}
}