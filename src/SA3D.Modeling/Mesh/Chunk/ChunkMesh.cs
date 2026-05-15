using Amicitia.IO.Binary;
using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Chunk.PolyChunks;
using SA3D.Modeling.Mesh.Chunk.Structs;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Chunk format mesh data
	/// </summary>
	public sealed class ChunkMesh : MeshData
	{
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
