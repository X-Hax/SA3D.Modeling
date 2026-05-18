using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.Lookup;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.Mesh
{
	/// <summary>
	/// 3D mesh data. Its possible for multiple attaches to make up one full mesh.
	/// </summary>
	[JsonConverter(typeof(BaseJsonConverter))]
	public abstract class MeshData : ICloneable, ILabel, IBinarySerializable<IOContext>
	{
		internal class BaseJsonConverter : ParentJsonObjectConverter<MeshFormat, MeshData>
		{
			public static readonly BaseJsonConverter instance = new();

			public const string _meshFormat = nameof(MeshFormat);
			public const string _label = nameof(Label);
			public const string _meshBounds = nameof(MeshBounds);

			/// <inheritdoc/>
			protected override string KeyPropertyName => _meshFormat;

			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _meshFormat, new(PropertyTokenType.String, null) },
				{ _label, new(PropertyTokenType.String, string.Empty) },
				{ _meshBounds, new(PropertyTokenType.Object, default(Bounds)) }
			});


			/// <inheritdoc/>
			protected override object? ReadBaseValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _meshFormat:
						return JsonSerializer.Deserialize<MeshFormat>(ref reader, options);
					case _label:
						return reader.GetString();
					case _meshBounds:
						return JsonSerializer.Deserialize<Bounds>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override MeshData CreateBase(ReadOnlyDictionary<string, object?> values)
			{
				throw new NotSupportedException();
			}

			/// <inheritdoc/>
			protected override void WriteBaseValues(Utf8JsonWriter writer, MeshData value, JsonSerializerOptions options)
			{
				writer.WritePropertyName(_meshFormat);
				JsonSerializer.Serialize(writer, value.MeshFormat, options);

				writer.WriteString(_label, value.Label);

				if(value.MeshBounds != default)
				{
					writer.WritePropertyName(_meshBounds);
					JsonSerializer.Serialize(writer, value.MeshBounds, options);
				}
			}

			/// <inheritdoc/>
			protected override MeshFormat GetKeyFromValue(MeshData value)
			{
				return value.MeshFormat;
			}

			/// <inheritdoc/>
			protected override Dictionary<MeshFormat, IChildJsonConverter<MeshData>> CreateConverters()
			{
				return new()
				{
					{ MeshFormat.Basic, new Basic.BasicMesh.JsonConverter() },
					{ MeshFormat.Ginja, new Ginja.GinjaMesh.JsonConverter() },
					{ MeshFormat.Chunk, new Chunk.ChunkMesh.JsonConverter() },
				};
			}
		}

		/// <inheritdoc/>
		public abstract string LabelPrefix { get; }

		/// <inheritdoc/>
		public string Label { get; set; }

		/// <summary>
		/// Format of the attach.
		/// </summary>
		public abstract MeshFormat MeshFormat { get; }

		/// <summary>
		/// Bounding sphere of the attach.
		/// </summary>
		public Bounds MeshBounds { get; set; }


		/// <summary>
		/// Base constructor for derived attach types.
		/// </summary>
		protected MeshData()
		{
			Label = LabelPrefix.GenerateIdentifier();
		}


		/// <summary>
		/// Checks whether the attaches mesh data has/relies on weights.
		/// </summary>
		/// <returns>Whether the attaches mesh data has/relies on weights</returns>
		public virtual bool CheckHasWeights()
		{
			return false;
		}

		/// <summary>
		/// Recalculates <see cref="Bounds"/> from the attach data.
		/// </summary>
		public abstract void RecalculateBounds();

		/// <summary>
		/// Checks whether the attach can be written in the given model format.
		/// </summary>
		/// <param name="format">The format to check.</param>
		/// <returns>Whether the model can be written.</returns>
		public abstract bool CanWrite(Format format);


		/// <inheritdoc/>
		public abstract void Read(BinaryObjectReader reader, IOContext context);

		/// <inheritdoc/>
		public abstract void Write(BinaryObjectWriter writer, IOContext context);


		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the attach.
		/// </summary>
		/// <returns>The cloned attach.</returns>
		public abstract MeshData Clone();

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Label} - Buffer";
		}
	}
}
