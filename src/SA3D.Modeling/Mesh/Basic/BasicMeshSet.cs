using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.Ascii;
using SA3D.Common.IO;
using SA3D.Common.JsonConverters;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Basic.Polygon;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.Mesh.Basic
{
	/// <summary>
	/// BASIC format mesh structure for holding polygon information.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class BasicMeshSet : ICloneable, IBinarySerializable<IOContext>, IAsciiSerializable
	{
		private class JsonConverter : SimpleJsonObjectConverter<BasicMeshSet>
		{
			private const string _materialIndex = nameof(MaterialIndex);
			private const string _polygonType = nameof(PolygonType);
			private const string _polygons = nameof(Polygons);
			private const string _polygonAttributes = nameof(PolygonAttributes);
			private const string _normals = nameof(Normals);
			private const string _colors = nameof(Colors);
			private const string _textureCoordinates = nameof(TextureCoordinates);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _materialIndex, new(PropertyTokenType.Number, 0u) },
				{ _polygonType, new(PropertyTokenType.String, null) },
				{ _polygons, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _polygonAttributes, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _normals, new(PropertyTokenType.Object | PropertyTokenType.String, null, true) },
				{ _colors, new(PropertyTokenType.Object | PropertyTokenType.String, null, true) },
				{ _textureCoordinates, new(PropertyTokenType.Object | PropertyTokenType.String, null, true) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _materialIndex:
						return reader.GetUInt16();
					case _polygonType:
						return JsonSerializer.Deserialize<BasicPolygonType>(ref reader, options);
					case _polygons:
						BasicPolygonType type = (BasicPolygonType?)values[_polygonType]
							?? throw new InvalidDataException($"Basic meshes require property \"{_polygonType}\" before the \"{_polygons}\" array!");

						string label;
						IBasicPolygon[] polygons;

						switch(type)
						{
							case BasicPolygonType.Triangles:
								LabeledArray<BasicTriangle> triangles = JsonSerializer.Deserialize<LabeledArray<BasicTriangle>>(ref reader, options)!;
								label = triangles.Label;
								polygons = triangles.Array.Cast<IBasicPolygon>().ToArray();

								break;
							case BasicPolygonType.Quads:
								LabeledArray<BasicQuad> quads = JsonSerializer.Deserialize<LabeledArray<BasicQuad>>(ref reader, options)!;
								label = quads.Label;
								polygons = quads.Array.Cast<IBasicPolygon>().ToArray();

								break;
							case BasicPolygonType.NPoly:
							case BasicPolygonType.TriangleStrips:
								LabeledArray<BasicMultiPolygon> multiPolygons = JsonSerializer.Deserialize<LabeledArray<BasicMultiPolygon>>(ref reader, options)!;
								label = multiPolygons.Label;
								polygons = multiPolygons.Array.Cast<IBasicPolygon>().ToArray();

								break;
							default:
								throw new InvalidOperationException("Cannot be reached; If reached, basic polygon type somehow invalid.");
						}

						return new LabeledArray<IBasicPolygon>(label, polygons);
					case _polygonAttributes:
						JsonSerializerOptions polygonAttributeOptions = new(options);
						polygonAttributeOptions.Converters.Add(new UInt32HexJsonConverter());
						return JsonSerializer.Deserialize<LabeledArray<uint>>(ref reader, polygonAttributeOptions);
					case _normals:
						return JsonSerializer.Deserialize<LabeledArray<Vector3>>(ref reader, options);
					case _colors:
						return JsonSerializer.Deserialize<LabeledArray<Color>>(ref reader, options);
					case _textureCoordinates:
						return JsonSerializer.Deserialize<LabeledArray<Vector2>>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override BasicMeshSet Create(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					MaterialIndex = (ushort)values[_materialIndex]!,
					PolygonType = (BasicPolygonType)values[_polygonType]!,
					Polygons = (LabeledArray<IBasicPolygon>)values[_polygons]!,
					Normals = (LabeledArray<Vector3>?)values[_normals],
					Colors = (LabeledArray<Color>?)values[_colors],
					TextureCoordinates = (LabeledArray<Vector2>?)values[_textureCoordinates],
					PolygonAttributes = (LabeledArray<uint>?)values[_polygonAttributes]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, BasicMeshSet value, JsonSerializerOptions options)
			{
				writer.WriteNumber(_materialIndex, value.MaterialIndex);

				writer.WritePropertyName(_polygonType);
				JsonSerializer.Serialize(writer, value.PolygonType, options);

				writer.WritePropertyName(_polygons);
				JsonSerializer.Serialize(writer, value.Polygons, options);

				if(value.PolygonAttributes != null)
				{
					writer.WritePropertyName(_polygonAttributes);

					JsonSerializerOptions polygonAttributeOptions = new(options);
					polygonAttributeOptions.Converters.Add(new UInt32HexJsonConverter());
					JsonSerializer.Serialize(writer, value.PolygonAttributes, polygonAttributeOptions);
				}

				if(value.Normals != null)
				{
					writer.WritePropertyName(_normals);
					JsonSerializer.Serialize(writer, value.Normals, options);
				}

				if(value.Colors != null)
				{
					writer.WritePropertyName(_colors);
					JsonSerializer.Serialize(writer, value.Colors, options);
				}

				if(value.TextureCoordinates != null)
				{
					writer.WritePropertyName(_textureCoordinates);
					JsonSerializer.Serialize(writer, value.TextureCoordinates, options);
				}
			}
		}

		/// <summary>
		/// Default label prefix for <see cref="Polygons"/>
		/// </summary>
		public const string PolygonLabelPrefix = "polygons_";

		/// <summary>
		/// Default label prefix for <see cref="PolygonAttributes"/>
		/// </summary>
		public const string PolygonAttributesLabelPrefix = "polygon_attributes_";

		/// <summary>
		/// Default label prefix for <see cref="Normals"/>
		/// </summary>
		public const string NormalsLabelPrefix = "polygon_normals_";

		/// <summary>
		/// Default label prefix for <see cref="Colors"/>
		/// </summary>
		public const string ColorsLabelPrefix = "polygon_colors_";

		/// <summary>
		/// Default label prefix for <see cref="TextureCoordinates"/>
		/// </summary>
		public const string TextureCoordinatesLabelPrefix = "polygon_texcoords_";


		/// <summary>
		/// Index indicating which material to use from <see cref="BasicMesh.Materials"/>.
		/// </summary>
		public ushort MaterialIndex { get; set; }

		/// <summary>
		/// Indicating how polygons are stored.
		/// </summary>
		public BasicPolygonType PolygonType { get; set; }

		/// <summary>
		/// Polygons of the mesh.
		/// </summary>
		public LabeledArray<IBasicPolygon> Polygons { get; set; }

		/// <summary>
		/// Polygon corner attributes
		/// </summary>
		public LabeledArray<uint>? PolygonAttributes { get; set; }

		/// <summary>
		/// Polygon corner normals
		/// </summary>
		public LabeledArray<Vector3>? Normals { get; set; }

		/// <summary>
		/// Polygon corner colors
		/// </summary>
		public LabeledArray<Color>? Colors { get; set; }

		/// <summary>
		/// Polygon corner texture coordinates
		/// </summary>
		public LabeledArray<Vector2>? TextureCoordinates { get; set; }


		/// <summary>
		/// Creates a new, blank basic mesh
		/// </summary>
		public BasicMeshSet()
		{
			Polygons = new LabeledArray<IBasicPolygon>(PolygonLabelPrefix.GenerateIdentifier(), 0);
		}

		/// <summary>
		/// Counts up the number of polygon corners in the mesh set
		/// </summary>
		public int GetPolygonCornerCount()
		{
			return Polygons.Sum(x => x.NumIndices);
		}

		/// <summary>
		/// Counts up the number of individual polygons in the mesh
		/// </summary>
		public int GetPolygonCount()
		{
			if(PolygonType == BasicPolygonType.TriangleStrips)
			{
				return Polygons.Sum(x => x.NumIndices - 2);
			}
			else
			{
				return Polygons.Length;
			}
		}

		/// <summary>
		/// Checks whether polygon data is valid and throws an <see cref="InvalidDataException"/> if not.
		/// </summary>
		/// <exception cref="InvalidDataException"></exception>
		public void VerifyPolygonData()
		{
			Type expectedPolygonType = PolygonType switch
			{
				BasicPolygonType.Triangles => typeof(BasicTriangle),
				BasicPolygonType.Quads => typeof(BasicQuad),
				BasicPolygonType.NPoly or BasicPolygonType.TriangleStrips => typeof(BasicMultiPolygon),
				_ => throw new InvalidDataException($"Invalid polygon type \"{PolygonType}\"!"),
			};

			if(Polygons.Any(x => x.GetType() != expectedPolygonType))
			{
				throw new InvalidDataException($"Not all polygons are of the expected type {expectedPolygonType}!");
			}

			if(PolygonAttributes == null && Normals == null && Colors == null && TextureCoordinates == null)
			{
				return;
			}

			int polygonCount = GetPolygonCount();
			int cornerCount = GetPolygonCornerCount();

			if(PolygonAttributes != null && PolygonAttributes.Length < polygonCount)
			{
				throw new InvalidDataException($"Mesh has {polygonCount} polygons, but {PolygonAttributes.Length} polygon attributes!");
			}

			if(Normals != null && Normals.Length < polygonCount)
			{
				throw new InvalidDataException($"Mesh has {polygonCount} polygons, but {Normals.Length} normals!");
			}

			if(Colors != null && Colors.Length < cornerCount)
			{
				throw new InvalidDataException($"Mesh has {cornerCount} corners, but {Colors.Length} colors!");
			}

			if(TextureCoordinates != null && TextureCoordinates.Length < cornerCount)
			{
				throw new InvalidDataException($"Mesh has {cornerCount} corners, but {TextureCoordinates.Length} texture coordinates!");
			}
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, IOContext context)
		{
			ushort header = reader.ReadUInt16();
			MaterialIndex = (ushort)(header & 0x3FFFu);
			PolygonType = (BasicPolygonType)(header >> 14);

			ushort polyCount = reader.ReadUInt16();

			Polygons = reader.ReadLabeledObjectArrayOffset(IBasicPolygon.GetReader(PolygonType), polyCount, "poly_", context.PointerLUT)
				?? new(PolygonLabelPrefix.GenerateIdentifier(), 0);

			int polygonCount = GetPolygonCount();
			int cornerCount = GetPolygonCornerCount();

			PolygonAttributes = reader.ReadLabeledObjectArrayOffset(r => r.ReadUInt32(), polygonCount, PolygonAttributesLabelPrefix, context.PointerLUT);
			Normals = reader.ReadLabeledObjectArrayOffset(StructBinaryHelper.ReadVector3, polygonCount, NormalsLabelPrefix, context.PointerLUT);
			Colors = reader.ReadLabeledObjectArrayOffset(r => r.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_32), cornerCount, ColorsLabelPrefix, context.PointerLUT);
			TextureCoordinates = reader.ReadLabeledObjectArrayOffset(FloatIOType.Short.GetVector2Reader(), cornerCount, TextureCoordinatesLabelPrefix, context.PointerLUT);
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, IOContext context)
		{
			VerifyPolygonData();

			ushort header = (ushort)((MaterialIndex & 0x3FFFu) | (ushort)((int)PolygonType << 14));
			writer.WriteUInt16(header);
			writer.WriteUInt16((ushort)Polygons.Length);
			writer.WriteObjectArrayOffset(Polygons.EmptyNull(), context.PointerLUT);
			writer.WriteObjectArrayOffset((w, v) => w.WriteUInt32(v), PolygonAttributes, context.PointerLUT);
			writer.WriteObjectArrayOffset(StructBinaryHelper.WriteVector3, Normals.EmptyNull(), context.PointerLUT);
			writer.WriteObjectArrayOffset((w, v) => w.WriteObject(v, ColorIOType.ARGB8_32), Colors.EmptyNull(), context.PointerLUT);
			writer.WriteObjectArrayOffset(FloatIOType.Short.GetVector2Writer(), TextureCoordinates.EmptyNull(), context.PointerLUT);
		}

		/// <summary>
		/// Write polygon data to an ascii writer
		/// </summary>
		/// <param name="writer">The writer to write to</param>
		public void WritePolygons(AsciiWriter writer)
		{
			string polyType = PolygonType == BasicPolygonType.TriangleStrips ? "STRIP" : "POLY";

			writer.WriteArray("POLYGON", Polygons, 0);
			writer.WriteArray(polyType + "ATTR", PolygonAttributes, 0, (w, v) => w.WriteLine($"\tNORM( {v.ToAsciiHex()} ),"));
			writer.WriteArray(polyType + "NORMAL", Normals, 0, (w, v) => w.WriteLine($"\tPNORM( {v.ToAscii()} ),"));
			writer.WriteArray("VERTCOLOR", Colors, 0, (w, v) => w.WriteLine($"\tARGB( {v.Alpha}, {v.Red}, {v.Green}, {v.Blue} ),"));
			writer.WriteArray("VERTUV", TextureCoordinates, 0, (w, v) => w.WriteLine($"\tUV( {(int)v.X}, {(int)v.Y} ),"));
		}

		/// <inheritdoc/>
		public void Write(AsciiWriter writer)
		{
			using(writer.WriteBlock("MESH"))
			{
				writer.WritePropertyLine("TypeMatId", $"( 0x{(ushort)(((int)PolygonType) << 14):x2}, {MaterialIndex} )");
				writer.WritePropertyLine("MeshNum", Polygons.Length.ToString());
				writer.WriteObjectPropertyLine("Meshes", Polygons.EmptyNull());
				writer.WriteObjectPropertyLine("PolyAttrs", PolygonAttributes.EmptyNull());
				writer.WriteObjectPropertyLine("PolyNormal", Normals.EmptyNull());
				writer.WriteObjectPropertyLine("VertColor", Colors.EmptyNull());
				writer.WriteObjectPropertyLine("VertUV", TextureCoordinates.EmptyNull());
			}
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the mesh.
		/// </summary>
		/// <returns>The clone.</returns>
		public BasicMeshSet Clone()
		{
			return new()
			{
				MaterialIndex = MaterialIndex,
				PolygonAttributes = PolygonAttributes,
				PolygonType = PolygonType,
				Polygons = new LabeledArray<IBasicPolygon>(Polygons.Label, [.. Polygons.Select(x => (IBasicPolygon)x.Clone())]),
				Normals = Normals?.Clone(),
				Colors = Colors?.Clone(),
				TextureCoordinates = TextureCoordinates?.Clone()
			};
		}


	}
}
