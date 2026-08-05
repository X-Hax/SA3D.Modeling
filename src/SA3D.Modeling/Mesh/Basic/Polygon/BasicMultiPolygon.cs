using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.Ascii;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Basic.Polygon
{
	/// <summary>
	/// A BASIC polygon containing a variable number of indices.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct BasicMultiPolygon : IBasicPolygon
	{
		private class JsonConverter : SimpleJsonObjectConverter<BasicMultiPolygon>
		{
			private const string _reversed = nameof(Reversed);
			private const string _indices = nameof(Indices);

			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _reversed, new(PropertyTokenType.Bool, false) },
				{ _indices, new(PropertyTokenType.Array, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				return propertyName switch
				{
					_reversed => reader.GetBoolean(),
					_indices => JsonSerializer.Deserialize<ushort[]>(ref reader, options),
					_ => throw new InvalidPropertyException(),
				};
			}

			/// <inheritdoc/>
			protected override BasicMultiPolygon Create(ReadOnlyDictionary<string, object?> values)
			{
				bool reversed = (bool)values[_reversed]!;

				ushort[] indices = (ushort[]?)values[_indices]
					?? throw new InvalidDataException("Multipolygon requires indices!");

				return new(indices, reversed);
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, BasicMultiPolygon value, JsonSerializerOptions options)
			{
				writer.WriteBoolean(_reversed, value.Reversed);

				writer.WritePropertyName(_indices);
				JsonSerializer.Serialize(writer, value.Indices, options);
			}
		}

		/// <summary>
		/// Polygon corner information
		/// </summary>
		public ushort[] Indices { get; set; }

		/// <summary>
		/// Whether the backface culling direction is flipped.
		/// </summary>
		public bool Reversed { get; set; }

		/// <inheritdoc/>
		public readonly uint Size => (uint)((1 + Indices.Length) * sizeof(ushort));

		/// <inheritdoc/>
		public readonly int NumIndices => Indices.Length;


		/// <inheritdoc/>
		public readonly ushort this[int index]
		{
			get => Indices[index];
			set => Indices[index] = value;
		}

		/// <summary>
		/// Creates a new multi polygon.
		/// </summary>
		/// <param name="corners">Indices of the polygon.</param>
		/// <param name="reversed">Whether the polygons backface culling direction is flipped.</param>
		public BasicMultiPolygon(ushort[] corners, bool reversed)
		{
			Indices = corners;
			Reversed = reversed;
		}

		/// <summary>
		/// Creates a new empty multi polygon.
		/// </summary>
		/// <param name="size">Number of indices the polygon holds.</param>
		/// <param name="reversed">Whether the polygons backface culling direction is flipped.</param>
		public BasicMultiPolygon(uint size, bool reversed)
			: this(new ushort[size], reversed) { }


		void IBinarySerializable.Read(BinaryObjectReader reader)
		{
			ushort header = reader.ReadUInt16();
			Indices = new ushort[header & 0x7FFF];
			Reversed = (header & 0x8000) != 0;
			for(int i = 0; i < Indices.Length; i++)
			{
				Indices[i] = reader.ReadUInt16();
			}
		}

		readonly void IBinarySerializable.Write(BinaryObjectWriter writer)
		{
			writer.WriteUInt16((ushort)((Indices.Length & 0x7FFF) | (Reversed ? 0x8000 : 0)));
			for(int i = 0; i < Indices.Length; i++)
			{
				writer.WriteUInt16(Indices[i]);
			}
		}

		readonly void IAsciiSerializable.Write(AsciiWriter writer)
		{
			writer.Write($"\tStrip(0x{(Reversed ? "8000" : "0")}, {Indices.Length}), ");

			if(Indices.Length > 10)
			{
				writer.WriteLine();
				writer.Write("\t\t");
			}

			for(int i = 0; i < Indices.Length; i++)
			{
				writer.Write($"{Indices[i]}, ");

				if(i > 0 && i % 10 == 0)
				{
					writer.WriteLine();
					writer.Write("\t\t");
				}
			}

			writer.WriteLine();
		}

		/// <inheritdoc/>
		public readonly IEnumerator<ushort> GetEnumerator()
		{
			return ((IEnumerable<ushort>)Indices).GetEnumerator();
		}

		/// <inheritdoc/>
		readonly IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		/// <inheritdoc/>
		public readonly object Clone()
		{
			return new BasicMultiPolygon((ushort[])Indices.Clone(), Reversed);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Multi: {Reversed} - {Indices.Length}";
		}

	}
}
