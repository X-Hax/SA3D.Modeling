using J113D.Json;
using SA3D.Modeling.Mesh.Ginja.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Ginja.Structs
{
	/// <summary>
	/// A single corner of a polygon, called loop
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	[JsonConverter(typeof(JsonConverter))]
	public struct GinjaCorner : IEquatable<GinjaCorner>
	{
		private class JsonConverter : SimpleJsonObjectConverter<GinjaCorner>
		{
			private const string _positionMatrixIDIndex = nameof(PositionMatrixIDIndex);
			private const string _positionIndex = nameof(PositionIndex);
			private const string _normalIndex = nameof(NormalIndex);
			private const string _color0Index = nameof(Color0Index);
			private const string _color1Index = nameof(Color1Index);
			private const string _texCoord0Index = nameof(TexCoord0Index);
			private const string _texCoord1Index = nameof(TexCoord1Index);
			private const string _texCoord2Index = nameof(TexCoord2Index);
			private const string _texCoord3Index = nameof(TexCoord3Index);
			private const string _texCoord4Index = nameof(TexCoord4Index);
			private const string _texCoord5Index = nameof(TexCoord5Index);
			private const string _texCoord6Index = nameof(TexCoord6Index);
			private const string _texCoord7Index = nameof(TexCoord7Index);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
		{
			{ _positionMatrixIDIndex, new(PropertyTokenType.Number, (ushort)0u) },
			{ _positionIndex, new(PropertyTokenType.Number, (ushort)0u) },
			{ _normalIndex, new(PropertyTokenType.Number, (ushort)0u) },
			{ _color0Index, new(PropertyTokenType.Number, (ushort)0u) },
			{ _color1Index, new(PropertyTokenType.Number, (ushort)0u) },
			{ _texCoord0Index, new(PropertyTokenType.Number, (ushort)0u) },
			{ _texCoord1Index, new(PropertyTokenType.Number, (ushort)0u) },
			{ _texCoord2Index, new(PropertyTokenType.Number, (ushort)0u) },
			{ _texCoord3Index, new(PropertyTokenType.Number, (ushort)0u) },
			{ _texCoord4Index, new(PropertyTokenType.Number, (ushort)0u) },
			{ _texCoord5Index, new(PropertyTokenType.Number, (ushort)0u) },
			{ _texCoord6Index, new(PropertyTokenType.Number, (ushort)0u) },
			{ _texCoord7Index, new(PropertyTokenType.Number, (ushort)0u) },
		});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _positionMatrixIDIndex:
					case _positionIndex:
					case _normalIndex:
					case _color0Index:
					case _color1Index:
					case _texCoord0Index:
					case _texCoord1Index:
					case _texCoord2Index:
					case _texCoord3Index:
					case _texCoord4Index:
					case _texCoord5Index:
					case _texCoord6Index:
					case _texCoord7Index:
						return reader.GetUInt16();
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override GinjaCorner Create(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					PositionMatrixIDIndex = (ushort)values[_positionMatrixIDIndex]!,
					PositionIndex = (ushort)values[_positionIndex]!,
					NormalIndex = (ushort)values[_normalIndex]!,
					Color0Index = (ushort)values[_color0Index]!,
					Color1Index = (ushort)values[_color1Index]!,
					TexCoord0Index = (ushort)values[_texCoord0Index]!,
					TexCoord1Index = (ushort)values[_texCoord1Index]!,
					TexCoord2Index = (ushort)values[_texCoord2Index]!,
					TexCoord3Index = (ushort)values[_texCoord3Index]!,
					TexCoord4Index = (ushort)values[_texCoord4Index]!,
					TexCoord5Index = (ushort)values[_texCoord5Index]!,
					TexCoord6Index = (ushort)values[_texCoord6Index]!,
					TexCoord7Index = (ushort)values[_texCoord7Index]!,
				};
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, GinjaCorner value, JsonSerializerOptions options)
			{
				void writeNumber(string name, ushort number)
				{
					if(number != 0)
					{
						writer.WriteNumber(name, number);
					}
				}

				writeNumber(_positionMatrixIDIndex, value.PositionMatrixIDIndex);
				writeNumber(_positionIndex, value.PositionIndex);
				writeNumber(_normalIndex, value.NormalIndex);
				writeNumber(_color0Index, value.Color0Index);
				writeNumber(_color1Index, value.Color1Index);
				writeNumber(_texCoord0Index, value.TexCoord0Index);
				writeNumber(_texCoord1Index, value.TexCoord1Index);
				writeNumber(_texCoord2Index, value.TexCoord2Index);
				writeNumber(_texCoord3Index, value.TexCoord3Index);
				writeNumber(_texCoord4Index, value.TexCoord4Index);
				writeNumber(_texCoord5Index, value.TexCoord5Index);
				writeNumber(_texCoord6Index, value.TexCoord6Index);
				writeNumber(_texCoord7Index, value.TexCoord7Index);
			}
		}

		/// <summary>
		/// The index to <see cref="GinjaVertexType.PositionMatrixID"/>.
		/// </summary>
		public ushort PositionMatrixIDIndex { get; set; }

		/// <summary>
		/// The index to <see cref="GinjaVertexType.Position"/>.
		/// </summary>
		public ushort PositionIndex { get; set; }

		/// <summary>
		/// The index to <see cref="GinjaVertexType.Normal"/>.
		/// </summary>
		public ushort NormalIndex { get; set; }

		/// <summary>
		/// The index to <see cref="GinjaVertexType.Color0"/>.
		/// </summary>
		public ushort Color0Index { get; set; }

		/// <summary>
		/// The index to <see cref="GinjaVertexType.Color1"/>.
		/// </summary>
		public ushort Color1Index { get; set; }

		/// <summary>
		/// The index to <see cref="GinjaVertexType.TexCoord0"/>.
		/// </summary>
		public ushort TexCoord0Index { get; set; }

		/// <summary>
		/// The index to <see cref="GinjaVertexType.TexCoord1"/>.
		/// </summary>
		public ushort TexCoord1Index { get; set; }

		/// <summary>
		/// The index to <see cref="GinjaVertexType.TexCoord2"/>.
		/// </summary>
		public ushort TexCoord2Index { get; set; }

		/// <summary>
		/// The index to <see cref="GinjaVertexType.TexCoord3"/>.
		/// </summary>
		public ushort TexCoord3Index { get; set; }

		/// <summary>
		/// The index to <see cref="GinjaVertexType.TexCoord4"/>.
		/// </summary>
		public ushort TexCoord4Index { get; set; }

		/// <summary>
		/// The index to <see cref="GinjaVertexType.TexCoord5"/>.
		/// </summary>
		public ushort TexCoord5Index { get; set; }

		/// <summary>
		/// The index to <see cref="GinjaVertexType.TexCoord6"/>.
		/// </summary>
		public ushort TexCoord6Index { get; set; }

		/// <summary>
		/// The index to <see cref="GinjaVertexType.TexCoord7"/>.
		/// </summary>
		public ushort TexCoord7Index { get; set; }


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is GinjaCorner corner &&
				   PositionMatrixIDIndex == corner.PositionMatrixIDIndex &&
				   PositionIndex == corner.PositionIndex &&
				   NormalIndex == corner.NormalIndex &&
				   Color0Index == corner.Color0Index &&
				   Color1Index == corner.Color1Index &&
				   TexCoord0Index == corner.TexCoord0Index &&
				   TexCoord1Index == corner.TexCoord1Index &&
				   TexCoord2Index == corner.TexCoord2Index &&
				   TexCoord3Index == corner.TexCoord3Index &&
				   TexCoord4Index == corner.TexCoord4Index &&
				   TexCoord5Index == corner.TexCoord5Index &&
				   TexCoord6Index == corner.TexCoord6Index &&
				   TexCoord7Index == corner.TexCoord7Index;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			HashCode hash = new();
			hash.Add(PositionMatrixIDIndex);
			hash.Add(PositionIndex);
			hash.Add(NormalIndex);
			hash.Add(Color0Index);
			hash.Add(Color1Index);
			hash.Add(TexCoord0Index);
			hash.Add(TexCoord1Index);
			hash.Add(TexCoord2Index);
			hash.Add(TexCoord3Index);
			hash.Add(TexCoord4Index);
			hash.Add(TexCoord5Index);
			hash.Add(TexCoord6Index);
			hash.Add(TexCoord7Index);
			return hash.ToHashCode();
		}

		readonly bool IEquatable<GinjaCorner>.Equals(GinjaCorner other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two GC corners for equality.
		/// </summary>
		/// <param name="left">Lefthand corner.</param>
		/// <param name="right">Righthand corner.</param>
		/// <returns>Whether the two corners are equal.</returns>
		public static bool operator ==(GinjaCorner left, GinjaCorner right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two GC corners for inequality.
		/// </summary>
		/// <param name="left">Lefthand corner.</param>
		/// <param name="right">Righthand corner.</param>
		/// <returns>Whether the two corners are inequal.</returns>
		public static bool operator !=(GinjaCorner left, GinjaCorner right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"({PositionIndex}, {NormalIndex}, {Color0Index}, {TexCoord0Index})";
		}
	}
}
