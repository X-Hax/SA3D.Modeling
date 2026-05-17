using Amicitia.IO.Binary;
using Amicitia.IO.Streams;
using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.AnimationData.Utilities;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.AnimationData
{
	/// <summary>
	/// Keyframe storage for an animation.
	/// </summary>
	public class KeyframeSet : IBinarySerializable<AnimationIOContext>
	{
		/// <summary>
		/// Base label for all the keyframes
		/// </summary>
		public string BaseLabel { get; set; }


		/// <summary>
		/// Transform position keyframes.
		/// </summary>
		public SortedDictionary<uint, Vector3> Position { get; private set; }

		/// <summary>
		/// Transform rotation (euler angles) keyframes.
		/// </summary>
		public SortedDictionary<uint, Vector3> EulerRotation { get; private set; }

		/// <summary>
		/// Transform scale keyframes.
		/// </summary>
		public SortedDictionary<uint, Vector3> Scale { get; private set; }

		/// <summary>
		/// General vector3 keyframes.
		/// </summary>
		public SortedDictionary<uint, Vector3> Vector { get; private set; }

		/// <summary>
		/// Mesh vertex positions.
		/// </summary>
		public SortedDictionary<uint, LabeledArray<Vector3>> Vertex { get; private set; }

		/// <summary>
		/// Mesh vertex normals.
		/// </summary>
		public SortedDictionary<uint, LabeledArray<Vector3>> Normal { get; private set; }

		/// <summary>
		/// Camera lookat target.
		/// </summary>
		public SortedDictionary<uint, Vector3> Target { get; private set; }

		/// <summary>
		/// Camera Roll (euler angle).
		/// </summary>
		public SortedDictionary<uint, float> Roll { get; private set; }

		/// <summary>
		/// Camera field of view (radians).
		/// </summary>
		public SortedDictionary<uint, float> Angle { get; private set; }

		/// <summary>
		/// Light Color.
		/// </summary>
		public SortedDictionary<uint, Color> LightColor { get; private set; }

		/// <summary>
		/// Light intensity.
		/// </summary>
		public SortedDictionary<uint, float> Intensity { get; private set; }

		/// <summary>
		/// Spotlights.
		/// </summary>
		public SortedDictionary<uint, Spotlight> Spot { get; private set; }

		/// <summary>
		/// Point light positions.
		/// </summary>
		public SortedDictionary<uint, Vector2> Point { get; private set; }

		/// <summary>
		/// Rotation (quaternion) keyframes.
		/// </summary>
		public SortedDictionary<uint, Quaternion> QuaternionRotation { get; private set; }

		/// <summary>
		/// Whether any keyframes exist in this keyframe set
		/// </summary>
		public bool HasKeyframes
			=> GetKeyEnumerable().Any(x => x.Any());

		/// <summary>
		/// Returns the number of keyframes in the biggest keyframe dictionary.
		/// </summary>
		public uint KeyframeCount
		{
			get
			{
				bool hasKeys = false;
				uint maxKey = 0;
				foreach(IEnumerable<uint> keys in GetKeyEnumerable())
				{
					if(!keys.Any())
					{
						continue;
					}

					hasKeys = true;
					maxKey = uint.Max(maxKey, keys.Last());
				}

				if(hasKeys)
				{
					return maxKey + 1;
				}
				else
				{
					return 0;
				}
			}
		}

		/// <summary>
		/// Channels that contain keyframes.
		/// </summary>
		public KeyframeAttributes Type
		{
			get
			{
				KeyframeAttributes attribs = 0;

				foreach((KeyframeAttributes type, IEnumerable<uint> keys) in GetTypeKeyEnumerable())
				{
					if(keys.Any())
					{
						attribs |= type;
					}
				}

				return attribs;
			}
		}


		/// <summary>
		/// Creates an empty keyframe storage
		/// </summary>
		public KeyframeSet()
		{
			BaseLabel = "keyframes_".GenerateIdentifier();
			Position = [];
			EulerRotation = [];
			Scale = [];
			Vector = [];
			Vertex = [];
			Normal = [];
			Target = [];
			Roll = [];
			Angle = [];
			LightColor = [];
			Intensity = [];
			Spot = [];
			Point = [];
			QuaternionRotation = [];
		}


		private IEnumerable<IEnumerable<uint>> GetKeyEnumerable()
		{
			yield return Position.Keys;
			yield return EulerRotation.Keys;
			yield return Scale.Keys;
			yield return Vector.Keys;
			yield return Vertex.Keys;
			yield return Normal.Keys;
			yield return Target.Keys;
			yield return Roll.Keys;
			yield return Angle.Keys;
			yield return LightColor.Keys;
			yield return Intensity.Keys;
			yield return Spot.Keys;
			yield return Point.Keys;
			yield return QuaternionRotation.Keys;
		}

		private IEnumerable<(KeyframeAttributes type, IEnumerable<uint> keys)> GetTypeKeyEnumerable()
		{
			uint current = 1;
			foreach(IEnumerable<uint> keys in GetKeyEnumerable())
			{
				yield return ((KeyframeAttributes)current, keys);
				current <<= 1;
			}
		}

		/// <summary>
		/// Returns a all values at a specific frame
		/// </summary>
		/// <param name="frame">Frame to get the values of</param>
		/// <returns></returns>
		public Frame GetFrameAt(float frame)
		{
			return new()
			{
				FrameTime = frame,
				Position = Position.ValueAtFrame(frame),
				EulerRotation = EulerRotation.ValueAtFrame(frame),
				Scale = Scale.ValueAtFrame(frame),
				Vector = Vector.ValueAtFrame(frame),
				Vertex = Vertex.ValueAtFrame(frame),
				Normal = Normal.ValueAtFrame(frame),
				Target = Target.ValueAtFrame(frame),
				Roll = Roll.ValueAtFrame(frame),
				Angle = Angle.ValueAtFrame(frame),
				Color = LightColor.ValueAtFrame(frame),
				Intensity = Intensity.ValueAtFrame(frame),
				Spotlight = Spot.ValueAtFrame(frame),
				Point = Point.ValueAtFrame(frame),
				QuaternionRotation = QuaternionRotation.ValueAtFrame(frame),
			};
		}

		/// <summary>
		/// Optimizes the keyframes.
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
			Position.OptimizeVector3(generalThreshold, start, end);

			if(asDegrees)
			{
				EulerRotation.OptimizeVector3Degrees(generalThreshold, start, end);
				Roll.OptimizeFloat(generalThreshold, start, end);
				Angle.OptimizeFloat(generalThreshold, start, end);
			}
			else
			{
				EulerRotation.OptimizeVector3(generalThreshold, start, end);
				Roll.OptimizeFloat(generalThreshold, start, end);
				Angle.OptimizeFloat(generalThreshold, start, end);
			}

			Scale.OptimizeVector3(generalThreshold, start, end);
			Vector.OptimizeVector3(generalThreshold, start, end);
			Target.OptimizeVector3(generalThreshold, start, end);
			LightColor.OptimizeColor(colorThreshold, start, end);
			Intensity.OptimizeFloat(generalThreshold, start, end);
			Spot.OptimizeSpotlight(generalThreshold, start, end);
			Point.OptimizeVector2(generalThreshold, start, end);
			QuaternionRotation.OptimizeQuaternion(quaternionThreshold, start, end);
		}

		/// <summary>
		/// Ensures that specified node transform properties have start- and end-frames.
		/// </summary>
		/// <param name="node">The node for which to ensure frames. If no keyframes exist, then they will be added with the values from this node.</param>
		/// <param name="targets">Keyframe types to target.</param>
		/// <param name="endFrame">The frame until which keyframes need to exist.</param>
		public void EnsureNodeKeyframes(Node node, KeyframeAttributes targets, uint endFrame)
		{
			void Ensure<T>(SortedDictionary<uint, T> keyframes, KeyframeAttributes type, T value)
			{
				if(!targets.HasFlag(type))
				{
					return;
				}

				if(!keyframes.ContainsKey(0))
				{
					keyframes.Add(0, value);
				}

				if(keyframes.Keys.Max() < endFrame)
				{
					keyframes.Add(endFrame, value);
				}
			}

			Ensure(Position, KeyframeAttributes.Position, node.Position);
			Ensure(EulerRotation, KeyframeAttributes.EulerRotation, node.EulerRotation);
			Ensure(QuaternionRotation, KeyframeAttributes.QuaternionRotation, node.QuaternionRotation);
			Ensure(Scale, KeyframeAttributes.Scale, node.Scale);
		}

		/// <summary>
		/// Ensures that specified keyframe types have start- and end-frames
		/// </summary>
		/// <param name="targets">Keyframe types to target.</param>
		/// <param name="endFrame">The frame until which keyframes need to exist.</param>
		public void EnsureKeyframes(KeyframeAttributes targets, uint endFrame)
		{
			void Ensure<T>(SortedDictionary<uint, T> keyframes, KeyframeAttributes type, T value)
			{
				if(!targets.HasFlag(type))
				{
					return;
				}

				if(!keyframes.ContainsKey(0))
				{
					keyframes.Add(0, value);
				}

				if(keyframes.Keys.Max() < endFrame)
				{
					keyframes.Add(endFrame, value);
				}
			}

			Ensure(Position, KeyframeAttributes.Position, Vector3.Zero);
			Ensure(EulerRotation, KeyframeAttributes.EulerRotation, Vector3.Zero);
			Ensure(Scale, KeyframeAttributes.Scale, Vector3.One);
			Ensure(Vector, KeyframeAttributes.Vector, default);
			Ensure(Vertex, KeyframeAttributes.Vertex, new LabeledArray<Vector3>(0));
			Ensure(Normal, KeyframeAttributes.Normal, new LabeledArray<Vector3>(0));
			Ensure(Target, KeyframeAttributes.Target, Vector3.Zero);
			Ensure(Roll, KeyframeAttributes.Roll, 0);
			Ensure(Angle, KeyframeAttributes.Angle, 0);
			Ensure(LightColor, KeyframeAttributes.LightColor, default);
			Ensure(Intensity, KeyframeAttributes.Intensity, default);
			Ensure(Spot, KeyframeAttributes.Spot, default);
			Ensure(Point, KeyframeAttributes.Point, default);
			Ensure(QuaternionRotation, KeyframeAttributes.QuaternionRotation, Quaternion.Identity);

		}


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, AnimationIOContext context)
		{
			int channelCount = context.KeyframeType.ChannelCount();

			long[] keyframeOffsets = new long[channelCount];
			for(int i = 0; i < channelCount; i++)
			{
				keyframeOffsets[i] = reader.ReadOffsetValue();
			}

			int[] keyframeCounts = reader.ReadArray<int>(channelCount);

			int index = 0;
			foreach(KeyframeAttributes flag in Enum.GetValues<KeyframeAttributes>())
			{
				if(!context.KeyframeType.HasFlag(flag))
				{
					continue;
				}

				long offset = keyframeOffsets[index];
				if(offset != 0)
				{
					using(SeekToken t = reader.AtOffset(offset))
					{
						int keyframeCount = keyframeCounts[index];

						switch(flag)
						{
							case KeyframeAttributes.Position:
								reader.ReadVector3Set(keyframeCount, Position, FloatIOType.Float);
								break;
							case KeyframeAttributes.EulerRotation:
								reader.ReadVector3Set(keyframeCount, EulerRotation, context.FileContext.ShortRotations ? FloatIOType.BAMSF16 : FloatIOType.BAMSF32);
								break;
							case KeyframeAttributes.Scale:
								reader.ReadVector3Set(keyframeCount, Scale, FloatIOType.Float);
								break;
							case KeyframeAttributes.Vector:
								reader.ReadVector3Set(keyframeCount, Vector, FloatIOType.Float);
								break;
							case KeyframeAttributes.Vertex:
								reader.ReadVector3ArraySet(keyframeCount, "vertex_", Vertex, context.BaseContext.PointerLUT);
								break;
							case KeyframeAttributes.Normal:
								reader.ReadVector3ArraySet(keyframeCount, "normal_", Normal, context.BaseContext.PointerLUT);
								break;
							case KeyframeAttributes.Target:
								reader.ReadVector3Set(keyframeCount, Target, FloatIOType.Float);
								break;
							case KeyframeAttributes.Roll:
								reader.ReadFloatSet(keyframeCount, Roll, FloatIOType.BAMSF32);
								break;
							case KeyframeAttributes.Angle:
								reader.ReadFloatSet(keyframeCount, Angle, FloatIOType.BAMSF32);
								break;
							case KeyframeAttributes.LightColor:
								reader.ReadColorSet(keyframeCount, LightColor, ColorIOType.ARGB8_32);
								break;
							case KeyframeAttributes.Intensity:
								reader.ReadFloatSet(keyframeCount, Intensity, FloatIOType.Float);
								break;
							case KeyframeAttributes.Spot:
								reader.ReadSpotSet(keyframeCount, Spot);
								break;
							case KeyframeAttributes.Point:
								reader.ReadVector2Set(keyframeCount, Point, FloatIOType.Float);
								break;
							case KeyframeAttributes.QuaternionRotation:
								reader.ReadQuaternionSet(keyframeCount, QuaternionRotation);
								break;
							default:
								break;
						}

					}
				}

				index++;
			}
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, AnimationIOContext context)
		{
			List<int> frameCounts = [];

			foreach((KeyframeAttributes type, IEnumerable<uint> keys) in GetTypeKeyEnumerable())
			{
				if(!context.KeyframeType.HasFlag(type))
				{
					continue;
				}

				int count = keys.Count();
				frameCounts.Add(count);
				if(count == 0)
				{
					writer.WriteOffsetValue(0);
					continue;
				}

				switch(type)
				{
					case KeyframeAttributes.Position:
						writer.WriteOffset(() => writer.WriteVector3Set(Position, FloatIOType.Float));
						break;
					case KeyframeAttributes.EulerRotation:
						writer.WriteOffset(() => writer.WriteVector3Set(EulerRotation, context.FileContext.ShortRotations ? FloatIOType.BAMSF16 : FloatIOType.BAMSF32));
						break;
					case KeyframeAttributes.Scale:
						writer.WriteOffset(() => writer.WriteVector3Set(Scale, FloatIOType.Float));
						break;
					case KeyframeAttributes.Vector:
						writer.WriteOffset(() => writer.WriteVector3Set(Vector, FloatIOType.Float));
						break;
					case KeyframeAttributes.Vertex:
						writer.WriteOffset(() => writer.WriteVector3ArrayData(Vertex, context.BaseContext.PointerLUT));
						break;
					case KeyframeAttributes.Normal:
						writer.WriteOffset(() => writer.WriteVector3ArrayData(Normal, context.BaseContext.PointerLUT));
						break;
					case KeyframeAttributes.Target:
						writer.WriteOffset(() => writer.WriteVector3Set(Target, FloatIOType.Float));
						break;
					case KeyframeAttributes.Roll:
						writer.WriteOffset(() => writer.WriteFloatSet(Roll, FloatIOType.BAMSF32));
						break;
					case KeyframeAttributes.Angle:
						writer.WriteOffset(() => writer.WriteFloatSet(Angle, FloatIOType.BAMSF32));
						break;
					case KeyframeAttributes.LightColor:
						writer.WriteOffset(() => writer.WriteColorSet(LightColor, ColorIOType.ARGB8_32));
						break;
					case KeyframeAttributes.Intensity:
						writer.WriteOffset(() => writer.WriteFloatSet(Intensity, FloatIOType.Float));
						break;
					case KeyframeAttributes.Spot:
						writer.WriteOffset(() => writer.WriteSpotlightSet(Spot));
						break;
					case KeyframeAttributes.Point:
						writer.WriteOffset(() => writer.WriteVector2Set(Point, FloatIOType.Float));
						break;
					case KeyframeAttributes.QuaternionRotation:
						writer.WriteOffset(() => writer.WriteQuaternionSet(QuaternionRotation));
						break;
					default:
						break;
				}
			}

			writer.WriteCollection(frameCounts);
		}
	}
}