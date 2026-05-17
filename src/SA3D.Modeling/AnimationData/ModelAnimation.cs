using Amicitia.IO.Binary;
using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.AnimationData
{
	/// <summary>
	/// Pairs a node and motion together.
	/// </summary>
	public class ModelAnimation : ILabel, IBinarySerializable<IOContext>
	{
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

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, IOContext context)
		{
			Model = reader.ReadObjectOffset<Node, IOContext>(context, context.PointerLUT)
				?? throw reader.ReadNullReference(nameof(ModelAnimation), nameof(Model));

			AnimationIOContext animationContext = new()
			{
				BaseContext = context,
				FileContext = new()
				{
					KeyframeSetCount = (uint)Model.GetAnimTreeNodeCount()
				}
			};

			Animation = reader.ReadObjectOffset<Animation, AnimationIOContext>(animationContext, context.PointerLUT)
				?? throw reader.ReadNullReference(nameof(ModelAnimation), nameof(Animation));
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, IOContext context)
		{
			writer.WriteObjectOffset(Model, context, context.PointerLUT);

			AnimationIOContext animationContext = new()
			{
				BaseContext = context,
				FileContext = new()
				{
					KeyframeSetCount = (uint)Model.GetAnimTreeNodeCount()
				}
			};

			writer.WriteObjectOffset(Animation, animationContext, context.PointerLUT);
		}
	}
}
