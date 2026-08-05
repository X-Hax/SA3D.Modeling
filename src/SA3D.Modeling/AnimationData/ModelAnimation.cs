using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
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
	/// Pairs a node and motion together.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class ModelAnimation : ILabel, IBinarySerializable<IOContext>
	{
		private class JsonConverter : SimpleJsonObjectConverter<ModelAnimation>
		{
			private const string _label = nameof(Label);
			private const string _model = nameof(Model);
			private const string _animation = nameof(Animation);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _label, new(PropertyTokenType.String, string.Empty) },
				{ _model, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _animation, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _label:
						return reader.GetString();
					case _model:
						return JsonSerializer.Deserialize<Node>(ref reader, options);
					case _animation:
						return JsonSerializer.Deserialize<Animation>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override ModelAnimation Create(ReadOnlyDictionary<string, object?> values)
			{
				Node model = (Node?)values[_model]
					?? throw new InvalidDataException($"ModelAnimation requires \"{_model}\" property");

				Animation animation = (Animation?)values[_animation]
					?? throw new InvalidDataException($"ModelAnimation requires \"{_animation}\" property");

				return new(model, animation)
				{
					Label = (string)values[_label]!
				};
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, ModelAnimation value, JsonSerializerOptions options)
			{
				writer.WriteString(_label, value.Label);

				writer.WritePropertyName(_model);
				JsonSerializer.Serialize(writer, value.Model, options);

				writer.WritePropertyName(_animation);
				JsonSerializer.Serialize(writer, value.Animation, options);
			}
		}

		/// <inheritdoc/>
		public string LabelPrefix => "action_";

		/// <inheritdoc/>
		public string Label { get; set; }

		/// <summary>
		/// Assigned node.
		/// </summary>
		public Node Model { get; set; }

		/// <summary>
		/// Assigned motion.
		/// </summary>
		public Animation Animation { get; set; }


		/// <summary>
		/// Creates a new, blank node animation
		/// </summary>
		public ModelAnimation() : this(new(), new()) { }

		/// <summary>
		/// Creates a new node motion.
		/// </summary>
		/// <param name="model">The model of the pair.</param>
		/// <param name="animation">The animation of the pair.</param>
		public ModelAnimation(Node model, Animation animation)
		{
			Label = LabelPrefix.GenerateIdentifier();
			Model = model;
			Animation = animation;
		}


		void IBinarySerializable<IOContext>.Read(BinaryObjectReader reader, IOContext context)
		{
			Model = reader.ReadObjectOffset<Node, IOContext>(context, context.OffsetLUT)
				?? throw reader.ReadNullReference(nameof(ModelAnimation), nameof(Model));

			AnimationIOContext animationContext = new()
			{
				OffsetLUT = context.OffsetLUT,
				KeyframeSetCount = (uint)Model.GetAnimTreeNodeCount()
			};

			Animation = reader.ReadObjectOffset<Animation, AnimationIOContext>(animationContext, context.OffsetLUT)
				?? throw reader.ReadNullReference(nameof(ModelAnimation), nameof(Animation));
		}

		void IBinarySerializable<IOContext>.Write(BinaryObjectWriter writer, IOContext context)
		{
			writer.WriteObjectOffset(Model, context, context.OffsetLUT);

			AnimationIOContext animationContext = new()
			{
				OffsetLUT = context.OffsetLUT,
				KeyframeSetCount = (uint)Model.GetAnimTreeNodeCount()
			};

			writer.WriteObjectOffset(Animation, animationContext, context.OffsetLUT);
		}
	}
}
