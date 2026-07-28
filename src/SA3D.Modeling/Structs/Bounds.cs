using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.IO;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// Bounding sphere determining the bounds of an object in 3D space.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct Bounds : IEquatable<Bounds>, IBinarySerializable
	{
		private class JsonConverter : SimpleJsonObjectConverter<Bounds>
		{
			private const string _position = nameof(Position);
			private const string _radius = nameof(Radius);

			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _position, new(PropertyTokenType.String, Vector3.Zero) },
				{ _radius, new(PropertyTokenType.Number, 0.0f) }
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				return propertyName switch
				{
					_position => JsonSerializer.Deserialize<Vector3>(ref reader, options),
					_radius => reader.GetSingle(),
					_ => throw new InvalidPropertyException(),
				};
			}

			/// <inheritdoc/>
			protected override Bounds Create(ReadOnlyDictionary<string, object?> values)
			{
				return new(
					(Vector3)values[_position]!,
					(float)values[_radius]!);
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, Bounds value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_position);
				JsonSerializer.Serialize(writer, value.Position, options);
				writer.WriteNumber(_radius, value.Radius);
			}
		}

		private Vector3 _position;

		private float _radius;

		/// <summary>
		/// Position of the Bounds.
		/// </summary>
		public Vector3 Position
		{
			readonly get => _position;
			set
			{
				_position = value;
				RecalculateMatrix();
			}
		}

		/// <summary>
		/// Radius of the Bounds.
		/// </summary>
		public float Radius
		{
			readonly get => _radius;
			set
			{
				_radius = value;
				RecalculateMatrix();
			}
		}

		/// <summary>
		/// Matrix to transform a spherical mesh of diameter 1 to represent the bounds.
		/// </summary>
		public Matrix4x4 Matrix { get; private set; }

		/// <summary>
		/// Creates new bounds from a position and radius
		/// </summary>
		/// <param name="position"></param>
		/// <param name="radius"></param>
		public Bounds(Vector3 position, float radius)
		{
			_position = position;
			_radius = radius;
			Matrix = Matrix4x4.CreateScale(_radius) * Matrix4x4.CreateTranslation(_position);
		}

		private void RecalculateMatrix()
		{
			Matrix = Matrix4x4.CreateScale(_radius) * Matrix4x4.CreateTranslation(_position);
		}

		/// <summary>
		/// Creates the tightest possible bounds from a list of points
		/// </summary>
		/// <param name="points"></param>
		/// <returns></returns>
		public static Bounds FromPoints(IEnumerable<Vector3> points)
		{
			Vector3 position = VectorUtilities.CalculateCenter(points);
			float radius = 0;
			foreach(Vector3 p in points)
			{
				float distance = Vector3.Distance(position, p);
				if(distance > radius)
				{
					radius = distance;
				}
			}

			return new Bounds(position, radius);
		}

		#region I/O

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			Position = reader.ReadVector3();
			Radius = reader.ReadSingle();
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer)
		{
			writer.WriteVector3(Position);
			writer.WriteSingle(Radius);
		}

		#endregion

		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is Bounds bounds &&
				   Position.Equals(bounds.Position) &&
				   Radius == bounds.Radius;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(Position, Radius);
		}

		readonly bool IEquatable<Bounds>.Equals(Bounds other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares the components of 2 bounds for equality.
		/// </summary>
		/// <param name="l">Lefthand bounds.</param>
		/// <param name="r">Righthand bounds.</param>
		/// <returns>Whether the boundss are equal.</returns>
		public static bool operator ==(Bounds l, Bounds r)
		{
			return l.Equals(r);
		}

		/// <summary>
		/// Compares the components of 2 bounds for inequality.
		/// </summary>
		/// <param name="l">Lefthand bounds.</param>
		/// <param name="r">Righthand bounds.</param>
		/// <returns>Whether the boundss are inequal.</returns>
		public static bool operator !=(Bounds l, Bounds r)
		{
			return !l.Equals(r);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{Position} : {Radius}";
		}
	}
}
