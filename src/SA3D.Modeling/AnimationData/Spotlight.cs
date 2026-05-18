using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.AnimationData
{
	/// <summary>
	/// Spotlight for cutscenes.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public struct Spotlight : IBinarySerializable
	{
		private class JsonConverter : SimpleJsonObjectConverter<Spotlight>
		{
			private const string _near = nameof(Near);
			private const string _far = nameof(Far);
			private const string _insideAngle = nameof(InsideAngle);
			private const string _outsideAngle = nameof(OutsideAngle);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _near, new(PropertyTokenType.Number, 0f) },
				{ _far, new(PropertyTokenType.Number, 0f) },
				{ _insideAngle, new(PropertyTokenType.Number, 0f) },
				{ _outsideAngle, new(PropertyTokenType.Number, 0f) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _near:
					case _far:
					case _insideAngle:
					case _outsideAngle:
						return reader.GetSingle();
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override Spotlight Create(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					Near = (float)values[_near]!,
					Far = (float)values[_far]!,
					InsideAngle = (float)values[_insideAngle]!,
					OutsideAngle = (float)values[_outsideAngle]!,
				};
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, Spotlight value, JsonSerializerOptions options)
			{
				writer.WriteNumber(_near, value.Near);
				writer.WriteNumber(_far, value.Far);
				writer.WriteNumber(_insideAngle, value.InsideAngle);
				writer.WriteNumber(_outsideAngle, value.OutsideAngle);
			}
		}

		/// <summary>
		/// Closest light distance.
		/// </summary>
		public float Near { get; set; }

		/// <summary>
		/// Furthest light distance.
		/// </summary>
		public float Far { get; set; }

		/// <summary>
		/// Inner cone angle.
		/// </summary>
		public float InsideAngle { get; set; }

		/// <summary>
		/// Outer cone angle.
		/// </summary>
		public float OutsideAngle { get; set; }

		/// <summary>
		/// Linearly interpolate between two spotlights.
		/// </summary>
		/// <param name="from">Spotlight from which to start interpolating.</param>
		/// <param name="to">Spotlight to which to interpolate.</param>
		/// <param name="time">Value by which to interpolate</param>
		/// <returns>The interpolated spotlight.</returns>
		public static Spotlight Lerp(Spotlight from, Spotlight to, float time)
		{
			float inverse = 1 - time;
			return new Spotlight()
			{
				Near = (to.Near * time) + (from.Near * inverse),
				Far = (to.Far * time) + (from.Far * inverse),
				InsideAngle = (to.InsideAngle * time) + (from.InsideAngle * inverse),
				OutsideAngle = (to.OutsideAngle * time) + (from.OutsideAngle * inverse),
			};
		}

		/// <summary>
		/// Calculates the distance between two spotlight values (handled like a Vector4).
		/// </summary>
		/// <param name="from">First spotlight.</param>
		/// <param name="to">Second spotlight.</param>
		/// <returns>The distance</returns>
		public static float Distance(Spotlight from, Spotlight to)
		{
			return Vector4.Distance(
				new(from.Near, from.Far, from.InsideAngle, from.OutsideAngle),
				new(to.Near, to.Far, to.InsideAngle, to.OutsideAngle)
			);
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			Near = reader.ReadSingle();
			Far = reader.ReadSingle();
			InsideAngle = reader.ReadSingle(FloatIOType.BAMS32);
			OutsideAngle = reader.ReadSingle(FloatIOType.BAMS32);
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer)
		{
			writer.WriteSingle(Near);
			writer.WriteSingle(Far);
			writer.WriteSingle(InsideAngle, FloatIOType.BAMS32);
			writer.WriteSingle(OutsideAngle, FloatIOType.BAMS32);
		}
	}
}
