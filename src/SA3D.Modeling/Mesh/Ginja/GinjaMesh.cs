using Amicitia.IO.Binary;
using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Ginja.Enums;
using SA3D.Modeling.Structs;
using System.Linq;
using System.Numerics;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.Mesh.Ginja
{
	/// <summary>
	/// A GC format attach
	/// </summary>
	public sealed class GinjaMesh : MeshData
	{
		/// <summary>
		/// Label prefix for <see cref="VertexData"/>
		/// </summary>
		public const string VertexDataLabelPrefix = "vertex_";

		/// <summary>
		/// Label prefix for <see cref="OpaqueMeshes"/>
		/// </summary>
		public const string OpaqueMeshesLabelPrefix = "opaque_";

		/// <summary>
		/// Label prefix for <see cref="TransparentMeshes"/>
		/// </summary>
		public const string TransparentMeshesLabelPrefix = "transparent_";

		/// <summary>
		/// Seperate sets of vertex data in this attach.
		/// </summary>
		public LabeledArray<GinjaVertexSet>? VertexData { get; set; }

		/// <summary>
		/// Meshes with opaque rendering properties.
		/// </summary>
		public LabeledArray<GinjaMeshSet>? OpaqueMeshes { get; set; }

		/// <summary>
		/// Meshes with transparent rendering properties.
		/// </summary>
		public LabeledArray<GinjaMeshSet>? TransparentMeshes { get; set; }

		/// <inheritdoc/>
		public override AttachFormat MeshFormat
			=> AttachFormat.Ginja;

		/// <inheritdoc/>
		public override string LabelPrefix => "ginjaMesh_";


		/// <summary>
		/// Creates a new, empty ginja mesh
		/// </summary>
		public GinjaMesh() : base()
		{
			VertexData = new(VertexDataLabelPrefix.GenerateIdentifier(), 0);
			OpaqueMeshes = null;
			TransparentMeshes = null;
		}


		/// <inheritdoc/>
		public override void RecalculateBounds()
		{
			GinjaVertexSet? positions = VertexData?.FirstOrDefault(x => x.Type == GinjaVertexType.Position);
			Vector3[]? data = positions?.GetDataAsVector3(0);
			MeshBounds = data == null ? default : Bounds.FromPoints(data);
		}

		/// <inheritdoc/>
		public override bool CanWrite(Format format)
		{
			return format is Format.Ginja;
		}


		/// <inheritdoc/>
		public override void Read(BinaryObjectReader reader, IOContext context)
		{
			long vertexOffset = reader.ReadOffsetValue();
			_ = reader.ReadOffsetValue(); // vertex weights; we dont support it
			long opaqueOffset = reader.ReadOffsetValue();
			long transparentOffset = reader.ReadOffsetValue();

			short opaqueCount = reader.ReadInt16();
			short transparentCount = reader.ReadInt16();
			MeshBounds = reader.ReadObject<Bounds>();

			VertexData = reader.ReadLUTItemAtOffset(vertexOffset, context.PointerLUT, VertexDataLabelPrefix, (r) => GinjaVertexSet.ReadArray(r, context));
			OpaqueMeshes = reader.ReadLabeledObjectArrayAtOffset<GinjaMeshSet, GinjaIOContext>(opaqueOffset, opaqueCount, OpaqueMeshesLabelPrefix, new(context), context.PointerLUT);
			TransparentMeshes = reader.ReadLabeledObjectArrayAtOffset<GinjaMeshSet, GinjaIOContext>(transparentOffset, transparentCount, TransparentMeshesLabelPrefix, new(context), context.PointerLUT);
		}

		/// <inheritdoc/>
		public override void Write(BinaryObjectWriter writer, IOContext context)
		{
			writer.WriteObjectOffset(VertexData.EmptyNull(), (w, v) => GinjaVertexSet.WriteArray(w, v, context), context.PointerLUT);
			writer.WriteOffsetValue(0);
			writer.WriteObjectArrayOffset<GinjaMeshSet, GinjaIOContext>(OpaqueMeshes.EmptyNull(), new(context), context.PointerLUT);
			writer.WriteObjectArrayOffset<GinjaMeshSet, GinjaIOContext>(TransparentMeshes.EmptyNull(), new(context), context.PointerLUT);
			writer.WriteInt16((short)(OpaqueMeshes?.Length ?? 0));
			writer.WriteInt16((short)(TransparentMeshes?.Length ?? 0));
			writer.WriteObject(MeshBounds);
		}


		/// <inheritdoc/>
		public override GinjaMesh Clone()
		{
			return new()
			{
				Label = Label,
				MeshBounds = MeshBounds,
				VertexData = VertexData?.ContentClone(),
				OpaqueMeshes = OpaqueMeshes?.ContentClone(),
				TransparentMeshes = TransparentMeshes?.ContentClone(),
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Label} - GC: {VertexData?.Length ?? 0} - {OpaqueMeshes?.Length ?? 0} - {TransparentMeshes?.Length ?? 0}";
		}
	}
}
