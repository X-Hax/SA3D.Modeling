using Amicitia.IO.Binary;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.ObjectData;
using System;
using System.Linq;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.AnimationData
{
	/// <summary>
	/// Animation data for various targets.
	/// </summary>
	public class Animation : ILabel, IBinarySerializable<AnimationIOContext>
	{
		/// <summary>
		/// Label prefix for <see cref="KeyframeSets"/>
		/// </summary>
		public const string KeyframeSetLabelPrefix = "keyframes_";

		/// <inheritdoc/>
		public string LabelPrefix => "animation_";

		/// <inheritdoc/>
		public string Label { get; set; }

		/// <summary>
		/// Intepolation mode between keyframes.
		/// </summary>
		public InterpolationMode InterpolationMode { get; set; }

		/// <summary>
		/// Whether to use 16-bit for euler rotation BAMS values.
		/// </summary>
		public bool ShortRotations { get; set; }

		/// <summary>
		/// Animation keyframe sets. The index of a keyframe set corresponds to the index of the node it belongs to
		/// </summary>
		public LabeledArray<KeyframeSet> KeyframeSets { get; set; }

		/// <summary>
		/// Types of keyframe stored in this animation.
		/// </summary>
		public KeyframeAttributes KeyframeTypes
		{
			get
			{
				KeyframeAttributes type = ManualKeyframeTypes;
				foreach(KeyframeSet kf in KeyframeSets)
				{
					type |= kf.Type;
				}

				return type;
			}
		}

		/// <summary>
		/// Manually enforced keyframe types.
		/// </summary>
		public KeyframeAttributes ManualKeyframeTypes { get; set; }


		/// <summary>
		/// Whether the motion transforms nodes.
		/// </summary>
		public bool IsNodeMotion
			=> !IsShapeMotion && !IsCameraMotion && !IsSpotLightMotion && !IsLightMotion;

		/// <summary>
		/// Whether the motion alters vertex positions and/or normals of meshes.
		/// </summary>
		public bool IsShapeMotion
			=> HasAnyAttributes(KeyframeAttributes.Vertex | KeyframeAttributes.Normal);

		/// <summary>
		/// Whether the motion transforms a camera. 
		/// </summary>
		public bool IsCameraMotion
			=> HasAnyAttributes(KeyframeAttributes.Angle | KeyframeAttributes.Roll | KeyframeAttributes.Target);

		/// <summary>
		/// Whether the motion targets a spotlight
		/// </summary>
		public bool IsSpotLightMotion
			=> HasAnyAttributes(KeyframeAttributes.Spot);

		/// <summary>
		/// Whether the motion targets lights
		/// </summary>
		public bool IsLightMotion
			=> HasAnyAttributes(KeyframeAttributes.Intensity | KeyframeAttributes.LightColor | KeyframeAttributes.Vector);


		/// <summary>
		/// Creates a new empty motion.
		/// </summary>
		public Animation()
		{
			string identifier = GenerateIdentifier();
			Label = LabelPrefix + identifier;
			KeyframeSets = new(KeyframeSetLabelPrefix + identifier, 0);
		}


		private bool HasAnyAttributes(KeyframeAttributes attributes)
		{
			return (KeyframeTypes & attributes) != 0;
		}

		/// <summary>
		/// Returns the number of frames in this motion.
		/// </summary>
		/// <returns></returns>
		public uint GetFrameCount()
		{
			if(KeyframeSets.Length == 0)
			{
				return 0;
			}

			return KeyframeSets.Max(x => x.KeyframeCount);
		}

		/// <summary>
		/// Optimizes all keyframes across the motion.
		/// </summary>
		/// <param name="generalThreshold"></param>
		/// <param name="quaternionThreshold">Difference threshold to use between quaternion keyframes.</param>
		/// <param name="colorThreshold">Difference threshold to use between colors.</param>
		/// <param name="asDegrees">Compare angle keyframes as degrees and not as radians.</param>
		/// <param name="start">Frame from which to start optimizing. <see langword="null"/> uses default.</param>
		/// <param name="end">Frame at which to end optimizing. <see langword="null"/> uses default.</param>
		public void Optimize(
			float generalThreshold,
			float quaternionThreshold,
			float colorThreshold,
			bool asDegrees,
			uint? start = null,
			uint? end = null)
		{
			foreach(KeyframeSet keyframes in KeyframeSets)
			{
				keyframes.Optimize(generalThreshold, quaternionThreshold, colorThreshold, asDegrees, start, end);
			}
		}

		/// <summary>
		/// Ensures that the transform properties of all nodes in a model tree have start- and end-frames.
		/// </summary>
		/// <param name="model">Any node from a tree for which keyframes should be ensured..</param>
		/// <param name="targetTypes">Keyframe types to target.</param>
		/// <param name="createKeyframes">If enabled, new keyframe sets will be created for any node that does not have any yet. Otherwise, only preexisting keyframe sets will be ensured to have start and end.</param>
		public void EnsureNodeKeyframes(Node model, KeyframeAttributes targetTypes, bool createKeyframes)
		{
			if(default == (targetTypes & (
				KeyframeAttributes.Position
				| KeyframeAttributes.EulerRotation
				| KeyframeAttributes.QuaternionRotation
				| KeyframeAttributes.Scale)))
			{
				return;
			}

			uint maxFrame = GetFrameCount() - 1;
			Node[] animNodes = model.GetAnimTreeNodes();

			if(KeyframeSets.Length < animNodes.Length)
			{
				KeyframeSet[] sets = KeyframeSets.Array;
				Array.Resize(ref sets, animNodes.Length);

				for(int i = KeyframeSets.Length; i < animNodes.Length; i++)
				{
					sets[i] = new();
				}

				KeyframeSets.Array = sets;
			}

			for(int i = 0; i < animNodes.Length; i++)
			{
				KeyframeSets[i]!.EnsureNodeKeyframes(animNodes[i], targetTypes, maxFrame);
			}
		}

		/// <summary>
		/// Ensures that specified keyframe types of all keyframes have start- and end-frames.
		/// </summary>
		/// <param name="targetTypes">Keyframe types to target.</param>
		public void EnsureKeyframes(KeyframeAttributes targetTypes)
		{
			uint maxFrame = GetFrameCount() - 1;
			foreach(KeyframeSet kf in KeyframeSets)
			{
				kf.EnsureKeyframes(targetTypes, maxFrame);
			}
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, AnimationIOContext context)
		{
			ShortRotations = context.FileContext.ShortRotations;

			long keyframeOffset = reader.ReadOffsetValue();

			int framecount = reader.ReadInt32();
			ManualKeyframeTypes = (KeyframeAttributes)reader.ReadUInt16();
			ushort attributes = reader.ReadUInt16();
			InterpolationMode = (InterpolationMode)((attributes >> 6) & 0x3);

			context.KeyframeType = ManualKeyframeTypes;

			KeyframeSets = reader.ReadLabeledObjectArrayAtOffset<KeyframeSet, AnimationIOContext>(keyframeOffset, (int)context.FileContext.KeyframeSetCount, KeyframeSetLabelPrefix, context, context.BaseContext.PointerLUT)
				?? throw reader.ReadNullReference(nameof(Animation), nameof(KeyframeSets));
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, AnimationIOContext context)
		{
			context.KeyframeType = KeyframeTypes;
			if(context.KeyframeType == default)
			{
				// just to have some valid pointer here, as i think that is necessary(?)
				writer.WriteOffsetValue(writer.GetPositionOffset() + (sizeof(uint) * 2));
			}
			else
			{
				writer.WriteObjectArrayOffset(KeyframeSets, context, context.BaseContext.PointerLUT);
			}

			int channels = context.KeyframeType.ChannelCount();
			writer.WriteUInt32(GetFrameCount());
			writer.WriteUInt16((ushort)context.KeyframeType);
			writer.WriteUInt16((ushort)((channels & 0xF) | ((int)InterpolationMode << 6)));
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Label} - {KeyframeSets.Length}";
		}
	}
}
