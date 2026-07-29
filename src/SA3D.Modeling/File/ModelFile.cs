using Amicitia.IO.Binary;
using Amicitia.IO.Binary.Extensions;
using Amicitia.IO.Streams;
using J113D.Json;
using SA3D.Common.Ascii;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.File.MetaData;
using SA3D.Modeling.File.MetaData.Blocks;
using SA3D.Modeling.File.MetaData.Weights;
using SA3D.Modeling.Mesh;
using SA3D.Modeling.Mesh.Basic;
using SA3D.Modeling.Mesh.Chunk;
using SA3D.Modeling.Mesh.Chunk.Structs;
using SA3D.Modeling.Mesh.Ginja;
using SA3D.Modeling.Mesh.Ginja.Enums;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.ObjectData.Structs;
using SA3D.Modeling.Structs;
using SA3D.Modeling.TexName;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using static SA3D.Modeling.File.FileHeaders;

namespace SA3D.Modeling.File
{
	/// <summary>
	/// Node model with meshdata.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class ModelFile : IFileSerializable, IAsciiSerializable<AsciiIOContext>
	{
		private class JsonConverter : SimpleJsonObjectConverter<ModelFile>
		{
			private const string _njFile = nameof(NJFile);
			private const string _format = nameof(Format);
			private const string _model = nameof(Model);
			private const string _textureNames = nameof(TextureNames);
			private const string _metaData = nameof(MetaData);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _njFile, new(PropertyTokenType.Bool, false) },
				{ _format, new(PropertyTokenType.String, null) },
				{ _model, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _textureNames, new(PropertyTokenType.Object, null) },
				{ _metaData, new(PropertyTokenType.Array, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _njFile:
						return reader.GetBoolean();
					case _format:
						return JsonSerializer.Deserialize<Format>(ref reader, options);
					case _model:
						return JsonSerializer.Deserialize<Node>(ref reader, options);
					case _textureNames:
						return JsonSerializer.Deserialize<TextureNameList>(ref reader, options);
					case _metaData:
						return JsonSerializer.Deserialize<MetaDataBlocks>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override ModelFile Create(ReadOnlyDictionary<string, object?> values)
			{
				Node model = (Node?)values[_model]
					?? throw new InvalidDataException($"Modelfile requires \"{_model}\" property");

				ModelFile result = new(model)
				{
					NJFile = (bool)values[_njFile]!,
					TextureNames = (TextureNameList?)values[_textureNames],
					MetaData = (MetaDataBlocks?)values[_metaData] ?? new()
				};

				if((Format?)values[_format] is Format format)
				{
					result.Format = format;
				}

				return result;
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, ModelFile value, JsonSerializerOptions options)
			{
				if(value.NJFile)
				{
					writer.WriteBoolean(_njFile, value.NJFile);
				}

				writer.WritePropertyName(_format);
				JsonSerializer.Serialize(writer, value.Format, options);

				writer.WritePropertyName(_metaData);
				JsonSerializer.Serialize(writer, value.MetaData, options);

				if(value.TextureNames != null)
				{
					writer.WritePropertyName(_textureNames);
					JsonSerializer.Serialize(writer, value.TextureNames, options);
				}

				writer.WritePropertyName(_model);
				JsonSerializer.Serialize(writer, value.Model, options);
			}
		}

		/// <summary>
		/// Original filepath
		/// </summary>
		public string? Filepath { get; set; }

		/// <summary>
		/// Whether the file is an NJ binary.
		/// </summary>
		public bool NJFile { get; set; }

		/// <summary>
		/// Meshdata format of the file.
		/// </summary>
		public Format Format { get; set; }

		/// <summary>
		/// Hierarchy tip of the file.
		/// </summary>
		public Node Model { get; set; }

		/// <summary>
		/// Texture name list from NJ files
		/// </summary>
		public TextureNameList? TextureNames { get; set; }

		/// <summary>
		/// Meta data of the file.
		/// </summary>
		public MetaDataBlocks MetaData { get; set; }


		/// <summary>
		/// Creates a new blank model file
		/// </summary>
		public ModelFile() : this(new()) { }

		/// <summary>
		/// Creates a model file for a model
		/// </summary>
		/// <param name="model">Model of the file</param>
		public ModelFile(Node model)
		{
			Format = model.GetMeshFormat()?.ToFormat() ?? Format.Basic;
			Model = model;
			MetaData = new();
		}


		#region Checking


		/// <inheritdoc/>
		public bool Check(BinaryObjectReader reader)
		{
			return CheckIsSAFile(reader) || CheckIsNJFile(reader);
		}

		private bool CheckIsSAFile(BinaryObjectReader reader)
		{
			using SeekToken seekToken = reader.At();
			using EndiannessToken endiannessToken = reader.WithEndian(Endianness.Little);

			bool result = (reader.ReadUInt64() & HeaderMask) switch
			{
				SA1MDL or SADXMDL or SA2MDL or SA2BMDL => true,
				_ => false,
			};

			return result;
		}

		private bool CheckIsNJFile(BinaryObjectReader reader)
		{
			return NJBlockUtility.FindBlockOffset(reader, ModelBlockHeaders, out _);
		}

		#endregion

		#region Reading

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, FileContext context)
		{
			Filepath = context.Filepath;

			if(CheckIsSAFile(reader))
			{
				ReadSA(reader);
			}
			else if(CheckIsNJFile(reader))
			{
				ReadNJ(reader);
			}
			else
			{
				throw new FormatException("File is not a model file");
			}
		}


		private void ReadSA(BinaryObjectReader reader)
		{
			using EndiannessToken endiannesToken = reader.WithEndian(Endianness.Little);

			ulong headerVersion = reader.ReadUInt64();

			Format = (headerVersion & HeaderMask) switch
			{
				SA1MDL => Format.Basic,
				SADXMDL => Format.BasicDX,
				SA2MDL => Format.Chunk,
				SA2BMDL => Format.Ginja,
				_ => throw new FormatException("File invalid; Header malformed"),
			};

			byte version = (byte)(headerVersion >> 56);
			if(version > CurrentModelVersion)
			{
				throw new FormatException($"File invalid; Unsupported version {version}; Maximum supported version: {CurrentModelVersion}");
			}

			using(reader.At(4, SeekOrigin.Current))
			{
				MetaDataIOContext metaDataContext = new()
				{
					Version = version,
					HasAnimMorphFiles = true
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
				MeshFormat = Format,
				PointerLUT = new(labels)
			};

			Model = reader.ReadObjectOffset<Node, IOContext>(context, context.PointerLUT)
				?? throw reader.ReadNullReference(nameof(ModelFile), nameof(Model));

			if(MetaData.TryGetBlock(out WeightsMetaDataBlock? weightsBlock))
			{
				CreateWeldings(weightsBlock!.Weights, context.PointerLUT);
			}

			NJFile = false;
		}

		private static void CreateWeldings(IEnumerable<MetaWeightNode> weights, ModelOffsetLUT lut)
		{
			foreach(MetaWeightNode metaWeightNode in weights)
			{
				if(!lut.Nodes.TryGetValue(metaWeightNode.NodeOffset, out Node? node))
				{
					throw new InvalidDataException($"Metadata has weights for a node at {metaWeightNode.NodeOffset:X8}, but no node has been read at that address!");
				}

				VertexWelding[] vertexWelds = new VertexWelding[metaWeightNode.VertexWeights.Length];

				for(int i = 0; i < vertexWelds.Length; i++)
				{
					MetaWeightVertex metaWeightVertex = metaWeightNode.VertexWeights[i];

					Weld[] welds = new Weld[metaWeightVertex.Weights.Length];

					for(int j = 0; j < welds.Length; j++)
					{
						MetaWeight metaWeight = metaWeightVertex.Weights[j];

						if(!lut.Nodes.TryGetValue(metaWeight.NodeOffset, out Node? sourceNode))
						{
							throw new InvalidDataException($"Metadata draws weight influence from a node at {metaWeightNode.NodeOffset:X8}, but no node has been read at that address!");
						}

						welds[j] = new(sourceNode, metaWeight.VertexIndex, metaWeight.Weight);
					}

					vertexWelds[i] = new(metaWeightVertex.DestinationVertexIndex, welds);
				}

				Array.Sort(vertexWelds, (a, b) => a.DestinationVertexIndex.CompareTo(b.DestinationVertexIndex));

				node.Welding = vertexWelds;
			}

		}


		private void ReadNJ(BinaryObjectReader reader)
		{
			using EndiannessToken endiannesToken = reader.WithEndian(reader.CheckEndianness32(4, SeekOrigin.Current));
			Dictionary<long, string> blocks = NJBlockUtility.GetBlockOffsets(reader);

			Model = ReadNJModel(reader, blocks, out IOContext context);
			Format = context.MeshFormat;
			TextureNames = ReadNJTextureList(reader, blocks, context.PointerLUT);
			NJFile = true;
		}

		private static Node ReadNJModel(BinaryObjectReader reader, Dictionary<long, string> blocks, out IOContext context)
		{
			if(!NJBlockUtility.FindBlockOffset(blocks, ModelBlockHeaders, out long? modelBlockAddress))
			{
				throw new InvalidOperationException("NJ model file has no model block!");
			}

			string blockHeader = blocks[modelBlockAddress!.Value];
			Format format = blockHeader[2..] switch
			{
				BasicModelBlockType => Format.Basic,
				ChunkModelBlockType => Format.Chunk,
				_ => throw new UnreachableException()
			};

			long modelOffset = modelBlockAddress!.Value + (sizeof(uint) * 2);
			using SeekToken seekToken = reader.At(modelOffset, SeekOrigin.Begin);
			using OffsetOriginToken offsetOriginToken = reader.WithOffsetOrigin();

			context = new()
			{
				MeshFormat = format,
				PointerLUT = new()
			};

			return reader.ReadObject<Node, IOContext>(context, context.PointerLUT);
		}

		private static TextureNameList? ReadNJTextureList(BinaryObjectReader reader, Dictionary<long, string> blocks, OffsetLUT lut)
		{
			if(!NJBlockUtility.FindBlockOffset(blocks, TextureListBlockHeaders, out long? texturelistBlockOffset))
			{
				return null;
			}

			long textureListOffset = texturelistBlockOffset!.Value + (sizeof(uint) * 2);
			using SeekToken seekToken = reader.At(textureListOffset, SeekOrigin.Begin);
			using OffsetOriginToken offsetOriginToken = reader.WithOffsetOrigin();

			return reader.ReadObject<TextureNameList, OffsetLUT>(lut);
		}

		#endregion

		#region Writing

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, FileContext context)
		{
			if(NJFile)
			{
				WriteNJ(writer);
			}
			else
			{
				WriteSA(writer);
			}
		}


		private void WriteNJ(BinaryObjectWriter writer)
		{
			writer.WriteString(StringBinaryFormat.FixedLength, NinjaModelBlockIdentifier, 2);

			switch(Format)
			{
				case Format.Basic or Format.BasicDX:
					writer.WriteString(StringBinaryFormat.FixedLength, BasicModelBlockType, 2);
					break;
				case Format.Chunk:
					writer.WriteString(StringBinaryFormat.FixedLength, ChunkModelBlockType, 2);
					break;
				case Format.Ginja:
				default:
					throw new ArgumentException($"Meshdata format {Format} not supported for NJ binaries");
			}

			SeekToken fileSizeOffset = writer.At();
			writer.WriteUInt32(0);

			IOContext context = new()
			{
				MeshFormat = Format,
				LevelFormat = Format,
				PointerLUT = new()
			};

			long modelStart = writer.Position;

			using(writer.WithOffsetOrigin())
			{
				writer.WriteObject(Model, context, context.PointerLUT);
			}

			uint byteSize = (uint)(writer.Position - modelStart);

			using(writer.At())
			{
				fileSizeOffset.Dispose();
				writer.WriteUInt32(byteSize);
			}
		}


		private void WriteSA(BinaryObjectWriter writer)
		{
			ulong header = Format switch
			{
				Format.Basic => SA1MDLVer,
				Format.BasicDX => SADXMDLVer,
				Format.Chunk => SA2MDLVer,
				Format.Ginja => SA2BMDLVer,
				_ => throw new ArgumentException($"Model format {Format} not supported for SAMDL files"),
			};

			writer.WriteUInt64(header);

			IOContext context = new()
			{
				MeshFormat = Format,
				LevelFormat = Format,
				PointerLUT = new()
			};

			writer.WriteObjectOffset(Model, context);
			MetaData.Write(writer, context.PointerLUT.Labels, null, () => CreateMetaWeights(context.PointerLUT));
		}

		private void CreateMetaWeights(ModelOffsetLUT lut)
		{
			MetaData.Blocks.RemoveAll(x => x.Type is MetaDataBlockType.Weight);
			Node[] nodes = Model.GetTreeNodes();
			WeightsMetaDataBlock weightsBlock = new();

			foreach(Node node in nodes)
			{
				if(node.Welding == null)
				{
					continue;
				}

				long nodeOffset = lut.Nodes.GetAddress(node)!.Value;
				MetaWeightVertex[] metaWeightVertices = new MetaWeightVertex[node.Welding.Length];

				for(int i = 0; i < metaWeightVertices.Length; i++)
				{
					VertexWelding vertexWelding = node.Welding[i];

					MetaWeight[] metaWeights = new MetaWeight[vertexWelding.Welds.Length];
					for(int j = 0; j < metaWeights.Length; j++)
					{
						Weld weld = vertexWelding.Welds[j];

						if(!lut.Nodes.TryGetAddress(weld.SourceNode, out long sourceNodeOffset))
						{
							throw new InvalidDataException($"Source node \"{weld.SourceNode.Label}\" is not part of the model that is being written!");
						}

						metaWeights[j] = new MetaWeight(sourceNodeOffset, weld.VertexIndex, weld.Weight);
					}

					metaWeightVertices[i] = new(vertexWelding.DestinationVertexIndex, metaWeights);
				}


				weightsBlock.Weights.Add(new(nodeOffset, metaWeightVertices));
			}

			MetaData.Blocks.Add(weightsBlock);
		}


		/// <inheritdoc/>
		public void Write(AsciiWriter writer, AsciiIOContext context)
		{
			string format = Format switch
			{
				Format.Chunk => "Cnk",
				Format.Ginja => "Gj",
				_ => string.Empty,
			};

			int nodeCount = Model.GetTreeNodeCount();

			Stack<(Node, int)> depthStack = [];
			depthStack.Push((Model, 1));

			int depth = 0;
			while(depthStack.Count > 0)
			{
				(Node node, int nodeDepth) = depthStack.Pop();
				if(nodeDepth > depth)
				{
					depth = nodeDepth;
				}

				foreach(Node child in node)
				{
					depthStack.Push((child, nodeDepth + 1));
				}
			}

			int vertexCount;

			switch(Format)
			{
				case Format.Basic:
				case Format.BasicDX:
					vertexCount = Model.GetTreeMeshDataEnumerable().OfType<BasicMesh>().Max(x => x.Positions.Length);
					break;
				case Format.Chunk:
					vertexCount = Model.GetTreeMeshDataEnumerable().OfType<ChunkMesh>()
						.Max(x => x.VertexChunks?
							.Max<VertexChunk, int>(y => y.Type.CheckHasAttributes() ?
								y.Vertices.Max<ChunkVertex, int>(z => z.Index + y.IndexOffset)
								: y.IndexOffset + y.Vertices.Length
							) ?? 0);
					break;
				case Format.Ginja:
					vertexCount = Model.GetTreeMeshDataEnumerable().OfType<GinjaMesh>()
						.Max(x => x.VertexData?
							.Where(x => x.Type == GinjaVertexType.Position)
							.Max<GinjaVertexSet, int>(x => x.DataLength / 3) ?? 0);
					break;
				default:
					throw new InvalidOperationException();
			}

			writer.WriteLine($"/* {AsciiHeader} {(TextureNames != null ? "Mixed" : "Model")} {format}Model */", 2);
			writer.WriteLine($"/* ROOT OBJECT  : {Model.Label} n({nodeCount}) d({depth}) v({vertexCount}) */");

			if(TextureNames != null)
			{
				writer.WriteLine($"/* TEXLIST     : {TextureNames.Label} n(?) */");
				writer.WriteObject(TextureNames);
			}

			writer.WriteLine();

			ModelAsciiIOContext modelContext = ModelAsciiIOContext.FromModel(Format, Model, context);
			writer.WriteObject(Model, modelContext);

			using(writer.WriteObjectBlock("DEFAULT"))
			{
				writer.WriteLine("#ifndef DEFAULT_OBJECT_NAME");
				writer.WriteObjectPropertyLine("#define DEFAULT_OBJECT_NAME", Model);
				writer.WriteLine("#endif", 2);

				if(TextureNames != null)
				{
					writer.WriteLine("#ifndef DEFAULT_TEXLIST_NAME");
					writer.WriteObjectPropertyLine("#define DEFAULT_TEXLIST_NAME", TextureNames);
					writer.WriteLine("#endif", 2);
				}
			}
		}

		#endregion

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{(NJFile ? "" : "NJ")} Modelfile - {Format}";
		}

	}
}
