using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common;
using SA3D.Common.Ascii;
using SA3D.Common.Converters;
using SA3D.Common.IO;
using SA3D.Modeling.Mesh.Chunk.Structs;
using SA3D.Modeling.ObjectData.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Chunk containing a volume build from polygons.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class VolumeChunk : SizedChunk
	{
		internal class JsonConverter : ChildJsonObjectConverter<PolyChunkType, VolumeChunk, PolyChunk>
		{
			private const string _attributes = nameof(Attributes);
			private const string _polygons = nameof(Polygons);
			private const string _polygonAttributeCount = nameof(PolygonAttributeCount);


			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<PolyChunkType, PolyChunk> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _attributes, new(PropertyTokenType.String, (byte)0) },
				{ _polygons, new(PropertyTokenType.Array, null) },
				{ _polygonAttributeCount, new(PropertyTokenType.Number, 0) },
			});


			/// <inheritdoc/>
			protected override bool CheckTypeMatches(PolyChunkType key)
			{
				return key is >= PolyChunkType.Volume_Triangle and <= PolyChunkType.Volume_Strip;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _attributes:
						return UInt8HexConverter.ConvertFrom(reader.GetString()!, propertyName);
					case _polygons:
						PolyChunkType type = (PolyChunkType)values[BaseJsonConverter._type]!;

						if(type == PolyChunkType.Volume_Triangle)
						{
							return JsonSerializer.Deserialize<ChunkVolumeTriangle[]>(ref reader, options)!.Cast<IChunkVolumePolygon>().ToArray();
						}
						else if(type == PolyChunkType.Volume_Quad)
						{
							return JsonSerializer.Deserialize<ChunkVolumeQuad[]>(ref reader, options)!.Cast<IChunkVolumePolygon>().ToArray();
						}
						else if(type == PolyChunkType.Volume_Strip)
						{
							return JsonSerializer.Deserialize<ChunkVolumeStrip[]>(ref reader, options)!.Cast<IChunkVolumePolygon>().ToArray();
						}

						throw new InvalidOperationException("Cannot be reached; If reached, volume type somehow invalid.");

					case _polygonAttributeCount:
						return reader.GetInt32();
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override VolumeChunk CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				PolyChunkType type = (PolyChunkType)values[BaseJsonConverter._type]!;
				int polygonAttributeCount = (int)values[_polygonAttributeCount]!;

				IChunkVolumePolygon[] polygons = (IChunkVolumePolygon[]?)values[_polygons]
					?? throw new InvalidDataException($"Volume chunk requires \"{_polygons}\" property!");

				return new()
				{
					Type = type,
					Polygons = polygons,
					PolygonAttributeCount = polygonAttributeCount,
					Attributes = (byte)values[_attributes]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, VolumeChunk value, JsonSerializerOptions options)
			{
				if(value.Attributes != 0)
				{
					writer.WriteString(_attributes, value.Attributes.ToString("X", CultureInfo.InvariantCulture));
				}

				if(value.PolygonAttributeCount != 0)
				{
					writer.WriteNumber(_polygonAttributeCount, value.PolygonAttributeCount);
				}

				writer.WritePropertyName(_polygons);
				JsonSerializer.Serialize(writer, value.Polygons, options);
			}
		}

		/// <summary>
		/// Polygons of the volume
		/// </summary>
		public IChunkVolumePolygon[] Polygons { get; set; }

		/// <summary>
		/// User attribute count (ranges from 0 to 3)
		/// </summary>
		public int PolygonAttributeCount
		{
			get;
			set
			{
				if(value is < 0 or > 3)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "Value out of range. Must be between 0 and 3.");
				}

				field = value;
			}
		}

		/// <inheritdoc/>
		public override ushort Size
		{
			get
			{
				uint result = CalculateByteSize() / 2;

				if(result > ushort.MaxValue)
				{
					throw new InvalidOperationException($"Strip chunk size ({result}) exceeds maximum size ({ushort.MaxValue}).");
				}

				return (ushort)uint.Clamp(CalculateByteSize() / 2, 0, ushort.MaxValue);
			}
		}


		/// <summary>
		/// Creates a new empty volume chunk (using <see cref="PolyChunkType.Volume_Triangle"/>)
		/// </summary>
		public VolumeChunk() : base(PolyChunkType.Volume_Triangle)
		{
			Polygons = [];
		}


		/// <inheritdoc/>
		protected override bool IsTypeApplicable(PolyChunkType type)
		{
			return type is >= PolyChunkType.Volume_Triangle and <= PolyChunkType.Volume_Strip;
		}


		/// <summary>
		/// Changes the type of the volume chunk.
		/// </summary>
		public void ChangeType(PolyChunkType type)
		{
			Type = type;
		}

		/// <summary>
		/// Checks whether polygon data is valid and throws an <see cref="InvalidDataException"/> if not.
		/// </summary>
		/// <exception cref="InvalidDataException"></exception>
		public void VerifyPolygonData()
		{
			Type expectedPolygonType = Type switch
			{
				PolyChunkType.Volume_Triangle => typeof(ChunkVolumeTriangle),
				PolyChunkType.Volume_Quad => typeof(ChunkVolumeQuad),
				PolyChunkType.Volume_Strip => typeof(ChunkVolumeStrip),
				_ => throw new InvalidDataException(),
			};

			if(Polygons.Any(x => x.GetType() != expectedPolygonType))
			{
				throw new InvalidDataException($"Not all polygons are of the expected type {expectedPolygonType}!");
			}

			if(Type == PolyChunkType.Volume_Strip)
			{
				foreach(ChunkVolumeStrip strip in Polygons.Cast<ChunkVolumeStrip>())
				{
					strip.VerifyPolygonData();
				}
			}
		}

		/// <summary>
		/// Calculate the chunks size.
		/// </summary>
		/// <returns></returns>
		public uint CalculateByteSize()
		{
			uint result = 2; // header ushort; strip count and triangle attributes

			result += Type switch
			{
				PolyChunkType.Volume_Triangle => (ushort)(Polygons.Length * (6u + (PolygonAttributeCount * 2u))),
				PolyChunkType.Volume_Quad => (ushort)(Polygons.Length * (8u + (PolygonAttributeCount * 2u))),
				PolyChunkType.Volume_Strip => (ushort)Polygons.Sum(x => 2u + (2 * (x.NumIndices + ((x.NumIndices - 2) * PolygonAttributeCount)))),
				_ => throw new InvalidDataException(),
			};

			return result;
		}

		private void WriteCheck()
		{
			if(Polygons.Length > 0x3FFF)
			{
				throw new InvalidOperationException($"Poly count ({Polygons.Length}) exceeds maximum ({0x3FFF})");
			}
		}

		/// <inheritdoc/>
		protected override void Read(BinaryObjectReader reader)
		{
			base.Read(reader);

			ushort data = reader.ReadUInt16();
			int polygonCount = data & 0x3FFF;
			PolygonAttributeCount = (byte)(data >> 14);

			Polygons = Type switch
			{
				PolyChunkType.Volume_Triangle => reader.ReadObjectArray<ChunkVolumeTriangle>(polygonCount).Cast<IChunkVolumePolygon>().ToArray(),
				PolyChunkType.Volume_Quad => reader.ReadObjectArray<ChunkVolumeQuad>(polygonCount).Cast<IChunkVolumePolygon>().ToArray(),
				PolyChunkType.Volume_Strip => reader.ReadObjectArray<ChunkVolumeStrip>(polygonCount).Cast<IChunkVolumePolygon>().ToArray(),
				_ => throw new InvalidOperationException(),
			};
		}

		/// <inheritdoc/>
		protected override void Write(BinaryObjectWriter writer)
		{
			WriteCheck();
			VerifyPolygonData();

			base.Write(writer);

			writer.WriteUInt16((ushort)(Polygons.Length | (PolygonAttributeCount << 14)));

			foreach(IChunkVolumePolygon p in Polygons)
			{
				p.Write(writer, PolygonAttributeCount);
			}
		}

		/// <inheritdoc/>
		protected override string GetAsciiAttributes()
		{
			return "0x0";
		}

		/// <inheritdoc/>
		protected override void Write(AsciiWriter writer, ModelAsciiIOContext context)
		{
			WriteCheck();
			VerifyPolygonData();
			base.Write(writer, context);
			writer.WriteLine($" _NB( UFO_{PolygonAttributeCount}, {Polygons.Length} ),");

			foreach(IChunkVolumePolygon p in Polygons)
			{
				p.Write(writer, (context, PolygonAttributeCount));
			}
		}

		/// <inheritdoc/>
		public override VolumeChunk Clone()
		{
			return new()
			{
				Type = Type,
				Attributes = Attributes,
				Polygons = Polygons.ContentClone(),
				PolygonAttributeCount = PolygonAttributeCount
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Type} - {PolygonAttributeCount} : {Polygons.Length}";
		}


	}
}
