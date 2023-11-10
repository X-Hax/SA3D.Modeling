using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Mesh.Converters;
using SA3D.Modeling.Mesh.Gamecube.Enums;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.Mesh.Gamecube
{
	/// <summary>
	/// A GC format attach
	/// </summary>
	public sealed class GCAttach : Attach
	{
		/// <summary>
		/// Seperate sets of vertex data in this attach.
		/// </summary>
		public Dictionary<GCVertexType, GCVertexSet> VertexData { get; }

		/// <summary>
		/// Meshes with opaque rendering properties.
		/// </summary>
		public GCMesh[] OpaqueMeshes { get; set; }

		/// <summary>
		/// Meshes with transparent rendering properties.
		/// </summary>
		public GCMesh[] TransparentMeshes { get; set; }

		/// <inheritdoc/>
		public override AttachFormat Format
			=> AttachFormat.GC;


		/// <summary>
		/// Creates a new GC attach.
		/// </summary>
		/// <param name="vertexData">Vertex data.</param>
		/// <param name="opaqueMeshes">Opaque meshes.</param>
		/// <param name="transprentMeshes">Transparent meshes.</param>
		public GCAttach(Dictionary<GCVertexType, GCVertexSet> vertexData, GCMesh[] opaqueMeshes, GCMesh[] transprentMeshes)
		{
			VertexData = vertexData;
			OpaqueMeshes = opaqueMeshes;
			TransparentMeshes = transprentMeshes;
			Label = "attach_" + GenerateIdentifier();
		}

		internal GCAttach(GCVertexSet[] vertexData, GCMesh[] opaqueMeshes, GCMesh[] transprentMeshes)
		{
			VertexData = new();
			foreach(GCVertexSet v in vertexData)
			{
				if(VertexData.ContainsKey(v.Type))
				{
					throw new ArgumentException($"Vertexdata contains two sets with the attribute {v.Type}");
				}

				VertexData.Add(v.Type, v);
			}

			OpaqueMeshes = opaqueMeshes;
			TransparentMeshes = transprentMeshes;

			Label = "attach_" + GenerateIdentifier();
		}


		/// <inheritdoc/>
		public override bool CheckHasWeights()
		{
			return false;
		}

		/// <inheritdoc/>
		public override void RecalculateBounds()
		{
			MeshBounds = Bounds.FromPoints(VertexData[GCVertexType.Position].Vector3Data);
		}

		/// <inheritdoc/>
		public override bool CanWrite(ModelFormat format)
		{
			return base.CanWrite(format) || format is ModelFormat.SA2B;
		}


		/// <summary>
		/// Removes duplicate vertex data.
		/// </summary>
		public void OptimizeVertexData()
		{
			List<GCMesh> allMeshes = new(OpaqueMeshes);
			allMeshes.AddRange(TransparentMeshes);

			foreach(GCVertexSet item in VertexData.Values)
			{
				item.Optimize(allMeshes);
			}
		}

		/// <summary>
		/// Optimizes the polygon data of all meshes.
		/// </summary>
		public void OptimizePolygons()
		{
			for(int i = 0; i < OpaqueMeshes.Length; i++)
			{
				OpaqueMeshes[i].OptimizePolygons();
			}

			for(int i = 0; i < TransparentMeshes.Length; i++)
			{
				TransparentMeshes[i].OptimizePolygons();
			}
		}


		/// <summary>
		/// Converts the gamecube attach to a set of buffer meshes.
		/// </summary>
		/// <param name="optimize">Whether to optimize the buffer mesh data.</param>
		/// <returns>The converted buffer meshes.</returns>
		public BufferMesh[] ConvertToBufferMeshData(bool optimize)
		{
			return GCConverter.ConvertGCToBuffer(this, optimize);
		}

		/// <inheritdoc/>
		protected override uint WriteInternal(EndianStackWriter writer, ModelFormat format, PointerLUT lut)
		{
			uint vtxAddr = GCVertexSet.WriteArray(writer, VertexData.Values.ToArray());

			uint[] opaqueMeshStructs = GCMesh.WriteArrayContents(writer, OpaqueMeshes);
			uint[] transparentMeshStructs = GCMesh.WriteArrayContents(writer, TransparentMeshes);

			uint opaqueAddress = 0;
			if(opaqueMeshStructs.Length > 0)
			{
				opaqueAddress = writer.PointerPosition;
				foreach(uint i in opaqueMeshStructs)
				{
					writer.WriteUInt(i);
				}
			}

			uint transparentAddress = 0;
			if(transparentMeshStructs.Length > 0)
			{
				transparentAddress = writer.PointerPosition;
				foreach(uint i in transparentMeshStructs)
				{
					writer.WriteUInt(i);
				}
			}

			uint address = writer.PointerPosition;

			writer.WriteUInt(vtxAddr);
			writer.WriteEmpty(4);
			writer.WriteUInt(opaqueAddress);
			writer.WriteUInt(transparentAddress);
			writer.WriteUShort((ushort)OpaqueMeshes.Length);
			writer.WriteUShort((ushort)TransparentMeshes.Length);
			MeshBounds.Write(writer);
			return address;
		}

		/// <summary>
		/// Reads a gamecube attach off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The gamecube attach that was read.</returns>
		public static GCAttach Read(EndianStackReader reader, uint address, PointerLUT lut)
		{
			GCAttach onRead()
			{
				uint vertexAddress = reader.ReadPointer(address);
				// uint gap = data.ReadPointer(address + 4);
				uint opaqueAddress = reader.ReadPointer(address + 8);
				uint transparentAddress = reader.ReadPointer(address + 12);

				int opaqueCount = reader.ReadShort(address + 16);
				int transparentCount = reader.ReadShort(address + 18);
				address += 20;
				Bounds bounds = Bounds.Read(reader, ref address);

				GCVertexSet[] vertexData = GCVertexSet.ReadArray(reader, vertexAddress);
				GCMesh[] opaqueMeshes = GCMesh.ReadArray(reader, opaqueAddress, opaqueCount);
				GCMesh[] transparentMeshes = GCMesh.ReadArray(reader, transparentAddress, transparentCount);

				return new GCAttach(vertexData, opaqueMeshes, transparentMeshes)
				{
					MeshBounds = bounds
				};
			}

			return lut.GetAddLabeledValue(address, "attach_", onRead);
		}


		/// <inheritdoc/>
		public override GCAttach Clone()
		{
			Dictionary<GCVertexType, GCVertexSet> vertexSets = new();
			foreach(KeyValuePair<GCVertexType, GCVertexSet> t in VertexData)
			{
				vertexSets.Add(t.Key, t.Value.Clone());
			}

			return new GCAttach(vertexSets, OpaqueMeshes.ContentClone(), TransparentMeshes.ContentClone())
			{
				Label = Label,
				MeshBounds = MeshBounds
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Label} - GC: {VertexData.Count} - {OpaqueMeshes.Length} - {TransparentMeshes.Length}";
		}
	}
}
