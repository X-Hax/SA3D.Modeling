using Amicitia.IO;
using Amicitia.IO.Binary;
using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Ginja.Enums;
using SA3D.Modeling.Mesh.Ginja.Parameters;
using System;
using System.Linq;

namespace SA3D.Modeling.Mesh.Ginja
{
	/// <summary>
	/// A single mesh, with its own parameter and primitive data <br/>
	/// </summary>
	public class GinjaMeshSet : ICloneable, IBinarySerializable<GinjaIOContext>
	{
		/// <summary>
		/// Label prefix for <see cref="Parameters"/>
		/// </summary>
		public const string ParametersLabelPrefix = "parameters_";

		/// <summary>
		/// Label prefix for <see cref="Polygons"/>
		/// </summary>
		public const string PolygonsLabelPrefix = "polygons_";

		/// <summary>
		/// The data parameters.
		/// </summary>
		public LabeledArray<IGinjaParameter> Parameters { get; set; }

		/// <summary>
		/// The polygon data.
		/// </summary>
		public LabeledArray<GinjaPolygon> Polygons { get; set; }


		/// <summary>
		/// Create a new empty meshset
		/// </summary>
		public GinjaMeshSet()
		{
			string identifier = StringExtensions.GenerateIdentifier();

			Parameters = new(ParametersLabelPrefix + identifier, 0);
			Polygons = new(PolygonsLabelPrefix + identifier, 0);
		}

		/// <summary>
		/// Returns the index format of the first 
		/// </summary>
		/// <returns></returns>
		public GinjaIndexFormat? GetIndexFormat()
		{
			foreach(GinjaIndexFormatParameter param in Parameters.OfType<GinjaIndexFormatParameter>())
			{
				return param.IndexFormat;
			}

			return null;
		}


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, GinjaIOContext context)
		{
			string identifier = StringExtensions.GenerateIdentifier();

			long parametersOffset = reader.ReadOffsetValue();
			int parametersCount = reader.ReadInt32();

			long polygonsOffset = reader.ReadOffsetValue();
			int polygonsSize = reader.ReadInt32();

			Parameters = reader.ReadLabeledObjectArrayAtOffset(IGinjaParameter.ReadParameter, parametersOffset, parametersCount, ParametersLabelPrefix, context.BaseContext.PointerLUT)
				?? new(ParametersLabelPrefix + identifier, 0);

			context.IndexFormat = GetIndexFormat() ?? context.IndexFormat;

			Polygons = reader.ReadLUTItemAtOffset(polygonsOffset, context.BaseContext.PointerLUT, PolygonsLabelPrefix,
				(r) => GinjaPolygon.ReadArray(r, polygonsSize, context.IndexFormat))
				?? new(PolygonsLabelPrefix + identifier, 0);
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, GinjaIOContext context)
		{
			context.IndexFormat = GetIndexFormat() ?? context.IndexFormat;
			GinjaIndexFormat currentIndexFormat = context.IndexFormat;

			writer.WriteObjectOffset(Parameters.EmptyNull(), (w, v) =>
			{
				long alignOrigin = w.Position;
				w.WriteObjectArray(v);
				w.Align(0x20, alignOrigin);
			}, context.BaseContext.PointerLUT);

			writer.WriteInt32(Parameters.Length);

			writer.WriteObjectOffset(Polygons.EmptyNull(), (w, v) =>
			{
				long alignOrigin = w.Position;
				w.WriteObjectArray(v, currentIndexFormat);
				w.Align(0x20, alignOrigin);
			}, context.BaseContext.PointerLUT);

			int cornerSize = GinjaPolygon.GetIndexSizes(currentIndexFormat).Sum();
			int size = Polygons.Sum(x => (x.Corners.Length * cornerSize) + 3);
			size = AlignmentHelper.Align(size, 0x20);

			writer.WriteInt32(size);
		}


		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a clone of the mesh.
		/// </summary>
		/// <returns>The cloned mesh.</returns>
		public GinjaMeshSet Clone()
		{
			return new()
			{
				Parameters = Parameters.Clone(),
				Polygons = Polygons.ContentClone()
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			GinjaIndexFormat? format = GetIndexFormat();
			return (format.HasValue ? ((uint)format.Value).ToString("X8") : "NULL") + $" - {Parameters.Length} - {Polygons.Length}";
		}


	}
}
