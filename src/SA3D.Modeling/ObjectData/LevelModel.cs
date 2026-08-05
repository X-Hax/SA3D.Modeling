using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.Converters;
using SA3D.Common.IO;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.ObjectData.Events;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.ObjectData
{
	/// <summary>
	/// Stage Geometry
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class LevelModel : IBinarySerializable<IOContext>
	{
		private class JsonConverter : SimpleJsonObjectConverter<LevelModel>
		{
			private const string _model = nameof(Model);
			private const string _modelBounds = nameof(ModelBounds);
			private const string _surfaceAttributes = nameof(SurfaceAttributes);
			private const string _blockBit = nameof(BlockBit);
			private const string _unknown = nameof(Unknown);

			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _model, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _modelBounds, new(PropertyTokenType.Object, default(Bounds)) },
				{ _surfaceAttributes, new(PropertyTokenType.String | PropertyTokenType.Number, default(SurfaceAttributes)) },
				{ _blockBit, new(PropertyTokenType.String, 0u) },
				{ _unknown, new(PropertyTokenType.Number, 0u) },

			});


			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _model:
						return JsonSerializer.Deserialize<Node>(ref reader, options);
					case _modelBounds:
						return JsonSerializer.Deserialize<Bounds>(ref reader, options);
					case _surfaceAttributes:
						return JsonSerializer.Deserialize<SurfaceAttributes>(ref reader, options);
					case _blockBit:
						return UInt32HexConverter.ConvertFrom(reader.GetString()!, $"{nameof(LevelModel)}.{_blockBit}");
					case _unknown:
						return reader.GetUInt32();
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override LevelModel Create(ReadOnlyDictionary<string, object?> values)
			{
				Node model = (Node?)values[_model]
					?? throw new InvalidDataException($"Landentry requires \"{_model}\" property");

				return new(model)
				{
					SurfaceAttributes = (SurfaceAttributes)values[_surfaceAttributes]!,
					ModelBounds = (Bounds)values[_modelBounds]!,
					BlockBit = (uint)values[_blockBit]!,
					Unknown = (uint)values[_unknown]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, LevelModel value, JsonSerializerOptions options)
			{
				if(value.BlockBit != 0)
				{
					writer.WriteString(_blockBit, UInt32HexConverter.ConvertTo(value.BlockBit));
				}

				if(value.Unknown != 0)
				{
					writer.WriteNumber(_unknown, value.Unknown);
				}

				writer.WritePropertyName(_surfaceAttributes);
				JsonSerializer.Serialize(writer, value.SurfaceAttributes, options);

				writer.WritePropertyName(_modelBounds);
				JsonSerializer.Serialize(writer, value.ModelBounds, options);

				writer.WritePropertyName(_model);
				JsonSerializer.Serialize(writer, value.Model, options);
			}
		}

		private Node _model;

		/// <summary>
		/// Model behind the landentry.
		/// </summary>
		public Node Model
		{
			get => _model;
			set
			{
				_model.OnTransformsUpdated -= OnTransformsUpdated;
				_model.OnMeshDataUpdated -= OnMeshDataUpdated;

				_model = value;

				_model.OnTransformsUpdated += OnTransformsUpdated;
				_model.OnMeshDataUpdated += OnMeshDataUpdated;
			}
		}

		/// <summary>
		/// World space bounds.
		/// <br/> Get automatically updated when the transforms change.
		/// </summary>
		public Bounds ModelBounds { get; set; }

		/// <summary>
		/// Geometry behavior attributes.
		/// </summary>
		public SurfaceAttributes SurfaceAttributes { get; set; }

		/// <summary>
		/// Block mapping bits
		/// </summary>
		public uint BlockBit { get; set; }

		/// <summary>
		/// No idea what this does at all, might be unused
		/// </summary>
		public uint Unknown { get; set; }

		/// <summary>
		/// Creates a new, empty land entry
		/// </summary>
		public LevelModel() : this(new()) { }

		/// <summary>
		/// Creates a new landentry object.
		/// </summary>
		/// <param name="node">Model behind the landentry.</param>
		public LevelModel(Node node)
		{
			_model = node;
			_model.OnTransformsUpdated += OnTransformsUpdated;
			_model.OnMeshDataUpdated += OnMeshDataUpdated;

			UpdateBounds();
		}


		private void OnMeshDataUpdated(Node node, MeshDataUpdatedEventArgs args)
		{
			UpdateBounds();
		}

		private void OnTransformsUpdated(Node node, TransformsUpdatedEventArgs args)
		{
			UpdateBounds();
		}

		/// <summary>
		/// Copies the Meshdata-bounds and applies the landentries transform matrix to them
		/// </summary>
		public void UpdateBounds()
		{
			if(Model.MeshData == null)
			{
				ModelBounds = default;
				return;
			}

			Vector3 position = Vector3.Transform(Model.MeshData.MeshBounds.Position, Model.QuaternionRotation) + Model.Position;
			float radius = Model.MeshData.MeshBounds.Radius * Model.Scale.GreatestValue();
			ModelBounds = new(position, radius);
		}


		void IBinarySerializable<IOContext>.Read(BinaryObjectReader reader, IOContext context)
		{
			ModelBounds = reader.ReadObject<Bounds>();

			if(context.LevelFormat is Format.Basic or Format.BasicDX)
			{
				reader.Skip(sizeof(float) * 2); // SA1 has unused radius y and radius z values
			}

			Model = reader.ReadObjectOffset<Node, IOContext>(context, context.OffsetLUT)
				?? throw reader.ReadNullReference(nameof(LevelModel), nameof(Model));

			if(context.LevelFormat >= Format.Chunk)
			{
				Unknown = reader.ReadUInt32();
				BlockBit = reader.ReadUInt32();
				SurfaceAttributes = ((SA2SurfaceAttributes)reader.ReadUInt32()).ToUniversal();
			}
			else
			{
				BlockBit = reader.ReadUInt32();
				SurfaceAttributes = ((SA1SurfaceAttributes)reader.ReadUInt32()).ToUniversal();
			}
		}

		void IBinarySerializable<IOContext>.Write(BinaryObjectWriter writer, IOContext context)
		{
			writer.WriteObject(ModelBounds);

			if(context.LevelFormat is Format.Basic or Format.BasicDX)
			{
				writer.Skip(sizeof(float) * 2); // SA1 has unused radius y and radius z values
			}

			writer.WriteObjectOffset(Model, context, context.OffsetLUT);

			if(context.LevelFormat >= Format.Chunk)
			{
				writer.WriteUInt32(Unknown);
				writer.WriteUInt32(BlockBit);
				writer.WriteUInt32((uint)SurfaceAttributes.ToSA2());
			}
			else
			{
				writer.WriteUInt32(BlockBit);
				writer.WriteUInt32((uint)SurfaceAttributes.ToSA1());
			}
		}



		/// <summary>
		/// Creates a copy of the landentry copies the node tree but reuses attaches.
		/// </summary>
		/// <returns></returns>
		public LevelModel Copy()
		{
			return new(Model.DeepSimpleCopy())
			{
				SurfaceAttributes = SurfaceAttributes,
				BlockBit = BlockBit,
				Unknown = Unknown,
				ModelBounds = ModelBounds
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Model.ToString();
		}

	}
}
