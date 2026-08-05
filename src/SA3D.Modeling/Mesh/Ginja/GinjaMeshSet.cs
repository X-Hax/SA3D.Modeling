using Amicitia.IO;
using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Ginja.Enums;
using SA3D.Modeling.Mesh.Ginja.Parameters;
using SA3D.Modeling.Mesh.Ginja.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Ginja
{
	/// <summary>
	/// A single mesh, with its own parameter and primitive data <br/>
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class GinjaMeshSet : ICloneable, IBinarySerializable<GinjaIOContext>
	{
		private class JsonConverter : SimpleJsonObjectConverter<GinjaMeshSet>
		{
			private const string _parameters = nameof(Parameters);
			private const string _polygons = nameof(Polygons);

			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _parameters, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _polygons, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _parameters:
						return JsonSerializer.Deserialize<LabeledArray<IGinjaParameter>>(ref reader, options);
					case _polygons:
						return JsonSerializer.Deserialize<GinjaPolygonArray>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override GinjaMeshSet Create(ReadOnlyDictionary<string, object?> values)
			{
				LabeledArray<IGinjaParameter> parameters = (LabeledArray<IGinjaParameter>?)values[_parameters]
					?? throw new InvalidDataException($"GinjaMeshSet requires property \"{_parameters}\"!");

				GinjaPolygonArray polygons = (GinjaPolygonArray?)values[_polygons]
					?? throw new InvalidDataException($"GinjaMeshSet requires property \"{_polygons}\"!");

				return new()
				{
					Parameters = parameters,
					Polygons = polygons
				};
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, GinjaMeshSet value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_parameters);
				JsonSerializer.Serialize(writer, value.Parameters, options);

				writer.WritePropertyName(_polygons);
				JsonSerializer.Serialize(writer, value.Polygons, options);
			}
		}

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
		public LabeledArray<IGinjaParameter>? Parameters { get; set; }

		/// <summary>
		/// The polygon data.
		/// </summary>
		public GinjaPolygonArray? Polygons { get; set; }


		/// <summary>
		/// Create a new empty meshset
		/// </summary>
		public GinjaMeshSet()
		{
			string identifier = StringExtensions.GenerateIdentifier();

			Parameters = new(ParametersLabelPrefix + identifier, 0);
			Polygons = new() { Label = PolygonsLabelPrefix + identifier };
		}

		/// <summary>
		/// Returns the index format of the first 
		/// </summary>
		/// <returns></returns>
		public GinjaIndexFormat? GetIndexFormat()
		{
			if(Parameters == null)
			{
				return null;
			}

			foreach(GinjaIndexFormatParameter param in Parameters.OfType<GinjaIndexFormatParameter>())
			{
				return param.IndexFormat;
			}

			return null;
		}


		void IBinarySerializable<GinjaIOContext>.Read(BinaryObjectReader reader, GinjaIOContext context)
		{
			string identifier = StringExtensions.GenerateIdentifier();

			long parametersOffset = reader.ReadOffsetValue();
			int parametersCount = reader.ReadInt32();

			long polygonsOffset = reader.ReadOffsetValue();
			int polygonsSize = reader.ReadInt32();

			Parameters = reader.ReadLabeledObjectArrayAtOffset(IGinjaParameter.ReadParameter, parametersOffset, parametersCount, ParametersLabelPrefix, context.BaseContext.OffsetLUT);

			context.IndexFormat = GetIndexFormat() ?? context.IndexFormat;

			Polygons = reader.ReadObjectAtOffset<GinjaPolygonArray, (GinjaIndexFormat format, int size)>(polygonsOffset, (context.IndexFormat, polygonsSize), context.BaseContext.OffsetLUT);
		}

		void IBinarySerializable<GinjaIOContext>.Write(BinaryObjectWriter writer, GinjaIOContext context)
		{
			context.IndexFormat = GetIndexFormat() ?? context.IndexFormat;
			GinjaIndexFormat currentIndexFormat = context.IndexFormat;

			writer.WriteObjectOffset(Parameters.EmptyNull(), (w, v) =>
			{
				long alignOrigin = w.Position;
				w.WriteObjectArray(v);
				w.Align(0x20, alignOrigin);
			}, context.BaseContext.OffsetLUT);

			writer.WriteInt32(Parameters?.Length ?? 0);

			writer.WriteObjectOffset(Polygons.EmptyNull(), (w, v) =>
			{
				long alignOrigin = w.Position;
				w.WriteObject(v, (currentIndexFormat, 0));
				w.Align(0x20, alignOrigin);
			}, context.BaseContext.OffsetLUT);

			int cornerSize = GinjaPolygonArray.GetIndexSizes(currentIndexFormat).Sum();
			int size = Polygons?.Sum(x => (x.Corners.Length * cornerSize) + 3) ?? 0;
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
				Parameters = Parameters?.Clone(),
				Polygons = Polygons == null ? null : new(Polygons.Select(x => x.Clone())) { Label = Polygons.Label }
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			GinjaIndexFormat? format = GetIndexFormat();
			return (format.HasValue ? ((uint)format.Value).ToString("X8") : "NULL") + $" - {Parameters?.Length ?? 0} - {Polygons?.Length ?? 0}";
		}


	}
}
