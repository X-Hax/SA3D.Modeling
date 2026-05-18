using Amicitia.IO.Binary;
using J113D.Json;
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
	/// Vertex with weights
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct MetaWeightVertex : IEquatable<MetaWeightVertex>, IBinarySerializable
	{
		private class JsonConverter : SimpleJsonObjectConverter<MetaWeightVertex>
		{
			private const string _destinationVertexIndex = nameof(DestinationVertexIndex);
			private const string _weights = nameof(Weights);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _destinationVertexIndex, new(PropertyTokenType.Number, 0) },
				{ _weights, new(PropertyTokenType.Array, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _destinationVertexIndex:
						return reader.GetUInt32();
					case _weights:
						return JsonSerializer.Deserialize<MetaWeight[]>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override MetaWeightVertex Create(ReadOnlyDictionary<string, object?> values)
			{
				uint destinationVertexIndex = (uint?)values[_destinationVertexIndex]
					?? throw new InvalidDataException($"MetaWeightVertex requires a \"{_destinationVertexIndex}\" property");

				MetaWeight[] weights = (MetaWeight[]?)values[_weights]
					?? throw new InvalidDataException($"MetaWeightVertex requires a \"{_weights}\" property");

				return new(destinationVertexIndex, weights);
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, MetaWeightVertex value, JsonSerializerOptions options)
			{
				writer.WriteNumber(_destinationVertexIndex, value.DestinationVertexIndex);

				writer.WritePropertyName(_weights);
				JsonSerializer.Serialize(writer, value.Weights, options);
			}
		}

		/// <summary>
		/// Index to the vertex that the weights influence.
		/// </summary>
		public uint DestinationVertexIndex { get; set; }

		/// <summary>
		/// Weights for the vertex.
		/// </summary>
		public MetaWeight[] Weights { get; set; }


		/// <summary>
		/// Creates a new meta weight vertex.
		/// </summary>
		/// <param name="destinationVertexIndex">Index to the vertex that the weights influence.</param>
		/// <param name="weights">Weights for the vertex.</param>
		public MetaWeightVertex(uint destinationVertexIndex, MetaWeight[] weights)
		{
			DestinationVertexIndex = destinationVertexIndex;
			Weights = weights;
		}


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			DestinationVertexIndex = reader.ReadUInt32();
			int weightCount = reader.ReadInt32();
			Weights = reader.ReadObjectArray<MetaWeight>(weightCount);
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer)
		{
			writer.WriteUInt32(DestinationVertexIndex);
			writer.WriteInt32(Weights.Length);
			writer.WriteObjectArray(Weights);
		}


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is MetaWeightVertex vertex &&
				   DestinationVertexIndex == vertex.DestinationVertexIndex &&
				   EqualityComparer<MetaWeight[]>.Default.Equals(Weights, vertex.Weights);
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(DestinationVertexIndex, Weights);
		}

		readonly bool IEquatable<MetaWeightVertex>.Equals(MetaWeightVertex other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two meta weight vertices for equality.
		/// </summary>
		/// <param name="left">Lefthand meta weight vertex.</param>
		/// <param name="right">Righthand meta weight vertex.</param>
		/// <returns>Whether the two meta weight vertices are equal.</returns>
		public static bool operator ==(MetaWeightVertex left, MetaWeightVertex right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two meta weight vertices for inequality.
		/// </summary>
		/// <param name="left">Lefthand meta weight vertex.</param>
		/// <param name="right">Righthand meta weight vertex.</param>
		/// <returns>Whether the two meta weight vertices are inequal.</returns>
		public static bool operator !=(MetaWeightVertex left, MetaWeightVertex right)
		{
			return !(left == right);
		}


		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{DestinationVertexIndex} - {Weights.Length}";
		}
	}
}
