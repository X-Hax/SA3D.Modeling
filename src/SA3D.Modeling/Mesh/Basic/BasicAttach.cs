using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Mesh.Converters;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.Structs;
using System;
using System.Numerics;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.Mesh.Basic
{
	/// <summary>
	/// Mesh data format used by SA1 and SA2
	/// </summary>
	public sealed class BasicAttach : Attach
	{
		/// <summary>
		/// Vertex positions.
		/// </summary>
		public ILabeledArray<Vector3> Positions { get; }

		/// <summary>
		/// Vertex normals.
		/// </summary>
		public ILabeledArray<Vector3> Normals { get; }

		/// <summary>
		/// Polygon structures.
		/// </summary>
		public ILabeledArray<BasicMesh> Meshes { get; }

		/// <summary>
		/// Materials for the meshes.
		/// </summary>
		public ILabeledArray<BasicMaterial> Materials { get; }

		/// <inheritdoc/>
		public override AttachFormat Format
			=> AttachFormat.BASIC;


		private BasicAttach(ILabeledArray<Vector3> positions, ILabeledArray<Vector3> normals, ILabeledArray<BasicMesh> meshes, ILabeledArray<BasicMaterial> materials, Bounds meshBounds) : base()
		{
			Label = "attach_" + GenerateIdentifier();

			Positions = positions;
			Normals = normals;
			Meshes = meshes;
			Materials = materials;
			MeshBounds = meshBounds;
		}

		/// <summary>
		/// Creates a new BASIC attach using existing data.
		/// <br/> Array labels are automatically generated.
		/// </summary>
		/// <param name="positions">Vertex positions.</param>
		/// <param name="normals">Vertex normals.</param>
		/// <param name="meshes">Polygons structures.</param>
		/// <param name="materials">Materials the meshes.</param>
		public BasicAttach(Vector3[] positions, Vector3[] normals, BasicMesh[] meshes, BasicMaterial[] materials) : base()
		{
			string identifier = GenerateIdentifier();
			Label = "attach_" + identifier;

			Positions = new LabeledArray<Vector3>("vertex_" + identifier, positions);
			Normals = new LabeledArray<Vector3>("normal_" + identifier, normals);
			Meshes = new LabeledArray<BasicMesh>("meshlist_" + identifier, meshes);
			Materials = new LabeledArray<BasicMaterial>("matlist_" + identifier, materials);
		}

		/// <summary>
		/// Creates a new BASIC attach using existing data.
		/// </summary>
		/// <param name="positions">Vertex positions.</param>
		/// <param name="normals">Vertex normals.</param>
		/// <param name="meshes">Polygons structures.</param>
		/// <param name="materials">Materials the meshes.</param>
		public BasicAttach(ILabeledArray<Vector3> positions, ILabeledArray<Vector3> normals, ILabeledArray<BasicMesh> meshes, ILabeledArray<BasicMaterial> materials) : base()
		{
			Label = "attach_" + GenerateIdentifier();

			Positions = positions;
			Normals = normals;
			Meshes = meshes;
			Materials = materials;

			if(normals != null && positions.Length != normals.Length)
			{
				throw new ArgumentException("Position and Normal count doesnt match!");
			}
		}

		/// <inheritdoc/>
		public override bool CheckHasWeights()
		{
			return false;
		}

		/// <inheritdoc/>
		public override void RecalculateBounds()
		{
			MeshBounds = Bounds.FromPoints(Positions);
		}

		/// <inheritdoc/>
		public override bool CanWrite(ModelFormat format)
		{
			return base.CanWrite(format) || format is ModelFormat.SA1 or ModelFormat.SADX;
		}

		/// <summary>
		/// Converts the baisc attach to a set of buffer meshes.
		/// </summary>
		/// <param name="optimize">Whether to optimize the buffer mesh data.</param>
		/// <returns>The converted buffer meshes.</returns>
		public BufferMesh[] ConvertToBufferMeshData(bool optimize)
		{
			return BasicConverter.ConvertBasicToBuffer(this, optimize);
		}


		/// <inheritdoc/>
		protected override uint WriteInternal(EndianStackWriter writer, ModelFormat format, PointerLUT lut)
		{
			uint posAddress = writer.WriteCollection(Positions, (w, v) => w.WriteVector3(v));
			uint nrmAddress = writer.WriteCollection(Normals, (w, v) => w.WriteVector3(v));

			uint meshAddress = writer.WriteCollection(Meshes,
				(w, m) => m.WriteMeshset(w, format == ModelFormat.SADX, lut),
				(w, m) => m.WriteData(w, lut));

			uint materialAddress = writer.WriteCollection(Materials, (w, m) => m.Write(w));

			uint outAddress = writer.PointerPosition;

			writer.WriteUInt(posAddress);
			writer.WriteUInt(nrmAddress);
			writer.WriteUInt((uint)Positions.Length);
			writer.WriteUInt(meshAddress);
			writer.WriteUInt(materialAddress);
			writer.WriteUShort((ushort)Meshes.Length);
			writer.WriteUShort((ushort)Materials.Length);
			MeshBounds.Write(writer);

			if(format == ModelFormat.SADX)
			{
				writer.WriteUInt(0);
			}

			return outAddress;
		}

		/// <summary>
		/// Reads a BASIC attach from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which the attach is located.</param>
		/// <param name="DX">Whether the attach is from SADX.</param>
		/// <param name="lut">Pointer references to use.</param>
		/// <returns>The BASIC attach that was read.</returns>
		public static BasicAttach Read(EndianStackReader reader, uint address, bool DX, PointerLUT lut)
		{
			BasicAttach onRead()
			{
				ILabeledArray<T> readArray<T>(uint arrayOffset, uint countOffset, string genPrefix, bool shortCount, uint elementSize, EndianIOExtensions.ReadValueDelegate<T> read)
				{
					uint itemCount = shortCount
						? reader.ReadUShort(address + countOffset)
						: reader.ReadUInt(address + countOffset);

					/* === Note regarding Empty arrays here ===
                     * Some modded models in the past appear to have used empty arrays. 
                     * In an effort to support them, we just create a new array for them.
                     * Seeing how this is only for old mod models, its not tragic if
                     * we have to remove it in case they actually break something else.
                     */

					if(itemCount == 0)
					{
						return new LabeledArray<T>(0);
					}

					uint itemAddr = reader.ReadPointer(address + arrayOffset);
					return reader.ReadLabeledArray(itemAddr, itemCount, elementSize, read, genPrefix, lut);
				}

				uint meshSize = DX ? BasicMesh.StructSizeDX : BasicMesh.StructSize;

				ILabeledArray<Vector3> positions /***/ = readArray(0x00, 0x08, "vertex_", false, 12, /*******************/ (r, p) => r.ReadVector3(p));
				ILabeledArray<Vector3> normals /*****/ = readArray(0x04, 0x08, "normal_", false, 12, /*******************/ (r, p) => r.ReadVector3(p));
				ILabeledArray<BasicMesh> meshes /****/ = readArray(0x0C, 0x14, "meshlist_", true, meshSize, /************/ (r, p) => BasicMesh.Read(r, p, lut));
				ILabeledArray<BasicMaterial> materials = readArray(0x10, 0x16, "matlist_", true, BasicMaterial.StructSize, (r, p) => BasicMaterial.Read(r, p));

				Bounds bounds = Bounds.Read(reader, address + 24);

				return new BasicAttach(positions, normals, meshes, materials, bounds);
			}

			return lut.GetAddLabeledValue(address, "attach_", onRead);
		}


		/// <inheritdoc/>
		public override Attach Clone()
		{
			return new BasicAttach(Positions.Clone(), Normals.Clone(), Meshes.ContentClone(), Materials.Clone(), MeshBounds)
			{
				Label = Label
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Label} - BASIC";
		}
	}
}

