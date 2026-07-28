using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.Converters;
using SA3D.Common.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.File.MetaData.Weights
{
	/// <summary>
	/// Metadata weight.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct MetaWeight : IEquatable<MetaWeight>, IBinarySerializable
	{
		private class JsonConverter : SimpleJsonObjectConverter<MetaWeight>
		{
			private const string _nodeOffset = nameof(NodeOffset);
			private const string _vertexIndex = nameof(VertexIndex);
			private const string _weight = nameof(Weight);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _nodeOffset, new(PropertyTokenType.Number, 0L) },
				{ _vertexIndex, new(PropertyTokenType.Number, 0u) },
				{ _weight, new(PropertyTokenType.Number, 0f) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _nodeOffset:
						return (long)UInt32HexConverter.ConvertFrom(reader.GetString()!, propertyName);
					case _vertexIndex:
						return reader.GetUInt32();
					case _weight:
						return reader.GetSingle();
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override MetaWeight Create(ReadOnlyDictionary<string, object?> values)
			{
				return new(
					((long?)values[_nodeOffset]!).Value,
					((uint?)values[_vertexIndex]!).Value,
					((float?)values[_weight]!).Value
				);
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, MetaWeight value, JsonSerializerOptions options)
			{
				writer.WriteString(_nodeOffset, UInt32HexConverter.ConvertTo((uint)value.NodeOffset));
				writer.WriteNumber(_vertexIndex, value.VertexIndex);
				writer.WriteNumber(_weight, value.Weight);
			}
		}


		/// <summary>
		/// Pointer to the node that is weighted to.
		/// </summary>
		public long NodeOffset { get; set; }

		/// <summary>
		/// Vertex index to the draw position and normal from.
		/// </summary>
		public uint VertexIndex { get; set; }

		/// <summary>
		/// Influence of the weight.
		/// </summary>
		public float Weight { get; set; }


		/// <summary>
		/// Creates a new meta weight.
		/// </summary>
		/// <param name="nodeOffset">Offset to the node that is weighted to.</param>
		/// <param name="vertexIndex">Vertex cache index.</param>
		/// <param name="weight">Weight.</param>
		public MetaWeight(long nodeOffset, uint vertexIndex, float weight)
		{
			NodeOffset = nodeOffset;
			VertexIndex = vertexIndex;
			Weight = weight;
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			NodeOffset = reader.ReadOffsetValue();
			VertexIndex = reader.ReadUInt32();
			Weight = reader.ReadSingle();
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer)
		{
			writer.WriteOffsetValue(NodeOffset);
			writer.WriteUInt32(VertexIndex);
			writer.WriteSingle(Weight);
		}

		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is MetaWeight weight &&
				   NodeOffset == weight.NodeOffset &&
				   VertexIndex == weight.VertexIndex &&
				   Weight == weight.Weight;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(NodeOffset, VertexIndex, Weight);
		}

		/// <inheritdoc/>
		readonly bool IEquatable<MetaWeight>.Equals(MetaWeight other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two meta weights for equality.
		/// </summary>
		/// <param name="left">Lefthand meta weight.</param>
		/// <param name="right">Righthand meta weight.</param>
		/// <returns>Whether the two meta weights are equal.</returns>
		public static bool operator ==(MetaWeight left, MetaWeight right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two meta weights for inequality.
		/// </summary>
		/// <param name="left">Lefthand meta weight.</param>
		/// <param name="right">Righthand meta weight.</param>
		/// <returns>Whether the two meta weights are inequal.</returns>
		public static bool operator !=(MetaWeight left, MetaWeight right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{NodeOffset:X8} - {VertexIndex} - {Weight:F4}";
		}
	}
}
