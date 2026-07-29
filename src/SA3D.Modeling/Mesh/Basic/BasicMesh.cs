using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common;
using SA3D.Common.Ascii;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.ObjectData.Structs;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Basic
{
	/// <summary>
	/// Mesh data format used by SA1 and SA2
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class BasicMesh : MeshData
	{
		internal class JsonConverter : ChildJsonObjectConverter<MeshFormat, BasicMesh, MeshData>
		{
			private const string _positions = nameof(Positions);
			private const string _normals = nameof(Normals);
			private const string _meshes = nameof(MeshSets);
			private const string _materials = nameof(Materials);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<MeshFormat, MeshData> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _positions, new (PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _normals, new (PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _meshes, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _materials, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(MeshFormat key)
			{
				return key == MeshFormat.Basic;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _positions:
					case _normals:
						return JsonSerializer.Deserialize<LabeledArray<Vector3>>(ref reader, options);
					case _meshes:
						return JsonSerializer.Deserialize<LabeledArray<BasicMeshSet>>(ref reader, options);
					case _materials:
						return JsonSerializer.Deserialize<LabeledArray<BasicMaterial>>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override BasicMesh CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				LabeledArray<Vector3> positions = (LabeledArray<Vector3>?)values[_positions]
					?? throw new InvalidDataException($"Basic attach requires property \"{_positions}\"!");

				LabeledArray<Vector3> normals = (LabeledArray<Vector3>?)values[_normals]
					?? throw new InvalidDataException($"Basic attach requires property \"{_normals}\"!");

				LabeledArray<BasicMeshSet> meshes = (LabeledArray<BasicMeshSet>?)values[_meshes]
					?? throw new InvalidDataException($"Basic attach requires property \"{_meshes}\"!");

				LabeledArray<BasicMaterial> materials = (LabeledArray<BasicMaterial>?)values[_materials]
					?? throw new InvalidDataException($"Basic attach requires property \"{_materials}\"!");

				return new()
				{
					Label = (string)values[BaseJsonConverter._label]!,
					MeshBounds = (Bounds)values[BaseJsonConverter._meshBounds]!,
					Positions = positions,
					Normals = normals,
					MeshSets = meshes,
					Materials = materials
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, BasicMesh value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_positions);
				JsonSerializer.Serialize(writer, value.Positions, options);

				writer.WritePropertyName(_normals);
				JsonSerializer.Serialize(writer, value.Normals, options);

				writer.WritePropertyName(_meshes);
				JsonSerializer.Serialize(writer, value.MeshSets, options);

				writer.WritePropertyName(_materials);
				JsonSerializer.Serialize(writer, value.Materials, options);
			}
		}


		/// <summary>
		/// Label prefix for <see cref="Positions"/>.
		/// </summary>
		public const string PositionsLabelPrefix = "positions_";

		/// <summary>
		/// Label prefix for <see cref="Normals"/>.
		/// </summary>
		public const string NormalsLabelPrefix = "normals_";

		/// <summary>
		/// Label prefix for <see cref="MeshSets"/>.
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
		public LabeledArray<BasicMeshSet> MeshSets { get; set; }

		/// <summary>
		/// Materials for the meshes.
		/// </summary>
		public LabeledArray<BasicMaterial> Materials { get; set; }

		/// <inheritdoc/>
		public override MeshFormat MeshFormat
			=> MeshFormat.Basic;


		/// <summary>
		/// Creates a new, empty basic attach
		/// </summary>
		public BasicMesh() : base()
		{
			string identifier = StringExtensions.GenerateIdentifier();
			Positions = new(PositionsLabelPrefix + identifier, 0);
			MeshSets = new LabeledArray<BasicMeshSet>(MeshesLabelPrefix + identifier, 0);
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

				LabeledArray<T>? result = reader.ReadLabeledObjectArrayAtOffset(read, offset, count, labelPrefix, context.OffsetLUT);

				if(result == null && !allowNull)
				{
					throw reader.ReadNullReference(nameof(BasicMesh), fieldname, offset);
				}

				return result;
			}

			Positions = ReadArray(StructBinaryHelper.ReadVector3, positionsOffset, vertexCount, PositionsLabelPrefix, nameof(Positions), false)!;
			Normals = ReadArray(StructBinaryHelper.ReadVector3, normalsOffset, vertexCount, NormalsLabelPrefix, nameof(Normals), true);
			MeshSets = ReadArray(r => r.ReadObject<BasicMeshSet, IOContext>(context), meshesOffset, meshCount, MeshesLabelPrefix, nameof(MeshSets), false)!;
			Materials = ReadArray(r => r.ReadObject<BasicMaterial>(), materialsOffset, materialCount, MaterialsLabelPrefix, nameof(Materials), false)!;
		}

		/// <inheritdoc/>
		public override void Write(BinaryObjectWriter writer, IOContext context)
		{
			writer.WriteObjectArrayOffset(StructBinaryHelper.WriteVector3, Positions, context.OffsetLUT);
			writer.WriteObjectArrayOffset(StructBinaryHelper.WriteVector3, Normals.EmptyNull(), context.OffsetLUT);
			writer.WriteInt32(Positions.Length);
			writer.WriteObjectArrayOffset(MeshSets, context, context.OffsetLUT);
			writer.WriteObjectArrayOffset(Materials, context.OffsetLUT);
			writer.WriteUInt16((ushort)MeshSets.Length);
			writer.WriteUInt16((ushort)Materials.Length);
			writer.WriteObject(MeshBounds);

			if(context.MeshFormat == Structs.Format.BasicDX)
			{
				writer.WriteUInt32(0);
			}
		}

		/// <inheritdoc/>
		public override void Write(AsciiWriter writer, ModelAsciiIOContext context)
		{
			writer.WriteArray("MATERIAL", Materials, 1);

			foreach(BasicMeshSet meshSet in MeshSets)
			{
				meshSet.WritePolygons(writer);
			}

			writer.WriteArray("MESHSET", MeshSets, 1);
			writer.WriteArray("POINT", Positions, 0, (w, v) => w.WriteLine($"\tVERT( {v.ToAscii()} ),"));
			writer.WriteArray("NORMAL", Normals, 0, (w, v) => w.WriteLine($"\tNORM( {v.ToAscii()} ),"));

			using(writer.WriteStructBlock("MODEL", this))
			{
				writer.WriteObjectPropertyLine("Points", Positions);
				writer.WriteObjectPropertyLine("Normal", Normals);
				writer.WritePropertyLine("PointNum", Positions.Length.ToString());
				writer.WriteObjectPropertyLine("Meshset", MeshSets);
				writer.WriteObjectPropertyLine("Materials", Materials);
				writer.WritePropertyLine("MeshsetNum", MeshSets.Length.ToString());
				writer.WritePropertyLine("MatNum", Materials.Length.ToString());
				writer.WritePropertyLine("Center", MeshBounds.Position.ToAscii());
				writer.WritePropertyLine("Radius", MeshBounds.Radius.ToAscii());
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
				MeshSets = new(MeshSets.Label, [.. MeshSets.Select(x => x.Clone())]),
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

