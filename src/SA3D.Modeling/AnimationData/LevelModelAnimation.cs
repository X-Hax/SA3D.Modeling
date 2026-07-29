using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.Converters;
using SA3D.Common.IO;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.AnimationData
{
	/// <summary>
	/// Level geometry animation (only used in sa1)
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class LevelModelAnimation : IBinarySerializable<IOContext>
	{
		private class JsonConverter : SimpleJsonObjectConverter<LevelModelAnimation>
		{
			private const string _frame = nameof(Frame);
			private const string _step = nameof(Step);
			private const string _maxFrame = nameof(MaxFrame);
			private const string _model = nameof(Model);
			private const string _animation = nameof(Animation);
			private const string _textureListAddress = nameof(TextureListAddress);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _frame, new(PropertyTokenType.Number, 0f) },
				{ _step, new(PropertyTokenType.Number, 0f) },
				{ _maxFrame, new(PropertyTokenType.Number, 0f) },
				{ _model, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _animation, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _textureListAddress, new(PropertyTokenType.String, 0u) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _frame:
					case _step:
					case _maxFrame:
						return reader.GetSingle();
					case _model:
						return JsonSerializer.Deserialize<Node>(ref reader, options);
					case _animation:
						return JsonSerializer.Deserialize<ModelAnimation>(ref reader, options);
					case _textureListAddress:
						return UInt32HexConverter.ConvertFrom(reader.GetString()!, propertyName);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override LevelModelAnimation Create(ReadOnlyDictionary<string, object?> values)
			{
				Node model = (Node?)values[_model]
					?? throw new InvalidDataException($"LevelModelAnimation requires \"{_model}\" property");

				ModelAnimation animation = (ModelAnimation?)values[_animation]
					?? throw new InvalidDataException($"LevelModelAnimation requires \"{_animation}\" property");

				return new()
				{
					Frame = (float)values[_frame]!,
					Step = (float)values[_step]!,
					MaxFrame = (float)values[_maxFrame]!,
					Model = model,
					Animation = animation,
					TextureListAddress = (uint)values[_textureListAddress]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, LevelModelAnimation value, JsonSerializerOptions options)
			{
				writer.WriteNumber(_frame, value.Frame);
				writer.WriteNumber(_step, value.Step);
				writer.WriteNumber(_maxFrame, value.MaxFrame);

				if(value.TextureListAddress != 0)
				{
					writer.WriteString(_textureListAddress, UInt32HexConverter.ConvertTo(value.TextureListAddress));
				}

				writer.WritePropertyName(_model);
				JsonSerializer.Serialize(writer, value.Model, options);

				writer.WritePropertyName(_animation);
				JsonSerializer.Serialize(writer, value.Animation, options);
			}
		}


		/// <summary>
		/// First keyframe / Keyframe to start the animation at.
		/// </summary>
		public float Frame { get; set; }

		/// <summary>
		/// Keyframes traversed per frame-update / Animation Speed.
		/// </summary>
		public float Step { get; set; }

		/// <summary>
		/// Last keyframe / Length of the animation.
		/// </summary>
		public float MaxFrame { get; set; }

		/// <summary>
		/// Model that is being animated.
		/// </summary>
		public Node Model { get; set; }

		/// <summary>
		/// The corresponding node motion pair.
		/// </summary>
		public ModelAnimation Animation { get; set; }

		/// <summary>
		/// Texture list address to use.
		/// </summary>
		public uint TextureListAddress { get; set; }

		/// <summary>
		/// Creates a blank level model animation
		/// </summary>
		public LevelModelAnimation()
		{
			Step = 1;
			Model = new();
			Animation = new(Model, new());
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, IOContext context)
		{
			Frame = reader.ReadSingle();
			Step = reader.ReadSingle();
			MaxFrame = reader.ReadSingle();

			if(context.LevelFormat >= Format.Chunk)
			{
				Animation = reader.ReadObject<ModelAnimation, IOContext>(context);
				Animation.Label = Animation.LabelPrefix + Animation.Animation.Label;
				Model = Animation.Model;
			}
			else
			{
				Model = reader.ReadObjectOffset<Node, IOContext>(context, context.OffsetLUT)
					?? throw reader.ReadNullReference(nameof(LevelModelAnimation), nameof(Model));

				Animation = reader.ReadObjectOffset<ModelAnimation, IOContext>(context, context.OffsetLUT)
					?? throw reader.ReadNullReference(nameof(LevelModelAnimation), nameof(Animation));
			}

			TextureListAddress = reader.ReadUInt32();
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, IOContext context)
		{
			writer.WriteSingle(Frame);
			writer.WriteSingle(Step);
			writer.WriteSingle(MaxFrame);

			if(context.LevelFormat >= Format.Chunk)
			{
				writer.WriteObject(new ModelAnimation(Model, Animation.Animation), context);
			}
			else
			{
				writer.WriteObjectOffset(Model, context, context.OffsetLUT);
				writer.WriteObjectOffset(Animation, context, context.OffsetLUT);
			}

			writer.WriteUInt32(TextureListAddress);
		}
	}
}
