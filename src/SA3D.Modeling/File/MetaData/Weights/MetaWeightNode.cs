using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.Converters;
using SA3D.Common.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.File.MetaData.Weights
{
	/// <summary>
	/// Node with weight influence info.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct MetaWeightNode : IEquatable<MetaWeightNode>, IBinarySerializable
	{
		private class JsonConverter : SimpleJsonObjectConverter<MetaWeightNode>
		{
			private const string _nodeOffset = nameof(NodeOffset);
			private const string _vertexWeights = nameof(VertexWeights);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _nodeOffset, new(PropertyTokenType.Number, 0L) },
				{ _vertexWeights, new(PropertyTokenType.Array, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _nodeOffset:
						return (long)UInt32HexConverter.ConvertFrom(reader.GetString()!, propertyName);
					case _vertexWeights:
						return JsonSerializer.Deserialize<MetaWeightVertex[]>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override MetaWeightNode Create(ReadOnlyDictionary<string, object?> values)
			{
				long nodeOffset = (long?)values[_nodeOffset]
					?? throw new InvalidDataException($"MetaWeightNode requires a \"{_nodeOffset}\" property");

				MetaWeightVertex[] vertexWeights = (MetaWeightVertex[]?)values[_vertexWeights]
					?? throw new InvalidDataException($"MetaWeightNode requires a \"{_vertexWeights}\" property");

				return new(nodeOffset, vertexWeights);
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, MetaWeightNode value, JsonSerializerOptions options)
			{
				writer.WriteString(_nodeOffset, UInt32HexConverter.ConvertTo((uint)value.NodeOffset));

				writer.WritePropertyName(_vertexWeights);
				JsonSerializer.Serialize(writer, value.VertexWeights, options);
			}
		}

		/// <summary>
		/// Offset to the node being weighted.
		/// </summary>
		public long NodeOffset { get; set; }

		/// <summary>
		/// Weight influences.
		/// </summary>
		public MetaWeightVertex[] VertexWeights { get; set; }


		/// <summary>
		/// Creates a new meta weight node.
		/// </summary>
		/// <param name="nodeOffset">Offset to the node being weighted.</param>
		/// <param name="vertexWeights">Weight influences.</param>
		public MetaWeightNode(long nodeOffset, MetaWeightVertex[] vertexWeights)
		{
			NodeOffset = nodeOffset;
			VertexWeights = vertexWeights;
		}


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			NodeOffset = reader.ReadOffsetValue();
			int vertexCount = reader.ReadInt32();
			VertexWeights = reader.ReadObjectArray<MetaWeightVertex>(vertexCount);
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer)
		{
			writer.WriteOffsetValue(NodeOffset);
			writer.WriteInt32(VertexWeights.Length);
			writer.WriteObjectArray(VertexWeights);
		}


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is MetaWeightNode node &&
				   NodeOffset == node.NodeOffset &&
				   EqualityComparer<MetaWeightVertex[]>.Default.Equals(VertexWeights, node.VertexWeights);
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(NodeOffset, VertexWeights);
		}

		/// <inheritdoc/>
		readonly bool IEquatable<MetaWeightNode>.Equals(MetaWeightNode other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two meta weight nodes for equality.
		/// </summary>
		/// <param name="left">Lefthand meta weight node.</param>
		/// <param name="right">Righthand meta weight node.</param>
		/// <returns>Whether the two meta weight nodes are equal.</returns>
		public static bool operator ==(MetaWeightNode left, MetaWeightNode right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two meta weight nodes for inequality.
		/// </summary>
		/// <param name="left">Lefthand meta weight node.</param>
		/// <param name="right">Righthand meta weight node.</param>
		/// <returns>Whether the two meta weight nodes are inequal.</returns>
		public static bool operator !=(MetaWeightNode left, MetaWeightNode right)
		{
			return !(left == right);
		}


		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{NodeOffset:X8} - {VertexWeights.Length}";
		}
	}
}
