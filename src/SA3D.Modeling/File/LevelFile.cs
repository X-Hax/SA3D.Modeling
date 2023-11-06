using SA3D.Common.IO;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.Structs;
using System;
using System.IO;
using static SA3D.Modeling.File.FileHeaders;

namespace SA3D.Modeling.File
{
	/// <summary>
	/// Level geometry file contents.
	/// </summary>
	public class LevelFile
	{
		/// <summary>
		/// Landtable of the file.
		/// </summary>
		public LandTable Level { get; }

		/// <summary>
		/// MetaData of/for a LVL file
		/// </summary>
		public MetaData MetaData { get; }


		private LevelFile(LandTable level, MetaData metaData)
		{
			Level = level;
			MetaData = metaData;
		}


		/// <summary>
		/// Checks whether data is formatted as a level file.
		/// </summary>
		/// <param name="data">The data to check.</param>
		public static bool CheckIsLevelFile(byte[] data)
		{
			return CheckIsLevelFile(data, 0);
		}

		/// <summary>
		/// Checks whether data is formatted as a level file.
		/// </summary>
		/// <param name="data">The data to check.</param>
		/// <param name="address">Address at which to check.</param>
		public static bool CheckIsLevelFile(byte[] data, uint address)
		{
			return CheckIsLevelFile(new EndianStackReader(data), address);
		}

		/// <summary>
		/// Checks whether data is formatted as a level file.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		public static bool CheckIsLevelFile(EndianStackReader reader)
		{
			return CheckIsLevelFile(reader, 0);
		}

		/// <summary>
		/// Checks whether data is formatted as a level file.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to check.</param>
		public static bool CheckIsLevelFile(EndianStackReader reader, uint address)
		{
			switch(reader.ReadULong(address) & HeaderMask)
			{
				case SA1LVL:
				case SADXLVL:
				case SA2LVL:
				case SA2BLVL:
				case BUFLVL:
					break;
				default:
					return false;
			}

			return reader[address + 7] <= CurrentLandtableVersion;
		}


		/// <summary>
		/// Reads a level file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		/// <returns>The level file that was read.</returns>
		public static LevelFile ReadFromFile(string filepath)
		{
			return ReadFromData(System.IO.File.ReadAllBytes(filepath));
		}

		/// <summary>
		/// Reads a level file off byte data.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <returns>The level file that was read.</returns>
		public static LevelFile ReadFromData(byte[] data)
		{
			return ReadFromData(data, 0);
		}

		/// <summary>
		/// Reads a level file off byte data.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <returns>The level file that was read.</returns>
		public static LevelFile ReadFromData(byte[] data, uint address)
		{
			using(EndianStackReader reader = new(data))
			{
				return Read(reader, address);
			}
		}

		/// <summary>
		/// Reads a level file off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <returns>The level file that was read.</returns>
		public static LevelFile Read(EndianStackReader reader)
		{
			return Read(reader, 0);
		}

		/// <summary>
		/// Reads a level file off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <returns>The level file that was read.</returns>
		public static LevelFile Read(EndianStackReader reader, uint address)
		{
			reader.PushBigEndian(false);

			try
			{
				ulong header = reader.ReadULong(0) & HeaderMask;
				byte version = reader[7];

				ModelFormat format = header switch
				{
					SA1LVL => ModelFormat.SA1,
					SADXLVL => ModelFormat.SADX,
					SA2LVL => ModelFormat.SA2,
					SA2BLVL => ModelFormat.SA2B,
					BUFLVL => ModelFormat.Buffer,
					_ => throw new FormatException("File invalid; Header malformed"),
				};

				if(version > CurrentLandtableVersion)
				{
					throw new FormatException("File invalid; Version not supported");
				}

				MetaData metaData = MetaData.Read(reader, address + 0xC, version, false);
				PointerLUT lut = new(metaData.Labels);

				uint ltblAddress = reader.ReadUInt(address + 8);
				LandTable table = LandTable.Read(reader, ltblAddress, format, lut);

				return new(table, metaData);
			}
			finally
			{
				reader.PopEndian();
			}
		}


		/// <summary>
		/// Write the level file to a file. Previous labels may get lost.
		/// </summary>
		/// <param name="filepath">Path to the file to write to.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public void WriteToFile(string filepath)
		{
			WriteToFile(filepath, Level, MetaData);
		}

		/// <summary>
		/// Writes the level file to a byte array. Previous labels may get lost.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public byte[] WriteToData()
		{
			return WriteToData(Level, MetaData);
		}

		/// <summary>
		/// Writes the level file to an endian stack writer. Previous labels may get lost.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public void Write(EndianStackWriter writer)
		{
			Write(writer, Level, MetaData);
		}


		/// <summary>
		/// Write a level file to a file.
		/// </summary>
		/// <param name="filepath">Path to the file to write to.</param>
		/// <param name="level">The level to write.</param>
		/// <param name="metaData">The metadata to include.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public static void WriteToFile(string filepath, LandTable level, MetaData? metaData = null)
		{
			using(FileStream stream = System.IO.File.Create(filepath))
			{
				EndianStackWriter writer = new(stream);
				Write(writer, level, metaData);
			}
		}

		/// <summary>
		/// Writes a level file to a byte array.
		/// </summary>
		/// <param name="level">The level to write.</param>
		/// <param name="metaData">The metadata to include.</param>
		/// <returns>The written byte data.</returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static byte[] WriteToData(LandTable level, MetaData? metaData = null)
		{
			using(MemoryStream stream = new())
			{
				EndianStackWriter writer = new(stream);
				Write(writer, level, metaData);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Writes a level file to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="level">The level to write.</param>
		/// <param name="metaData">The metadata to include.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public static void Write(EndianStackWriter writer, LandTable level, MetaData? metaData = null)
		{
			// writing indicator
			switch(level.Format)
			{
				case ModelFormat.SA1:
					writer.WriteULong(SA1LVLVer);
					break;
				case ModelFormat.SADX:
					writer.WriteULong(SADXLVLVer);
					break;
				case ModelFormat.SA2:
					writer.WriteULong(SA2LVLVer);
					break;
				case ModelFormat.SA2B:
					writer.WriteULong(SA2BLVLVer);
					break;
				case ModelFormat.Buffer:
					writer.WriteULong(BUFLVLVer);
					break;
				default:
					break;
			}

			uint placeholderAddr = writer.Position;
			// 4 bytes: landtable address placeholder
			// 4 bytes: metadata placeholder
			writer.WriteEmpty(8);

			PointerLUT lut = new();

			uint ltblAddress = level.Write(writer, lut);

			metaData ??= new();
			metaData.Labels = lut.Labels.GetDictFrom();
			uint metaDataAddress = metaData.Write(writer);

			uint end = writer.Position;
			writer.Seek(placeholderAddr, SeekOrigin.Begin);
			writer.WriteUInt(ltblAddress);
			writer.WriteUInt(metaDataAddress);
			writer.Seek(end, SeekOrigin.Begin);
		}

	}
}
