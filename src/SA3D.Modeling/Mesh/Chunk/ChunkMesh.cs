using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Chunk.PolyChunks;
using SA3D.Modeling.Mesh.Chunk.Structs;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Chunk format mesh data
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class ChunkMesh : MeshData
	{
		internal class JsonConverter : ChildJsonObjectConverter<MeshFormat, ChunkMesh, MeshData>
		{
			private const string _vertexChunks = nameof(ChunkMesh.VertexChunks);
			private const string _polyChunks = nameof(ChunkMesh.PolyChunks);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<MeshFormat, MeshData> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
		{
			{ _vertexChunks, new(PropertyTokenType.Object | PropertyTokenType.String, null, true) },
			{ _polyChunks, new(PropertyTokenType.Object | PropertyTokenType.String, null, true) },
		});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(MeshFormat key)
			{
				return key == MeshFormat.Chunk;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _vertexChunks:
						return JsonSerializer.Deserialize<LabeledArray<VertexChunk>?>(ref reader, options);
					case _polyChunks:
						return JsonSerializer.Deserialize<LabeledArray<PolyChunk>?>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override ChunkMesh CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					Label = (string)values[BaseJsonConverter._label]!,
					MeshBounds = (Bounds)values[BaseJsonConverter._meshBounds]!,
					VertexChunks = (LabeledArray<VertexChunk>?)values[_vertexChunks],
					PolyChunks = (LabeledArray<PolyChunk>?)values[_polyChunks]
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, ChunkMesh value, JsonSerializerOptions options)
			{
				if(_vertexChunks != null)
				{
					writer.WritePropertyName(_vertexChunks);
					JsonSerializer.Serialize(writer, value.VertexChunks, options);
				}

				if(_polyChunks != null)
				{
					writer.WritePropertyName(_polyChunks);
					JsonSerializer.Serialize(writer, value.PolyChunks, options);
				}
			}
		}

		/// <summary>
		/// Label prefix for <see cref="VertexChunks"/>
		/// </summary>
		public const string VertexChunksLabelPrefix = "vertex_";

		/// <summary>
		/// Label prefix for <see cref="PolyChunks"/>
		/// </summary>
		public const string PolyChunksLabelPrefix = "poly_";

		/// <summary>
		/// Vertex data blocks.
		/// </summary>
		public LabeledArray<VertexChunk>? VertexChunks { get; set; }

		/// <summary>
		/// Polygon data blocks.
		/// </summary>
		public LabeledArray<PolyChunk>? PolyChunks { get; set; }

		/// <inheritdoc/>
		public override MeshFormat MeshFormat
			=> MeshFormat.Chunk;

		/// <inheritdoc/>
		public override string LabelPrefix => "chunkMesh_";

		/// <inheritdoc/>
		public override bool CheckHasWeights()
		{
			if(VertexChunks == null)
			{
				return false;
			}

			if(VertexChunks.Any(x => x.Type.CheckHasWeights()))
			{
				return true;
			}

			if(PolyChunks?.Any(a => a is StripChunk) == true)
			{
				HashSet<int> ids = [.. VertexChunks.SelectMany(x => Enumerable.Range(x.IndexOffset, x.Vertices.Length))];

				return PolyChunks
					.OfType<StripChunk>()
					.SelectMany(a => a.Strips)
					.SelectMany(a => a.Corners)
					.Any(a => !ids.Contains(a.Index));
			}

			return false;
		}

		/// <inheritdoc/>
		public override void RecalculateBounds()
		{
			if(PolyChunks == null || VertexChunks == null || CheckHasWeights())
			{
				MeshBounds = default;
				return;
			}

			IEnumerable<Vector3> vertexEnumerator()
			{
				foreach(VertexChunk? cnk in VertexChunks!)
				{
					if(cnk == null)
					{
						continue;
					}

					foreach(ChunkVertex vtx in cnk.Vertices)
					{
						yield return vtx.Position;
					}
				}
			}

			MeshBounds = Bounds.FromPoints(vertexEnumerator());
		}

		/// <inheritdoc/>
		public override bool CanWrite(Format format)
		{
			return format is Format.Chunk;
		}


		/// <inheritdoc/>
		public override void Read(BinaryObjectReader reader, IOContext context)
		{
			VertexChunks = reader.ReadLUTItemAtOffset(reader.ReadOffsetValue(), context.PointerLUT, VertexChunksLabelPrefix, VertexChunk.ReadArray);
			PolyChunks = reader.ReadLUTItemAtOffset(reader.ReadOffsetValue(), context.PointerLUT, PolyChunksLabelPrefix, PolyChunk.ReadArray);
			MeshBounds = reader.ReadObject<Bounds>();
		}

		/// <inheritdoc/>
		public override void Write(BinaryObjectWriter writer, IOContext context)
		{
			writer.WriteObjectOffset(VertexChunks.EmptyNull(), VertexChunk.WriteArray, context.PointerLUT);
			writer.WriteObjectOffset(PolyChunks.EmptyNull(), PolyChunk.WriteArray, context.PointerLUT);
			writer.WriteObject(MeshBounds);
		}

		/// <inheritdoc/>
		public override ChunkMesh Clone()
		{
			return new()
			{
				Label = Label,
				VertexChunks = VertexChunks?.ContentClone(),
				PolyChunks = PolyChunks?.ContentClone(),
				MeshBounds = MeshBounds
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"CHUNK {Label} - V[{VertexChunks?.Length}], P[{PolyChunks?.Length}]";
		}
	}
}
