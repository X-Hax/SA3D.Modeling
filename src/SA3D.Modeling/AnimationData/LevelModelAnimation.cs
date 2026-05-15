using Amicitia.IO.Binary;
using SA3D.Common.IO;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.AnimationData
{
	/// <summary>
	/// Level geometry animation (only used in sa1)
	/// </summary>
	public class LevelModelAnimation : IBinarySerializable<IOContext>
	{
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
		public uint TextureListPointer { get; set; }

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

			Model = reader.ReadObjectOffset<Node, IOContext>(context, context.PointerLUT)
				?? throw reader.ReadNullReference(nameof(LevelModelAnimation), nameof(Model));

			Animation = reader.ReadObjectOffset<ModelAnimation, IOContext>(context, context.PointerLUT)
				?? throw reader.ReadNullReference(nameof(LevelModelAnimation), nameof(Animation));

			TextureListPointer = reader.ReadUInt32();
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, IOContext context)
		{
			writer.WriteSingle(Frame);
			writer.WriteSingle(Step);
			writer.WriteSingle(MaxFrame);
			writer.WriteObjectOffset(Model, context, context.PointerLUT);
			writer.WriteObjectOffset(Animation, context, context.PointerLUT);
			writer.WriteUInt32(TextureListPointer);
		}
	}
}
