using SA3D.Modeling.Mesh;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;
using SA3D.Common.IO;
using System.IO;
using System;
using static SA3D.Modeling.File.FileHeaders;
using SA3D.Modeling.File.Structs;
using SA3D.Modeling.ObjectData.Structs;
using SA3D.Texturing.Texname;
using System.Collections.Generic;

namespace SA3D.Modeling.File
{
	/// <summary>
	/// Node model with attach data file contents.
	/// </summary>
	public class ModelFile
	{
		/// <summary>
		/// Whether the file is an NJ binary.
		/// </summary>
		public bool NJFile { get; }

		/// <summary>
		/// Attach format of the file.
		/// </summary>
		public ModelFormat Format { get; }

		/// <summary>
		/// Hierarchy tip of the file.
		/// </summary>
		public Node Model { get; }

		/// <summary>
		/// Texture name list from NJ files
		/// </summary>
		public TextureNameList? TextureNames { get; }

		/// <summary>
		/// Meta data of the file.
		/// </summary>
		public MetaData MetaData { get; }


		/// <summary>
		/// Creates a new model file.
		/// </summary>
		/// <param name="format">Whether the file is an NJ binary.</param>
		/// <param name="model">Attach format of the file.</param>
		/// <param name="metaData">Hierarchy tip of the file.</param>
		/// <param name="nj">Meta data of the file.</param>
		/// <param name="textureNames">Texture name list of the file.</param>
		public ModelFile(ModelFormat format, Node model, MetaData metaData, bool nj, TextureNameList? textureNames)
		{
			Format = format;
			Model = model;
			MetaData = metaData;
			NJFile = nj;
			TextureNames = textureNames;
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
			if(CheckIsSAModelFile(reader, address))
			{
				return true;
			}

			return CheckIsNJModelFile(reader, address);
		}

		private static bool CheckIsSAModelFile(EndianStackReader reader, uint address)
		{
			reader.PushBigEndian(false);

			bool result = (reader.ReadULong(address) & HeaderMask) switch
			{
				SA1MDL or SADXMDL or SA2MDL or SA2BMDL or BUFMDL => true,
				_ => false,
			};

			reader.PopEndian();

			return result;
		}

		private static bool CheckIsNJModelFile(EndianStackReader reader, uint address)
		{
			return NJBlockUtility.FindBlockAddress(reader, address, ModelBlockHeaders, out _);
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
			if(CheckIsSAModelFile(reader, address))
			{
				try
				{
					reader.PushBigEndian(false);
					return ReadSA(reader, address);
				}
				finally
				{
					reader.PopEndian();
				}
			}

			uint prevImageBase = reader.ImageBase;
			if(CheckIsNJModelFile(reader, address))
			{
				try
				{
					reader.PushBigEndian(reader.CheckBigEndian32(address + 4));
					return ReadNJ(reader, address);
				}
				finally
				{
					reader.ImageBase = prevImageBase;
					reader.PopEndian();
				}
			}

			throw new FormatException("File is not a model file");
		}

		private static ModelFile ReadNJ(EndianStackReader reader, uint address)
		{
			Dictionary<uint, uint> blocks = NJBlockUtility.GetBlockAddresses(reader, address);
			uint prevImageBase = reader.ImageBase;

			TextureNameList? textureNames = null;
			if(NJBlockUtility.FindBlockAddress(blocks, TextureListBlockHeaders, out uint? texturelistBlockAddress))
			{
				uint textureListAddress = texturelistBlockAddress!.Value + 8;
				reader.ImageBase = unchecked((uint)-textureListAddress);
				textureNames = TextureNameList.Read(reader, textureListAddress, new());
				reader.ImageBase = prevImageBase;
			}

			NJBlockUtility.FindBlockAddress(blocks, ModelBlockHeaders, out uint? modelBlockAddress);
			uint modelAddress = modelBlockAddress!.Value + 8;
			reader.ImageBase = unchecked((uint)-modelAddress);

			uint blockHeader = blocks[modelBlockAddress!.Value];
			ModelFormat format = (blockHeader >> 16) switch
			{
				BM => ModelFormat.SA1,
				CM => ModelFormat.SA2,
				_ => throw new FormatException()
			};

			Node model = Node.Read(reader, modelAddress, format, new());

			return new(format, model, new(), true, textureNames);
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

			if(metaData.MetaWeights.Count > 0)
			{
				CreateWeldings(metaData, lut);
			}

			reader.ImageBase = prevImageBase;

			return new(format, model, metaData, false, null);
		}

		private static void CreateWeldings(MetaData metadata, PointerLUT lut)
		{
			foreach(MetaWeightNode metaWeightNode in metadata.MetaWeights)
			{
				if(!lut.Nodes.TryGetValue(metaWeightNode.NodePointer, out Node? node))
				{
					throw new InvalidDataException($"Metadata has weights for a node at {metaWeightNode.NodePointer:X8}, but no node has been read at that address!");
				}

				VertexWelding[] vertexWelds = new VertexWelding[metaWeightNode.VertexWeights.Length];

				for(int i = 0; i < vertexWelds.Length; i++)
				{
					MetaWeightVertex metaWeightVertex = metaWeightNode.VertexWeights[i];

					Weld[] welds = new Weld[metaWeightVertex.Weights.Length];

					for(int j = 0; j < welds.Length; j++)
					{
						MetaWeight metaWeight = metaWeightVertex.Weights[j];

						if(!lut.Nodes.TryGetValue(metaWeight.NodePointer, out Node? sourceNode))
						{
							throw new InvalidDataException($"Metadata draws weight influence from a node at {metaWeightNode.NodePointer:X8}, but no node has been read at that address!");
						}

						welds[j] = new(sourceNode, metaWeight.VertexIndex, metaWeight.Weight);
					}

					vertexWelds[i] = new(metaWeightVertex.DestinationVertexIndex, welds);
				}

				Array.Sort(vertexWelds, (a, b) => a.DestinationVertexIndex.CompareTo(b.DestinationVertexIndex));

				node.Welding = vertexWelds;
			}

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

			CreateMetaWeights(model, metadata, lut);

			uint metadataAddress = metadata.Write(writer);

			uint end = writer.Position;
			writer.Seek(placeholderAddr, SeekOrigin.Begin);
			writer.WriteUInt(modelAddress);
			writer.WriteUInt(metadataAddress);
			writer.Seek(end, SeekOrigin.Begin);
		}

		private static void CreateMetaWeights(Node model, MetaData metadata, PointerLUT lut)
		{
			metadata.MetaWeights.Clear();
			Node[] nodes = model.GetTreeNodes();

			foreach(Node node in nodes)
			{
				if(node.Welding == null)
				{
					continue;
				}

				uint nodePointer = lut.Nodes.GetAddress(node)!.Value;
				MetaWeightVertex[] metaWeightVertices = new MetaWeightVertex[node.Welding.Length];

				for(int i = 0; i < metaWeightVertices.Length; i++)
				{
					VertexWelding vertexWelding = node.Welding[i];

					MetaWeight[] metaWeights = new MetaWeight[vertexWelding.Welds.Length];
					for(int j = 0; j < metaWeights.Length; j++)
					{
						Weld weld = vertexWelding.Welds[j];

						if(!lut.Nodes.TryGetAddress(weld.SourceNode, out uint sourceNodePointer))
						{
							throw new InvalidDataException($"Source node \"{weld.SourceNode.Label}\" is not part of the model that is being written!");
						}

						metaWeights[j] = new MetaWeight(sourceNodePointer, weld.VertexIndex, weld.Weight);
					}

					metaWeightVertices[i] = new(vertexWelding.DestinationVertexIndex, metaWeights);
				}

				metadata.MetaWeights.Add(new(nodePointer, metaWeightVertices));
			}
		}


		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{(NJFile ? "" : "NJ")} Modelfile - {Format}";
		}
	}
}
