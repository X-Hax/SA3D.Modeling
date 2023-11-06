using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Chunk.PolyChunks;
using SA3D.Modeling.Mesh.Chunk.Structs;
using SA3D.Modeling.Mesh.Converters;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Chunk format mesh data
	/// </summary>
	public sealed class ChunkAttach : Attach
	{
		/// <summary>
		/// Vertex data blocks.
		/// </summary>
		public ILabeledArray<VertexChunk?>? VertexChunks { get; set; }

		/// <summary>
		/// Polygon data blocks.
		/// </summary>
		public ILabeledArray<PolyChunk?>? PolyChunks { get; set; }

		/// <inheritdoc/>
		public override AttachFormat Format
			=> AttachFormat.CHUNK;


		/// <summary>
		/// Creates a new chunk attach.
		/// </summary>
		/// <param name="vertexChunks">Vertex data blocks.</param>
		/// <param name="polyChunks">Polygon data blocks</param>
		public ChunkAttach(VertexChunk?[]? vertexChunks, PolyChunk?[]? polyChunks) : base()
		{
			string identifier = GenerateIdentifier();
			Label = "attach_" + identifier;
			VertexChunks = vertexChunks == null ? null : new LabeledArray<VertexChunk?>("vertex_" + identifier, vertexChunks);
			PolyChunks = polyChunks == null ? null : new LabeledArray<PolyChunk?>("poly_" + identifier, polyChunks);
		}

		/// <summary>
		/// Creates a new chunk attach.
		/// </summary>
		/// <param name="vertexChunks">Vertex data blocks.</param>
		/// <param name="polyChunks">Polygon data blocks</param>
		public ChunkAttach(ILabeledArray<VertexChunk?>? vertexChunks, ILabeledArray<PolyChunk?>? polyChunks) : base()
		{
			Label = "attach_" + GenerateIdentifier();
			VertexChunks = vertexChunks;
			PolyChunks = polyChunks;
		}

		private ChunkAttach(string label, ILabeledArray<VertexChunk?>? vertexChunks, ILabeledArray<PolyChunk?>? polyChunks, Bounds meshBounds) : base()
		{
			Label = label;
			VertexChunks = vertexChunks;
			PolyChunks = polyChunks;
			MeshBounds = meshBounds;
		}


		/// <inheritdoc/>
		public override bool CheckHasWeights()
		{
			if(PolyChunks == null || !PolyChunks.Any(a => a is StripChunk))
			{
				return VertexChunks != null && VertexChunks.Any(a => a?.HasWeight == true);
			}

			HashSet<int> ids = new();
			if(VertexChunks != null)
			{
				foreach(VertexChunk? vc in VertexChunks)
				{
					if(vc == null)
					{
						continue;
					}

					if(vc.HasWeight)
					{
						return true;
					}

					ids.UnionWith(Enumerable.Range(vc.IndexOffset, vc.Vertices.Length));
				}
			}

			return PolyChunks
				.OfType<StripChunk>()
				.SelectMany(a => a.Strips)
				.SelectMany(a => a.Corners)
				.Any(a => !ids.Contains(a.Index));
		}

		/// <inheritdoc/>
		public override void RecalculateBounds()
		{
			if(PolyChunks == null || VertexChunks == null)
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

			if(CheckHasWeights())
			{
				MeshBounds = new(MeshBounds.Position, 0);
			}
		}

		/// <inheritdoc/>
		public override bool CanWrite(ModelFormat format)
		{
			return base.CanWrite(format) || format is ModelFormat.SA2;
		}

		/// <summary>
		/// Calculates the active polygon chunks per chunk attaches in a model tree.
		/// </summary>
		/// <param name="model">The model to get the active polygon chunks for.</param>
		/// <returns>The active polygon chunks.</returns>
		public static Dictionary<ChunkAttach, PolyChunk?[]> GetActivePolyChunks(Node model)
		{
			if(model.GetAttachFormat() != AttachFormat.CHUNK)
			{
				throw new FormatException($"Model {model.Label} is not a chunk model.");
			}

			return ChunkConverter.GetActivePolyChunks(model);
		}

		/// <summary>
		/// Reads a chunk attach off an endian byte reader.
		/// </summary>
		/// <param name="reader">Reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The chunk attach that was read.</returns>
		public static ChunkAttach Read(EndianStackReader reader, uint address, PointerLUT lut)
		{
			ChunkAttach onRead()
			{
				ILabeledArray<VertexChunk?>? vertexChunks = null;
				if(reader.TryReadPointer(address, out uint vertexAddress))
				{
					vertexChunks = lut.GetAddLabeledValue<LabeledArray<VertexChunk?>>(vertexAddress, "vertex_",
						() => new(VertexChunk.ReadArray(reader, vertexAddress)));
				}

				ILabeledArray<PolyChunk?>? polyChunks = null;
				if(reader.TryReadPointer(address + 4, out uint polyAddress))
				{
					polyChunks = lut.GetAddLabeledValue<LabeledArray<PolyChunk?>>(polyAddress, "poly_",
						() => new(PolyChunk.ReadArray(reader, polyAddress, lut)));
				}

				return new ChunkAttach(vertexChunks, polyChunks)
				{
					MeshBounds = Bounds.Read(reader, address + 8)
				};
			}

			return lut.GetAddLabeledValue(address, "attach_", onRead);
		}

		/// <inheritdoc/>
		protected override uint WriteInternal(EndianStackWriter writer, ModelFormat format, PointerLUT lut)
		{
			uint vertexAddress = lut.GetAddAddress(VertexChunks, () => VertexChunk.WriteArray(writer, VertexChunks!));
			uint polyAddress = lut.GetAddAddress(PolyChunks, () => PolyChunk.WriteArray(writer, PolyChunks!, lut));
			uint address = writer.PointerPosition;

			writer.WriteUInt(vertexAddress);
			writer.WriteUInt(polyAddress);
			MeshBounds.Write(writer);

			return address;
		}


		/// <inheritdoc/>
		public override ChunkAttach Clone()
		{
			LabeledArray<VertexChunk?>? vertexChunks = null;
			LabeledArray<PolyChunk?>? polyChunks = null;

			if(VertexChunks != null)
			{
				VertexChunk?[] chunks = VertexChunks.Select(x => x?.Clone()).ToArray();
				vertexChunks = new(VertexChunks.Label, chunks);
			}

			if(PolyChunks != null)
			{
				PolyChunk?[] chunks = PolyChunks.Select(x => x?.Clone()).ToArray();
				polyChunks = new(PolyChunks.Label, chunks);
			}

			return new ChunkAttach(Label, vertexChunks, polyChunks, MeshBounds);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"CHUNK {Label} - V[{VertexChunks?.Length}], P[{PolyChunks?.Length}]";
		}
	}
}
