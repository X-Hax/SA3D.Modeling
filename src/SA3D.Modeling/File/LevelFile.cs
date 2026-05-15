using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Common.IO;
using SA3D.Modeling.File.MetaData;
using SA3D.Modeling.File.MetaData.Blocks;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using static SA3D.Modeling.File.FileHeaders;

namespace SA3D.Modeling.File
{
	/// <summary>
	/// Level geometry file contents.
	/// </summary>
	public class LevelFile : IFile
	{
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
		public void Read(BinaryObjectReader reader)
		{
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
				PointerLUT = new(labels)
			};

			Level = reader.ReadObjectOffset<Level, IOContext>(context, context.PointerLUT)
				?? throw reader.ReadNullReference(nameof(LevelFile), nameof(Level));
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer)
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
				PointerLUT = new()
			};

			writer.WriteObjectOffset(Level, context);

			MetaData.ReplaceLabels(context.PointerLUT.Labels);
			writer.WriteObject(MetaData);
		}
	}
}