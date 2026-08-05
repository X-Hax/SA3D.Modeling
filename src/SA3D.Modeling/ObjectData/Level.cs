using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.Converters;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.AnimationData;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.ObjectData
{
	/// <summary>
	/// Stage geometry information
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class Level : ILabel, IBinarySerializable<IOContext>
	{
		private class JsonConverter : SimpleJsonObjectConverter<Level>
		{
			private const string _label = nameof(Label);
			private const string _format = nameof(Format);
			private const string _drawDistance = nameof(DrawDistance);
			private const string _textureFileName = nameof(TextureFileName);
			private const string _textureListOffset = nameof(TextureListAddress);
			private const string _attributes = nameof(Attributes);
			private const string _models = nameof(Models);
			private const string _modelAnimations = nameof(ModelAnimations);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _label, new(PropertyTokenType.String, string.Empty) },
				{ _format, new(PropertyTokenType.String, null) },
				{ _drawDistance, new(PropertyTokenType.Number, 0f) },
				{ _textureFileName, new(PropertyTokenType.String, null, true) },
				{ _textureListOffset, new(PropertyTokenType.String, 0) },
				{ _attributes, new(PropertyTokenType.String, default(LevelAttributes)) },
				{ _models, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _modelAnimations, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
			});


			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _label:
						return reader.GetString();
					case _format:
						return JsonSerializer.Deserialize<Format>(ref reader, options);
					case _drawDistance:
						return reader.GetSingle();
					case _textureFileName:
						return reader.GetString();
					case _textureListOffset:
						return UInt32HexConverter.ConvertFrom(reader.GetString()!, _textureListOffset);
					case _attributes:
						return JsonSerializer.Deserialize<LevelAttributes>(ref reader, options);
					case _models:
						return JsonSerializer.Deserialize<LabeledArray<LevelModel>>(ref reader, options);
					case _modelAnimations:
						return JsonSerializer.Deserialize<LabeledArray<LevelModelAnimation>>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override Level Create(ReadOnlyDictionary<string, object?> values)
			{
				LabeledArray<LevelModel> models = (LabeledArray<LevelModel>?)values[_models]
					?? throw new InvalidDataException($"Level requires \"{_models}\" property");

				Format format = (Format?)values[_format]
					?? throw new InvalidDataException($"Level requires \"{_format}\" property");

				Level result = new()
				{
					Format = format,
					Models = models,
					Label = (string)values[_label]!,
					Attributes = (LevelAttributes)values[_attributes]!,
					DrawDistance = (float)values[_drawDistance]!,
					TextureFileName = (string?)values[_textureFileName],
					TextureListAddress = (uint)values[_textureListOffset]!,
				};

				if(values[_modelAnimations] is LabeledArray<LevelModelAnimation> anims)
				{
					result.ModelAnimations = anims;
				}

				return result;
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, Level value, JsonSerializerOptions options)
			{
				writer.WriteString(_label, value.Label);

				writer.WritePropertyName(_format);
				JsonSerializer.Serialize(writer, value.Format, options);

				writer.WriteNumber(_drawDistance, value.DrawDistance);

				if(value.TextureFileName != null)
				{
					writer.WriteString(_textureFileName, value.TextureFileName);
				}

				if(value.TextureListAddress != 0)
				{
					writer.WriteString(_textureListOffset, UInt32HexConverter.ConvertTo(value.TextureListAddress));
				}

				if(value.Attributes != default)
				{
					writer.WritePropertyName(_attributes);
					JsonSerializer.Serialize(writer, value.Format, options);
				}

				writer.WritePropertyName(_models);
				JsonSerializer.Serialize(writer, value.Models, options);

				if(value.ModelAnimations?.Length > 0)
				{
					writer.WritePropertyName(_modelAnimations);
					JsonSerializer.Serialize(writer, value.ModelAnimations, options);
				}
			}
		}


		#region Properties

		/// <summary>
		/// Label prefix for <see cref="Models"/>
		/// </summary>
		public const string ModelsLabelPrefix = "models_";

		/// <summary>
		/// Label prefix for <see cref="ModelAnimations"/>
		/// </summary>
		public const string ModelAnimationsLabelPrefix = "modelAnimations_";

		/// <inheritdoc/>
		public string LabelPrefix => "level_";

		/// <inheritdoc/>
		public string Label { get; set; }

		/// <summary>
		/// Level geometry
		/// </summary>
		public LabeledArray<LevelModel> Models { get; set; }

		/// <summary>
		/// Geometry animations (sa1)
		/// </summary>
		public LabeledArray<LevelModelAnimation>? ModelAnimations { get; set; }

		/// <summary>
		/// Landtable attributes
		/// </summary>
		public LevelAttributes Attributes { get; set; }

		/// <summary>
		/// Draw distance
		/// </summary>
		public float DrawDistance { get; set; }

		/// <summary>
		/// Texture file name
		/// </summary>
		public string? TextureFileName { get; set; }

		/// <summary>
		/// Texture list address
		/// </summary>
		public uint TextureListAddress { get; set; }

		/// <summary>
		/// Format of the landtable
		/// </summary>
		public Format Format { get; private set; }

		#endregion

		/// <summary>
		/// Creates a new, empty level
		/// </summary>
		public Level()
		{
			string identifier = GenerateIdentifier();

			Label = LabelPrefix + identifier;
			Models = new(ModelsLabelPrefix + identifier, 0);
		}


		/// <summary>
		/// Sorts land entries to be viable for SA2 / SA2B export.
		/// </summary>
		public void SortLandEntries()
		{
			if(Format is not Format.Chunk and not Format.Ginja)
			{
				return;
			}

			LevelModel[] newOrder = [.. Models.OrderByDescending(x => x.SurfaceAttributes.HasFlag(SurfaceAttributes.Visible))];

			for(int i = 0; i < newOrder.Length; i++)
			{
				Models[i] = newOrder[i];
			}
		}


		void IBinarySerializable<IOContext>.Read(BinaryObjectReader reader, IOContext context)
		{
			Format = context.LevelFormat;

			short modelCount = reader.ReadInt16();
			short displayCount = 0;

			if(Format is Format.Chunk or Format.Ginja)
			{
				displayCount = reader.ReadInt16();
				// "direct-display model count" (runtime field)
				reader.Skip(sizeof(short));
			}

			short modelAnimationCount = reader.ReadInt16();

			Attributes = (LevelAttributes)reader.ReadInt16();
			reader.Skip(sizeof(short)); // "is loaded" (runtime field)

			DrawDistance = reader.ReadSingle();

			long modelsOffset = reader.ReadOffsetValue();
			if(modelsOffset == 0)
			{
				throw reader.ReadNullReference(nameof(LevelModel), nameof(Models));
			}

			reader.ReadAtOffset(modelsOffset, () =>
			{
				short baseCount = Format is Format.Chunk or Format.Ginja ? displayCount : modelCount;
				Models = reader.ReadLabeledObjectArray<LevelModel, IOContext>(baseCount, ModelsLabelPrefix, context, context.OffsetLUT);

				if(Format is Format.Chunk or Format.Ginja)
				{
					int collisionCount = modelCount - displayCount;

					IOContext collisionModelContext = new()
					{
						LevelFormat = Format,
						MeshFormat = Format.Basic,
						OffsetLUT = context.OffsetLUT
					};

					LevelModel[] collisionModels = reader.ReadObjectArray<LevelModel, IOContext>(collisionCount, collisionModelContext);
					Models.Array = [.. Models.Array, .. collisionModels];
				}
			});

			ModelAnimations = reader.ReadLabeledObjectArrayOffset<LevelModelAnimation, IOContext>(modelAnimationCount, ModelAnimationsLabelPrefix, context, context.OffsetLUT);

			TextureFileName = reader.ReadStringOffset();
			TextureListAddress = reader.ReadUInt32();
		}

		void IBinarySerializable<IOContext>.Write(BinaryObjectWriter writer, IOContext context)
		{
			short displayCount = 0;

			if(Format is Format.Chunk or Format.Ginja)
			{
				bool visiblesFinished = false;
				foreach(LevelModel model in Models)
				{
					if(model.Model.MeshData == null)
					{
						continue;
					}

					bool isVisible = model.Model.MeshData.MeshFormat is Mesh.MeshFormat.Ginja or Mesh.MeshFormat.Chunk;

					if(isVisible)
					{
						displayCount++;
					}

					if(!visiblesFinished)
					{
						visiblesFinished = !isVisible;
					}
					else if(isVisible)
					{
						throw new FormatException("Level models are not ordered propertly! Visual models need to come before collision models.");
					}
				}
			}

			writer.WriteInt16((short)Models.Length);

			if(Format is Format.Chunk or Format.Ginja)
			{
				writer.WriteInt16(displayCount);
				writer.WriteInt16(0); // direct display models (runtime field)
			}

			writer.WriteInt16((short)(ModelAnimations?.Length ?? 0));
			writer.WriteInt16((short)Attributes);
			writer.WriteInt16(0); // "is loaded" (runtime field)
			writer.WriteSingle(DrawDistance);

			writer.WriteObjectArrayOffset(Models, context, context.OffsetLUT);
			writer.WriteObjectArrayOffset(ModelAnimations, context, context.OffsetLUT);

			writer.WriteStringOffset(StringBinaryFormat.NullTerminated, TextureFileName);
			writer.WriteUInt32(TextureListAddress);

		}


		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Format} Level";
		}
	}
}
