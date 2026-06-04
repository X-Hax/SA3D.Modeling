using Amicitia.IO.Binary;
using SA3D.Common.Ascii;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Basic.Polygon
{
	/// <summary>
	/// A polygon with three index.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct BasicTriangle : IBasicPolygon
	{
		private class JsonConverter : JsonConverter<BasicTriangle>
		{
			/// <inheritdoc/>
			public override BasicTriangle Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
			{
				if(reader.TokenType != JsonTokenType.StartArray)
				{
					throw new InvalidDataException("Expected an array for BasicTriangle!");
				}

				ushort[] indices = JsonSerializer.Deserialize<ushort[]>(ref reader, options)!;

				if(indices.Length < 3)
				{
					throw new InvalidDataException("BasicTriangle has too few indices! At least 3 needed!");
				}

				return new(indices[0], indices[1], indices[2]);
			}

			/// <inheritdoc/>
			public override void Write(Utf8JsonWriter writer, BasicTriangle value, JsonSerializerOptions options)
			{
				writer.WriteStartArray();
				writer.WriteNumberValue(value.Index1);
				writer.WriteNumberValue(value.Index2);
				writer.WriteNumberValue(value.Index3);
				writer.WriteEndArray();
			}
		}

		/// <inheritdoc/>
		public readonly uint Size => 6;

		/// <inheritdoc/>
		public readonly int NumIndices => 3;


		/// <summary>
		/// First vertex index.
		/// </summary>
		public ushort Index1 { get; set; }

		/// <summary>
		/// Second vertex index.
		/// </summary>
		public ushort Index2 { get; set; }

		/// <summary>
		/// Third vertex index.
		/// </summary>
		public ushort Index3 { get; set; }


		/// <inheritdoc/>
		public ushort this[int index]
		{
			readonly get => index switch
			{
				0 => Index1,
				1 => Index2,
				2 => Index3,
				_ => throw new IndexOutOfRangeException(),
			};
			set
			{
				switch(index)
				{
					case 0:
						Index1 = value;
						break;
					case 1:
						Index2 = value;
						break;
					case 2:
						Index3 = value;
						break;
					default:
						throw new IndexOutOfRangeException();
				}
			}
		}

		/// <summary>
		/// Creates a new populated basic quad.
		/// </summary>
		/// <param name="index1">First vertex index.</param>
		/// <param name="index2">Second vertex index.</param>
		/// <param name="index3">Third vertex index.</param>
		public BasicTriangle(ushort index1, ushort index2, ushort index3)
		{
			Index1 = index1;
			Index2 = index2;
			Index3 = index3;
		}


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			Index1 = reader.ReadUInt16();
			Index2 = reader.ReadUInt16();
			Index3 = reader.ReadUInt16();
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer)
		{
			writer.WriteUInt16(Index1);
			writer.WriteUInt16(Index2);
			writer.WriteUInt16(Index3);
		}

		/// <inheritdoc/>
		public readonly void Write(AsciiWriter writer)
		{
			writer.WriteLine($"\t{Index1}, {Index2}, {Index3},");
		}


		/// <inheritdoc/>
		public readonly IEnumerator<ushort> GetEnumerator()
		{
			yield return Index1;
			yield return Index2;
			yield return Index3;
		}

		readonly IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc/>
		public readonly object Clone()
		{
			return this;
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Triangle: [{Index1}, {Index2}, {Index3}]";
		}


	}
}
