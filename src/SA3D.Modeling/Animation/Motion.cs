using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Linq;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.Animation
{
	/// <summary>
	/// Animation data for various targets.
	/// </summary>
	public class Motion : ILabel
	{
		/// <summary>
		/// Size of the motion struct in bytes.
		/// </summary>
		public const uint StructSize = 16;

		/// <inheritdoc/>
		public string Label { get; set; }

		/// <summary>
		/// Node in the models node tree that this animation targets.
		/// </summary>
		public uint ModelCount { get; set; }

		/// <summary>
		/// Intepolation mode between keyframes.
		/// </summary>
		public InterpolationMode InterpolationMode { get; set; }

		/// <summary>
		/// Whether to use 16-bit for euler rotation BAMS values.
		/// </summary>
		public bool ShortRot { get; set; }

		/// <summary>
		/// Keyframes based on their model id
		/// </summary>
		public Dictionary<int, Keyframes> Keyframes { get; }

		/// <summary>
		/// Types of keyframe stored in this animation.
		/// </summary>
		public KeyframeAttributes KeyframeTypes
		{
			get
			{
				KeyframeAttributes type = 0;
				foreach(Keyframes kf in Keyframes.Values)
				{
					type |= kf.Type;
				}

				return type | ManualKeyframeTypes;
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
		public Motion()
		{
			Label = "animation_" + GenerateIdentifier();
			Keyframes = new();
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
			uint result = 0;
			foreach(Keyframes k in Keyframes.Values)
			{
				result = uint.Max(result, k.KeyframeCount);
			}

			return result;
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
			foreach(Keyframes keyframes in Keyframes.Values)
			{
				keyframes.Optimize(generalThreshold, quaternionThreshold, colorThreshold, asDegrees, start, end);
			}
		}

		/// <summary>
		/// Ensures that the transform propertie of all nodes in a model tree have start- and end-frames.
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

			int i = 0;
			foreach(Node node in model.GetAnimTreeNodes())
			{
				if(!Keyframes.TryGetValue(i, out Keyframes? keyframes))
				{
					if(!createKeyframes)
					{
						continue;
					}

					keyframes = new();
					Keyframes.Add(i, keyframes);
				}

				keyframes.EnsureNodeKeyframes(node, targetTypes, maxFrame);

				i++;
			}
		}


		/// <summary>
		/// Writes the motion to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		public uint Write(EndianStackWriter writer, PointerLUT lut)
		{
			uint onWrite()
			{
				KeyframeAttributes type = KeyframeTypes;
				int channels = type.ChannelCount();

				uint keyframeCount = uint.Max(ModelCount, (uint)Keyframes.Keys.Max() + 1u);

				(uint address, uint count)[][] keyFrameLocations = new (uint addr, uint count)[keyframeCount][];

				for(int i = 0; i < keyframeCount; i++)
				{
					keyFrameLocations[i] = !Keyframes.ContainsKey(i)
						? new (uint, uint)[channels]
						: Keyframes[i].Write(writer, type, lut, ShortRot);
				}

				uint keyframesAddr = writer.PointerPosition;

				foreach((uint addr, uint count)[] kf in keyFrameLocations)
				{
					for(int i = 0; i < kf.Length; i++)
					{
						writer.WriteUInt(kf[i].addr);
					}

					for(int i = 0; i < kf.Length; i++)
					{
						writer.WriteUInt(kf[i].count);
					}
				}

				uint result = writer.PointerPosition;

				writer.WriteUInt(keyframesAddr);
				writer.WriteUInt(GetFrameCount());
				writer.WriteUShort((ushort)type);
				writer.WriteUShort((ushort)((channels & 0xF) | ((int)InterpolationMode << 6)));

				return result;

			}

			return lut.GetAddAddress(this, onWrite);
		}

		/// <summary>
		/// Reads a motion off an endian stack reader.
		/// </summary>
		/// <param name="reader">Byte source</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="modelCount">Number of nodes in the tree of the targeted model.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <param name="shortRot">Whether euler rotations are stored in 16-bit instead of 32-bit.</param>
		/// <returns>The motion that was read</returns>
		public static Motion Read(EndianStackReader reader, uint address, uint modelCount, PointerLUT lut, bool shortRot = false)
		{
			Motion onRead()
			{
				uint keyframeAddr = reader.ReadPointer(address);
				// offset 4 is frame count. We dont need to read that.
				KeyframeAttributes keyframeType = (KeyframeAttributes)reader.ReadUShort(address + 8);

				ushort tmp = reader.ReadUShort(address + 10);
				InterpolationMode mode = (InterpolationMode)((tmp >> 6) & 0x3);
				int channels = tmp & 0xF;

				Motion result = new()
				{
					InterpolationMode = mode,
					ModelCount = modelCount,
					ShortRot = shortRot,
					ManualKeyframeTypes = keyframeType
				};

				for(int i = 0; i < modelCount; i++)
				{
					Keyframes kf = Animation.Keyframes.Read(reader, ref keyframeAddr, keyframeType, lut, shortRot);
					result.Keyframes.Add(i, kf);
				}

				return result;

			}

			return lut.GetAddLabeledValue(address, "animation_", onRead);
		}


		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Label} : {ModelCount} - {Keyframes.Count}";
		}
	}
}
