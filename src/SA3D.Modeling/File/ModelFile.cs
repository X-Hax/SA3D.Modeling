using SA3D.Modeling.Mesh;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;
using SA3D.Common.IO;
using System.IO;
using System;
using static SA3D.Modeling.File.FileHeaders;

namespace SA3D.Modeling.File
{
	/// <summary>
	/// Node model with attach data file contents.
	/// </summary>
	public class ModelFile
	{
		/// <summary>
		/// Whether the file is an NJ binary
		/// </summary>
		public bool NJFile { get; }

		/// <summary>
		/// Attach format of the file
		/// </summary>
		public ModelFormat Format { get; }

		/// <summary>
		/// Hierarchy tip of the file
		/// </summary>
		public Node Model { get; }

		/// <summary>
		/// Meta data of the file
		/// </summary>
		public MetaData MetaData { get; }


		private ModelFile(ModelFormat format, Node model, MetaData metaData, bool nj)
		{
			Format = format;
			Model = model;
			MetaData = metaData;
			NJFile = nj;
		}


		/// <summary>
		/// Checks whether data is formatted as a model file.
		/// </summary>
		/// <param name="data">The data to check.</param>
		public static bool CheckIsModelFile(byte[] data)
		{
			return CheckIsModelFile(data, 0);
		}

		/// <summary>
		/// Checks whether data is formatted as a model file.
		/// </summary>
		/// <param name="data">The data to check.</param>
		/// <param name="address">Address at which to check.</param>
		public static bool CheckIsModelFile(byte[] data, uint address)
		{
			return CheckIsModelFile(new EndianStackReader(data), address);
		}

		/// <summary>
		/// Checks whether data is formatted as a model file.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		public static bool CheckIsModelFile(EndianStackReader reader)
		{
			return CheckIsModelFile(reader, 0);
		}

		/// <summary>
		/// Checks whether data is formatted as a model file.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to check.</param>
		public static bool CheckIsModelFile(EndianStackReader reader, uint address)
		{
			try
			{
				_ = GetNJModelBlockAddress(reader, address);
				return true;
			}
			catch(FormatException) { }

			switch(reader.ReadULong(address) & HeaderMask)
			{
				case SA1MDL:
				case SADXMDL:
				case SA2MDL:
				case SA2BMDL:
				case BUFMDL:
					break;
				default:
					return false;
			}

			return reader[address + 7] <= CurrentModelVersion;
		}


		/// <summary>
		/// Reads a model file.
		/// </summary>
		/// <param name="filepath">The path to the file that should be read.</param>
		/// <returns>The model file that was read.</returns>
		public static ModelFile ReadFromFile(string filepath)
		{
			return ReadFromBytes(System.IO.File.ReadAllBytes(filepath));
		}

		/// <summary>
		/// Reads a model file off byte data.
		/// </summary>
		/// <param name="data">Data to read.</param>
		/// <returns>The model file that was read.</returns>
		public static ModelFile ReadFromBytes(byte[] data)
		{
			return ReadFromBytes(data, 0);
		}

		/// <summary>
		/// Reads a model file off byte data.
		/// </summary>
		/// <param name="data">The data to read from.</param>
		/// <param name="address">The address at which to start reading.</param>
		/// <returns>The model file that was read.</returns>
		public static ModelFile ReadFromBytes(byte[] data, uint address)
		{
			using(EndianStackReader reader = new(data))
			{
				return Read(reader, address);
			}
		}

		/// <summary>
		/// Reads a model file off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <returns>The model file that was read.</returns>
		public static ModelFile Read(EndianStackReader reader)
		{
			return Read(reader, 0);
		}

		/// <summary>
		/// Reads a model file off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <returns>The model file that was read.</returns>
		public static ModelFile Read(EndianStackReader reader, uint address)
		{
			reader.PushBigEndian(false);

			try
			{
				return reader.ReadUShort(address) is NJ or GJ
					? ReadNJ(reader, address)
					: ReadSA(reader, address);
			}
			finally
			{
				reader.PopEndian();
			}
		}


		private static uint GetNJModelBlockAddress(EndianStackReader reader, uint address)
		{
			uint blockAddress = address;
			while(address < reader.Length + 8)
			{
				ushort njHeader = reader.ReadUShort(blockAddress);

				if(njHeader is not NJ or GJ)
				{
					throw new FormatException("Malformatted NJ data.");
				}

				ushort blockHeader = reader.ReadUShort(blockAddress + 2);

				if(blockHeader is BM or CM)
				{
					return blockAddress;
				}

				uint blockSize = reader.ReadUInt(blockAddress + 4);
				blockAddress += 8 + blockSize;
			}

			throw new FormatException("No model block found");
		}

		private static ModelFile ReadNJ(EndianStackReader reader, uint address)
		{
			uint blockAddress = GetNJModelBlockAddress(reader, address);
			ushort blockHeader = reader.ReadUShort(blockAddress + 2);

			uint modelAddress = blockAddress + 8;
			bool fileEndian = reader.CheckBigEndian32(modelAddress);
			reader.PushBigEndian(fileEndian);

			reader.ImageBase = unchecked((uint)-modelAddress);

			ModelFormat format = blockHeader switch
			{
				BM => ModelFormat.SA1,
				CM => ModelFormat.SA2,
				_ => throw new FormatException()
			};

			Node model = Node.Read(reader, modelAddress, format, new());

			return new(format, model, new(), true);
		}

		private static ModelFile ReadSA(EndianStackReader reader, uint address)
		{
			ulong header8 = reader.ReadULong(address) & HeaderMask;
			ModelFormat format = header8 switch
			{
				SA1MDL => ModelFormat.SA1,
				SADXMDL => ModelFormat.SADX,
				SA2MDL => ModelFormat.SA2,
				SA2BMDL => ModelFormat.SA2B,
				BUFMDL => ModelFormat.Buffer,
				_ => throw new FormatException("File invalid; Header malformed"),
			};

			// checking the version
			byte version = reader[address + 7];
			if(version > CurrentModelVersion)
			{
				throw new FormatException("File invalid; Unsupported version");
			}

			MetaData metaData = MetaData.Read(reader, address + 0xC, version, true);
			PointerLUT lut = new(metaData.Labels);

			uint prevImageBase = reader.ImageBase;
			if(address != 0)
			{
				reader.ImageBase = unchecked((uint)-address);
			}

			uint modelAddr = reader.ReadPointer(address + 8);
			Node model = Node.Read(reader, modelAddr, format, lut);

			reader.ImageBase = prevImageBase;

			return new(format, model, metaData, false);
		}


		/// <summary>
		/// Write the model file to a file. Previous labels may get lost.
		/// </summary>
		/// <param name="filepath">Path to the file to write to.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public void WriteToFile(string filepath)
		{
			WriteToFile(filepath, Model, NJFile, MetaData, Format);
		}

		/// <summary>
		/// Writes the model file to a byte array. Previous labels may get lost.
		/// </summary>
		/// <exception cref="InvalidOperationException"></exception>
		/// <returns>The written byte data.</returns>
		public byte[] WriteToBytes()
		{
			return WriteToBytes(Model, NJFile, MetaData, Format);
		}

		/// <summary>
		/// Writes the model file to an endian stack writer. Previous labels may get lost.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public void Write(EndianStackWriter writer)
		{
			Write(writer, Model, NJFile, MetaData, Format);
		}


		/// <summary>
		/// Write a model file to a file.
		/// </summary>
		/// <param name="filepath">Path to the file to write to.</param>
		/// <param name="model">The model to write.</param>
		/// <param name="nj">Whether to format as an NJ file.</param>
		/// <param name="metaData">The metadata to include.</param>
		/// <param name="format">The format to write in.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public static void WriteToFile(string filepath, Node model, bool nj = false, MetaData? metaData = null, ModelFormat? format = null)
		{
			using(FileStream stream = System.IO.File.Create(filepath))
			{
				EndianStackWriter writer = new(stream);
				Write(writer, model, nj, metaData, format);
			}
		}

		/// <summary>
		/// Writes a model file to a byte array.
		/// </summary>
		/// <param name="model">The model to write.</param>
		/// <param name="nj">Whether to format as an NJ file.</param>
		/// <param name="metaData">The metadata to include.</param>
		/// <param name="format">The format to write in.</param>
		/// <exception cref="InvalidOperationException"></exception>
		/// <returns>The written byte data.</returns>
		public static byte[] WriteToBytes(Node model, bool nj = false, MetaData? metaData = null, ModelFormat? format = null)
		{
			using(MemoryStream stream = new())
			{
				EndianStackWriter writer = new(stream);
				Write(writer, model, nj, metaData, format);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Writes a model file to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="model">The model to write.</param>
		/// <param name="nj">Whether to format as an NJ file.</param>
		/// <param name="metaData">The metadata to include.</param>
		/// <param name="format">The format to write in.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public static void Write(EndianStackWriter writer, Node model, bool nj = false, MetaData? metaData = null, ModelFormat? format = null)
		{
			format ??= model.GetAttachFormat() switch
			{
				AttachFormat.Buffer => ModelFormat.Buffer,
				AttachFormat.BASIC => ModelFormat.SA1,
				AttachFormat.CHUNK => ModelFormat.SA2,
				AttachFormat.GC => ModelFormat.SA2B,
				_ => throw new InvalidOperationException(),
			};

			if(nj)
			{
				WriteNJ(writer, model, format.Value);
			}
			else
			{
				WriteSA(writer, model, format.Value, metaData);
			}
		}

		private static void WriteNJ(EndianStackWriter writer, Node model, ModelFormat format)
		{
			writer.WriteUShort(NJ);
			switch(format)
			{
				case ModelFormat.SA1 or ModelFormat.SADX:
					writer.WriteUShort(BM);
					break;
				case ModelFormat.SA2:
					writer.WriteUShort(CM);
					break;
				default:
					throw new ArgumentException($"Attach format {format} not supported for NJ binaries");
			}

			uint placeholderAddress = writer.Position;
			writer.WriteEmpty(4); // file length placeholder

			uint nodeStart = writer.Position;
			writer.ImageBase = unchecked((uint)-nodeStart);
			writer.WriteEmpty(Node.StructSize);

			PointerLUT lut = new();

			model.Child?.Write(writer, format, lut);
			model.Next?.Write(writer, format, lut);
			model.Attach?.Write(writer, format, lut);

			uint byteSize = writer.Position - nodeStart;

			// write the root node to the start
			uint end = writer.Position;
			writer.Seek(placeholderAddress, SeekOrigin.Begin);
			writer.WriteUInt(byteSize);
			model.Write(writer, format, lut);

			// replace size
			writer.Seek(end, SeekOrigin.Begin);
		}

		private static void WriteSA(EndianStackWriter writer, Node model, ModelFormat format, MetaData? metadata)
		{
			ulong header = format switch
			{
				ModelFormat.SA1 => SA1MDLVer,
				ModelFormat.SADX => SADXMDLVer,
				ModelFormat.SA2 => SA2MDLVer,
				ModelFormat.SA2B => SA2BMDLVer,
				ModelFormat.Buffer => BUFMDLVer,
				_ => throw new ArgumentException($"Model format {format} not supported for SAMDL files"),
			};

			writer.WriteULong(header);

			uint placeholderAddr = writer.Position;
			// 4 bytes: node address placeholder
			// 4 bytes: metadata placeholder
			writer.WriteEmpty(8);

			PointerLUT lut = new();
			uint modelAddress = model.Write(writer, format, lut);

			metadata ??= new();
			metadata.Labels = lut.Labels.GetDictFrom();
			uint metadataAddress = metadata.Write(writer);

			uint end = writer.Position;
			writer.Seek(placeholderAddr, SeekOrigin.Begin);
			writer.WriteUInt(modelAddress);
			writer.WriteUInt(metadataAddress);
			writer.Seek(end, SeekOrigin.Begin);
		}


		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{(NJFile ? "" : "NJ")} Modelfile - {Format}";
		}
	}
}
