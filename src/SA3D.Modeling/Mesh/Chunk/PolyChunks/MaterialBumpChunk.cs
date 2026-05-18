

using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Polychunk with unknown usage.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public class MaterialBumpChunk : SizedChunk
	{
		internal class JsonConverter : ChildJsonObjectConverter<PolyChunkType, MaterialBumpChunk, PolyChunk>
		{
			private const string _dir = nameof(Dir);
			private const string _up = nameof(Up);

			/// <inheritdoc/>
			protected override ParentJsonObjectConverter<PolyChunkType, PolyChunk> ParentConverter => BaseJsonConverter.instance;

			/// <inheritdoc/>
			protected override ReadOnlyDictionary<string, PropertyDefinition> TargetPropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _dir, new(PropertyTokenType.String, default(Vector3)) },
				{ _up, new(PropertyTokenType.String, default(Vector3)) },
			});

			/// <inheritdoc/>
			protected override bool CheckTypeMatches(PolyChunkType key)
			{
				return key == PolyChunkType.Material_Bump;
			}

			/// <inheritdoc/>
			protected override object? ReadTargetValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _dir:
					case _up:
						return JsonSerializer.Deserialize<Vector3>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override MaterialBumpChunk CreateTarget(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					Dir = (Vector3)values[_dir]!,
					Up = (Vector3)values[_up]!,
				};
			}

			/// <inheritdoc/>
			protected override void WriteTargetValues(Utf8JsonWriter writer, MaterialBumpChunk value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_dir);
				JsonSerializer.Serialize(writer, value.Dir, options);

				writer.WritePropertyName(_up);
				JsonSerializer.Serialize(writer, value.Up, options);
			}
		}

		/// <inheritdoc/>
		public override ushort Size => 6;

		/// <summary>
		/// "direction" vector
		/// </summary>
		public Vector3 Dir { get; set; }

		/// <summary>
		/// "up" vector
		/// </summary>
		public Vector3 Up { get; set; }


		/// <summary>
		/// Creates a new material bump chunk.
		/// </summary>
		public MaterialBumpChunk() : base(PolyChunkType.Material_Bump) { }

		/// <inheritdoc/>
		public override void Read(BinaryObjectReader reader)
		{
			base.Read(reader);

			Dir = reader.ReadVector3(FloatIOType.NormalizedShort);
			Up = reader.ReadVector3(FloatIOType.NormalizedShort);
		}

		/// <inheritdoc/>
		protected override void WriteData(BinaryObjectWriter writer)
		{
			base.WriteData(writer);

			writer.WriteVector3(Dir, FloatIOType.NormalizedShort);
			writer.WriteVector3(Up, FloatIOType.NormalizedShort);
		}
	}
}
