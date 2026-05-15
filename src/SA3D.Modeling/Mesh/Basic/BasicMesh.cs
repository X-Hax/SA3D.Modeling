using Amicitia.IO.Binary;
using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Structs;
using System;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Basic
{
	/// <summary>
	/// Mesh data format used by SA1 and SA2
	/// </summary>
	public sealed class BasicMesh : MeshData
	{
		/// <summary>
		/// Label prefix for <see cref="Positions"/>.
		/// </summary>
		public const string PositionsLabelPrefix = "positions_";

		/// <summary>
		/// Label prefix for <see cref="Normals"/>.
		/// </summary>
		public const string NormalsLabelPrefix = "normals_";

		/// <summary>
		/// Label prefix for <see cref="Meshes"/>.
		/// </summary>
		public const string MeshesLabelPrefix = "meshes_";

		/// <summary>
		/// Label prefix for <see cref="Materials"/>.
		/// </summary>
		public const string MaterialsLabelPrefix = "materials_";


		/// <inheritdoc/>
		public override string LabelPrefix => "basicMesh_";

		/// <summary>
		/// Vertex positions.
		/// </summary>
		public LabeledArray<Vector3> Positions { get; set; }

		/// <summary>
		/// Vertex normals.
		/// </summary>
		public LabeledArray<Vector3>? Normals { get; set; }

		/// <summary>
		/// Polygon structures.
		/// </summary>
		public LabeledArray<BasicMeshSet> Meshes { get; set; }

		/// <summary>
		/// Materials for the meshes.
		/// </summary>
		public LabeledArray<BasicMaterial> Materials { get; set; }

		/// <inheritdoc/>
		public override AttachFormat MeshFormat
			=> AttachFormat.Basic;


		/// <summary>
		/// Creates a new, empty basic attach
		/// </summary>
		public BasicMesh() : base()
		{
			string identifier = StringExtensions.GenerateIdentifier();
			Positions = new(PositionsLabelPrefix + identifier, 0);
			Meshes = new LabeledArray<BasicMeshSet>(MeshesLabelPrefix + identifier, 0);
			Materials = new LabeledArray<BasicMaterial>(MaterialsLabelPrefix + identifier, 0);
		}


		/// <inheritdoc/>
		public override void RecalculateBounds()
		{
			MeshBounds = Bounds.FromPoints(Positions);
		}

		/// <inheritdoc/>
		public override bool CanWrite(Format format)
		{
			return format is Structs.Format.Basic or Structs.Format.BasicDX;
		}

		/// <inheritdoc/>
		public override void Read(BinaryObjectReader reader, IOContext context)
		{
			long positionsOffset = reader.ReadOffsetValue();
			long normalsOffset = reader.ReadOffsetValue();
			int vertexCount = reader.ReadInt32();
			long meshesOffset = reader.ReadOffsetValue();
			long materialsOffset = reader.ReadOffsetValue();
			ushort meshCount = reader.ReadUInt16();
			ushort materialCount = reader.ReadUInt16();
			MeshBounds = reader.ReadObject<Bounds>();

			if(context.MeshFormat == Structs.Format.BasicDX)
			{
				reader.Skip(sizeof(int));
			}

			LabeledArray<T>? ReadArray<T>(Func<BinaryObjectReader, T> read, long offset, int count, string labelPrefix, string fieldname, bool allowNull)
			{
				if(count == 0)
				{
					/* === Note regarding Empty arrays here ===
					 Some modded models in the past appear to have used empty arrays. 
					 In an effort to support them, we just create a new array for them.
					 Seeing how this is only for old mod models, its not tragic if
					 we have to remove it in case they actually break something else.
					*/

					return new($"{labelPrefix}{offset:X8}", 0);
				}

				LabeledArray<T>? result = reader.ReadLabeledObjectArrayAtOffset(read, offset, count, labelPrefix, context.PointerLUT);

				if(result == null && !allowNull)
				{
					throw reader.ReadNullReference(nameof(BasicMesh), fieldname, offset);
				}

				return result;
			}

			Positions = ReadArray(StructBinaryHelper.ReadVector3, positionsOffset, vertexCount, PositionsLabelPrefix, nameof(Positions), false)!;
			Normals = ReadArray(StructBinaryHelper.ReadVector3, normalsOffset, vertexCount, NormalsLabelPrefix, nameof(Normals), true);
			Meshes = ReadArray(r => r.ReadObject<BasicMeshSet>(), meshesOffset, meshCount, MeshesLabelPrefix, nameof(Meshes), false)!;
			Materials = ReadArray(r => r.ReadObject<BasicMaterial>(), materialsOffset, materialCount, MaterialsLabelPrefix, nameof(Materials), false)!;
		}

		/// <inheritdoc/>
		public override void Write(BinaryObjectWriter writer, IOContext context)
		{
			writer.WriteObjectArrayOffset(StructBinaryHelper.WriteVector3, Positions, context.PointerLUT);
			writer.WriteObjectArrayOffset(StructBinaryHelper.WriteVector3, Normals.EmptyNull(), context.PointerLUT);
			writer.WriteInt32(Positions.Length);
			writer.WriteObjectArrayOffset(Meshes, context.PointerLUT);
			writer.WriteObjectArrayOffset(Materials, context.PointerLUT);
			writer.WriteUInt16((ushort)Meshes.Length);
			writer.WriteUInt16((ushort)Materials.Length);
			writer.WriteObject(MeshBounds);

			if(context.MeshFormat == Structs.Format.BasicDX)
			{
				writer.WriteUInt32(0);
			}
		}


		/// <inheritdoc/>
		public override MeshData Clone()
		{
			return new BasicMesh()
			{
				Label = Label,
				Positions = Positions.Clone(),
				Normals = Normals?.Clone(),
				Meshes = new(Meshes.Label, Meshes.Select(x => x.Clone()).ToArray()),
				Materials = Materials.Clone(),
				MeshBounds = MeshBounds
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Label} - BASIC";
		}


	}
}

