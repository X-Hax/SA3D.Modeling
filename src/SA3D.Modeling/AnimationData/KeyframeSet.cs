using Amicitia.IO.Binary;
using J113D.Json;
using SA3D.Common.Ascii;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.AnimationData.Utilities;
using SA3D.Modeling.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.AnimationData
{
	/// <summary>
	/// Keyframe storage for an animation.
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class KeyframeSet : IBinarySerializable<AnimationIOContext>, IAsciiSerializable<AnimationAsciiIOContext>
	{
		private class JsonConverter : SimpleJsonObjectConverter<KeyframeSet>
		{
			private const string _position = nameof(Position);
			private const string _eulerRotation = nameof(EulerRotation);
			private const string _scale = nameof(Scale);
			private const string _vector = nameof(Vector);
			private const string _vertex = nameof(Vertex);
			private const string _normal = nameof(Normal);
			private const string _target = nameof(Target);
			private const string _roll = nameof(Roll);
			private const string _angle = nameof(Angle);
			private const string _lightColor = nameof(LightColor);
			private const string _intensity = nameof(Intensity);
			private const string _spotlight = nameof(Spotlight);
			private const string _point = nameof(Point);
			private const string _quaternionRotation = nameof(QuaternionRotation);


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _position, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _eulerRotation, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _scale, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _vector, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _vertex, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _normal, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _target, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _roll, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _angle, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _lightColor, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _intensity, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _spotlight, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _point, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _quaternionRotation, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _position:
					case _eulerRotation:
					case _scale:
					case _vector:
					case _target:
						return JsonSerializer.Deserialize<KeyframeArray<Vector3>>(ref reader, options);
					case _vertex:
					case _normal:
						return JsonSerializer.Deserialize<KeyframeArray<LabeledArray<Vector3>>>(ref reader, options);
					case _roll:
					case _angle:
					case _intensity:
						return JsonSerializer.Deserialize<KeyframeArray<float>>(ref reader, options);
					case _lightColor:
						return JsonSerializer.Deserialize<KeyframeArray<Color>>(ref reader, options);
					case _spotlight:
						return JsonSerializer.Deserialize<KeyframeArray<Spotlight>>(ref reader, options);
					case _point:
						return JsonSerializer.Deserialize<KeyframeArray<Vector2>>(ref reader, options);
					case _quaternionRotation:
						return JsonSerializer.Deserialize<KeyframeArray<Quaternion>>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override KeyframeSet Create(ReadOnlyDictionary<string, object?> values)
			{
				return new()
				{
					Position = (KeyframeArray<Vector3>?)values[_position],
					EulerRotation = (KeyframeArray<Vector3>?)values[_eulerRotation],
					Scale = (KeyframeArray<Vector3>?)values[_scale],
					Vector = (KeyframeArray<Vector3>?)values[_vector],
					Vertex = (KeyframeArray<LabeledArray<Vector3>>?)values[_vertex],
					Normal = (KeyframeArray<LabeledArray<Vector3>>?)values[_normal],
					Target = (KeyframeArray<Vector3>?)values[_target],
					Roll = (KeyframeArray<float>?)values[_roll],
					Angle = (KeyframeArray<float>?)values[_angle],
					LightColor = (KeyframeArray<Color>?)values[_lightColor],
					Intensity = (KeyframeArray<Vector2>?)values[_intensity],
					Spotlight = (KeyframeArray<Spotlight>?)values[_spotlight],
					Point = (KeyframeArray<Vector2>?)values[_point],
					QuaternionRotation = (KeyframeArray<Quaternion>?)values[_quaternionRotation]
				};
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, KeyframeSet value, JsonSerializerOptions options)
			{
				void writeKeyframeSet<T>(string name, KeyframeArray<T>? target)
				{
					if(target == null || target.Count == 0)
					{
						return;
					}

					writer.WritePropertyName(name);
					JsonSerializer.Serialize(writer, target, options);
				}

				writeKeyframeSet(_position, value.Position);
				writeKeyframeSet(_eulerRotation, value.EulerRotation);
				writeKeyframeSet(_scale, value.Scale);
				writeKeyframeSet(_vector, value.Vector);
				writeKeyframeSet(_vertex, value.Vertex);
				writeKeyframeSet(_normal, value.Normal);
				writeKeyframeSet(_target, value.Target);
				writeKeyframeSet(_roll, value.Roll);
				writeKeyframeSet(_angle, value.Angle);
				writeKeyframeSet(_lightColor, value.LightColor);
				writeKeyframeSet(_intensity, value.Intensity);
				writeKeyframeSet(_spotlight, value.Spotlight);
				writeKeyframeSet(_point, value.Point);
				writeKeyframeSet(_quaternionRotation, value.QuaternionRotation);
			}
		}


		/// <summary>
		/// Label prefix for <see cref="Position"/>
		/// </summary>
		public const string PositionLabelPrefix = "position_";

		/// <summary>
		/// Label prefix for <see cref="EulerRotation"/>
		/// </summary>
		public const string EulerRotationLabelPrefix = "rotation_";

		/// <summary>
		/// Label prefix for <see cref="Scale"/>
		/// </summary>
		public const string ScaleLabelPrefix = "scale_";

		/// <summary>
		/// Label prefix for <see cref="Vector"/>
		/// </summary>
		public const string VectorLabelPrefix = "vector_";

		/// <summary>
		/// Label prefix for <see cref="Vertex"/>
		/// </summary>
		public const string VertexLabelPrefix = "vertex_";

		/// <summary>
		/// Label prefix for <see cref="Normal"/>
		/// </summary>
		public const string NormalLabelPrefix = "normal_";

		/// <summary>
		/// Label prefix for <see cref="Target"/>
		/// </summary>
		public const string TargetLabelPrefix = "target_";

		/// <summary>
		/// Label prefix for <see cref="Roll"/>
		/// </summary>
		public const string RollLabelPrefix = "roll_";

		/// <summary>
		/// Label prefix for <see cref="Angle"/>
		/// </summary>
		public const string AngleLabelPrefix = "angle_";

		/// <summary>
		/// Label prefix for <see cref="LightColor"/>
		/// </summary>
		public const string LightColorLabelPrefix = "lightCol_";

		/// <summary>
		/// Label prefix for <see cref="Intensity"/>
		/// </summary>
		public const string IntensityLabelPrefix = "intensity_";

		/// <summary>
		/// Label prefix for <see cref="Spotlight"/>
		/// </summary>
		public const string SpotlightLabelPrefix = "spotlight_";

		/// <summary>
		/// Label prefix for <see cref="Point"/>
		/// </summary>
		public const string PointLabelPrefix = "point_";

		/// <summary>
		/// Label prefix for <see cref="QuaternionRotation"/>
		/// </summary>
		public const string QuaternionRotationLabelPrefix = "quaternion_";



		/// <summary>
		/// Transform position keyframes.
		/// </summary>
		public KeyframeArray<Vector3>? Position { get; set; }

		/// <summary>
		/// Transform rotation (euler angles) keyframes.
		/// </summary>
		public KeyframeArray<Vector3>? EulerRotation { get; set; }

		/// <summary>
		/// Transform scale keyframes.
		/// </summary>
		public KeyframeArray<Vector3>? Scale { get; set; }

		/// <summary>
		/// General vector3 keyframes.
		/// </summary>
		public KeyframeArray<Vector3>? Vector { get; set; }

		/// <summary>
		/// Mesh vertex positions.
		/// </summary>
		public KeyframeArray<LabeledArray<Vector3>>? Vertex { get; set; }

		/// <summary>
		/// Mesh vertex normals.
		/// </summary>
		public KeyframeArray<LabeledArray<Vector3>>? Normal { get; set; }

		/// <summary>
		/// Camera lookat target.
		/// </summary>
		public KeyframeArray<Vector3>? Target { get; set; }

		/// <summary>
		/// Camera Roll (euler angle).
		/// </summary>
		public KeyframeArray<float>? Roll { get; set; }

		/// <summary>
		/// Camera field of view (radians).
		/// </summary>
		public KeyframeArray<float>? Angle { get; set; }

		/// <summary>
		/// Light Color.
		/// </summary>
		public KeyframeArray<Color>? LightColor { get; set; }

		/// <summary>
		/// Light intensity + ambient
		/// </summary>
		public KeyframeArray<Vector2>? Intensity { get; set; }

		/// <summary>
		/// Spotlights.
		/// </summary>
		public KeyframeArray<Spotlight>? Spotlight { get; set; }

		/// <summary>
		/// Point light positions.
		/// </summary>
		public KeyframeArray<Vector2>? Point { get; set; }

		/// <summary>
		/// Rotation (quaternion) keyframes.
		/// </summary>
		public KeyframeArray<Quaternion>? QuaternionRotation { get; set; }

		/// <summary>
		/// Whether any keyframes exist in this keyframe set
		/// </summary>
		public bool HasKeyframes
			=> GetKeyEnumerable().Any(x => x?.Any() == true);

		/// <summary>
		/// Returns the number of keyframes in the biggest keyframe dictionary.
		/// </summary>
		public uint KeyframeCount
		{
			get
			{
				bool hasKeys = false;
				uint maxKey = 0;
				foreach(IEnumerable<uint>? keys in GetKeyEnumerable())
				{
					if(keys == null || !keys.Any())
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

				foreach((KeyframeAttributes type, IEnumerable<uint>? keys) in GetTypeKeyEnumerable())
				{
					if(keys != null && keys.Any())
					{
						attribs |= type;
					}
				}

				return attribs;
			}
		}


		private IEnumerable<IEnumerable<uint>?> GetKeyEnumerable()
		{
			yield return Position?.Keys;
			yield return EulerRotation?.Keys;
			yield return Scale?.Keys;
			yield return Vector?.Keys;
			yield return Vertex?.Keys;
			yield return Normal?.Keys;
			yield return Target?.Keys;
			yield return Roll?.Keys;
			yield return Angle?.Keys;
			yield return LightColor?.Keys;
			yield return Intensity?.Keys;
			yield return Spotlight?.Keys;
			yield return Point?.Keys;
			yield return QuaternionRotation?.Keys;

		}

		private IEnumerable<(KeyframeAttributes type, IEnumerable<uint>? keys)> GetTypeKeyEnumerable()
		{
			uint current = 1;
			foreach(IEnumerable<uint>? keys in GetKeyEnumerable())
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
				Spotlight = Spotlight.ValueAtFrame(frame),
				Point = Point.ValueAtFrame(frame),
				QuaternionRotation = QuaternionRotation.ValueAtFrame(frame),
			};
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

			ModelOffsetLUT lut = context.BaseContext.OffsetLUT;

			int index = 0;
			foreach(KeyframeAttributes flag in Enum.GetValues<KeyframeAttributes>())
			{
				if(!context.KeyframeType.HasFlag(flag))
				{
					continue;
				}

				long offset = keyframeOffsets[index];
				int keyframeCount = keyframeCounts[index];
				index++;

				switch(flag)
				{
					case KeyframeAttributes.Position:
						Position = reader.ReadKeyframeArrayAtOffset(offset, keyframeCount, PositionLabelPrefix, lut, r => r.ReadVector3Set(keyframeCount, FloatIOType.Float));
						break;
					case KeyframeAttributes.EulerRotation:
						EulerRotation = reader.ReadKeyframeArrayAtOffset(offset, keyframeCount, EulerRotationLabelPrefix, lut, r => r.ReadVector3Set(keyframeCount, context.FileContext.RotationAngleType));
						break;
					case KeyframeAttributes.Scale:
						Scale = reader.ReadKeyframeArrayAtOffset(offset, keyframeCount, ScaleLabelPrefix, lut, r => r.ReadVector3Set(keyframeCount, FloatIOType.Float));
						break;
					case KeyframeAttributes.Vector:
						Vector = reader.ReadKeyframeArrayAtOffset(offset, keyframeCount, VectorLabelPrefix, lut, r => r.ReadVector3Set(keyframeCount, FloatIOType.Float));
						break;
					case KeyframeAttributes.Vertex:
						Vertex = reader.ReadKeyframeArrayAtOffset(offset, keyframeCount, VertexLabelPrefix, lut, r => r.ReadVector3ArraySet(keyframeCount, "vertex_", lut));
						break;
					case KeyframeAttributes.Normal:
						Normal = reader.ReadKeyframeArrayAtOffset(offset, keyframeCount, NormalLabelPrefix, lut, r => r.ReadVector3ArraySet(keyframeCount, "normal_", lut));
						break;
					case KeyframeAttributes.Target:
						Target = reader.ReadKeyframeArrayAtOffset(offset, keyframeCount, TargetLabelPrefix, lut, r => r.ReadVector3Set(keyframeCount, FloatIOType.Float));
						break;
					case KeyframeAttributes.Roll:
						Roll = reader.ReadKeyframeArrayAtOffset(offset, keyframeCount, RollLabelPrefix, lut, r => r.ReadFloatSet(keyframeCount, context.FileContext.AngleType));
						break;
					case KeyframeAttributes.Angle:
						Angle = reader.ReadKeyframeArrayAtOffset(offset, keyframeCount, AngleLabelPrefix, lut, r => r.ReadFloatSet(keyframeCount, context.FileContext.AngleType));
						break;
					case KeyframeAttributes.LightColor:
						LightColor = reader.ReadKeyframeArrayAtOffset(offset, keyframeCount, LightColorLabelPrefix, lut, r => r.ReadColorSet(keyframeCount, ColorIOType.ARGB8_32));
						break;
					case KeyframeAttributes.Intensity:
						Intensity = reader.ReadKeyframeArrayAtOffset(offset, keyframeCount, IntensityLabelPrefix, lut, r => r.ReadVector2Set(keyframeCount, FloatIOType.Float));
						break;
					case KeyframeAttributes.Spot:
						Spotlight = reader.ReadKeyframeArrayAtOffset(offset, keyframeCount, SpotlightLabelPrefix, lut, r => r.ReadSpotlightSet(keyframeCount));
						break;
					case KeyframeAttributes.Point:
						Point = reader.ReadKeyframeArrayAtOffset(offset, keyframeCount, PointLabelPrefix, lut, r => r.ReadVector2Set(keyframeCount, FloatIOType.Float));
						break;
					case KeyframeAttributes.QuaternionRotation:
						QuaternionRotation = reader.ReadKeyframeArrayAtOffset(offset, keyframeCount, QuaternionRotationLabelPrefix, lut, r => r.ReadQuaternionSet(keyframeCount));
						break;
					default:
						throw new InvalidOperationException($"Invalid keyframe type {flag}!");
				}
			}
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, AnimationIOContext context)
		{
			List<int> frameCounts = [];

			ModelOffsetLUT lut = context.BaseContext.OffsetLUT;

			foreach((KeyframeAttributes type, IEnumerable<uint>? keys) in GetTypeKeyEnumerable())
			{
				if(!context.KeyframeType.HasFlag(type))
				{
					continue;
				}

				int count = keys?.Count() ?? 0;
				frameCounts.Add(count);
				if(count == 0)
				{
					writer.WriteOffsetValue(0);
					continue;
				}

				switch(type)
				{
					case KeyframeAttributes.Position:
						writer.WriteObjectOffset(Position, (w, v) => w.WriteVector3Set(v, FloatIOType.Float), lut);
						break;
					case KeyframeAttributes.EulerRotation:
						writer.WriteObjectOffset(EulerRotation, (w, v) => w.WriteVector3Set(v, context.FileContext.RotationAngleType), lut);
						break;
					case KeyframeAttributes.Scale:
						writer.WriteObjectOffset(Scale, (w, v) => w.WriteVector3Set(v, FloatIOType.Float), lut);
						break;
					case KeyframeAttributes.Vector:
						writer.WriteObjectOffset(Vector, (w, v) => w.WriteVector3Set(v, FloatIOType.Float), lut);
						break;
					case KeyframeAttributes.Vertex:
						writer.WriteObjectOffset(Vertex, (w, v) => w.WriteVector3ArrayData(v, context.BaseContext.OffsetLUT), lut);
						break;
					case KeyframeAttributes.Normal:
						writer.WriteObjectOffset(Normal, (w, v) => w.WriteVector3ArrayData(v, context.BaseContext.OffsetLUT), lut);
						break;
					case KeyframeAttributes.Target:
						writer.WriteObjectOffset(Target, (w, v) => w.WriteVector3Set(v, FloatIOType.Float), lut);
						break;
					case KeyframeAttributes.Roll:
						writer.WriteObjectOffset(Roll, (w, v) => w.WriteFloatSet(v, context.FileContext.AngleType), lut);
						break;
					case KeyframeAttributes.Angle:
						writer.WriteObjectOffset(Angle, (w, v) => w.WriteFloatSet(v, context.FileContext.AngleType), lut);
						break;
					case KeyframeAttributes.LightColor:
						writer.WriteObjectOffset(LightColor, (w, v) => w.WriteColorSet(v, ColorIOType.ARGB8_32), lut);
						break;
					case KeyframeAttributes.Intensity:
						writer.WriteObjectOffset(Intensity, (w, v) => w.WriteVector2Set(v, FloatIOType.Float), lut);
						break;
					case KeyframeAttributes.Spot:
						writer.WriteObjectOffset(Spotlight, (w, v) => w.WriteSpotlightSet(v), lut);
						break;
					case KeyframeAttributes.Point:
						writer.WriteObjectOffset(Point, (w, v) => w.WriteVector2Set(v, FloatIOType.Float), lut);
						break;
					case KeyframeAttributes.QuaternionRotation:
						writer.WriteObjectOffset(QuaternionRotation, (w, v) => w.WriteQuaternionSet(v), lut);
						break;
					default:
						throw new InvalidOperationException($"Unsupported keyframe type \"{type}\"");
				}
			}

			writer.WriteCollection(frameCounts);
		}

		/// <summary>
		/// Write keyframe data
		/// </summary>
		/// <param name="writer">The writer to write to</param>
		/// <param name="typePrefix">The type prefix to use for keyframe arrays</param>
		public void WriteKeyframes(AsciiWriter writer, string typePrefix)
		{
			void WriteKeyframes<T>(string type, KeyframeArray<T>? keyframes, string keyframeType, Func<T, string> keyframeToAscii)
			{
				if(keyframes == null)
				{
					return;
				}

				using(AsciiWriterBlockToken? block = writer.WriteStructBlockWithReference(type, keyframes))
				{
					if(block == null)
					{
						return;
					}

					foreach(KeyValuePair<uint, T> keyframe in keyframes)
					{
						writer.WriteLine($"\t{keyframeType}( {keyframe.Key}, {keyframeToAscii(keyframe.Value)} ),");
					}
				}
			}

			WriteKeyframes(typePrefix + "POSITION", Position, "MKEYF", v => v.ToAscii());
			WriteKeyframes(typePrefix + "ROTATION", EulerRotation, "MKEYA", v => v.ToAsciiDegrees());
			WriteKeyframes(typePrefix + "SCALE", Scale, "MKEYF", v => v.ToAscii());
			WriteKeyframes(typePrefix + "VECTOR", Vector, "MKEYF", v => v.ToAscii());
			WriteKeyframes(typePrefix + "POINTER", Vertex, "MKEYP", v => v.Label);
			WriteKeyframes(typePrefix + "POINTER", Normal, "MKEYP", v => v.Label);
			WriteKeyframes(typePrefix + "TARGET", Target, "MKEYF", v => v.ToAscii());
			WriteKeyframes(typePrefix + "ROLL", Roll, "MKEYA1", v => v.ToAsciiDegrees());
			WriteKeyframes(typePrefix + "ANGLE", Angle, "MKEYA1", v => v.ToAsciiDegrees());
			WriteKeyframes(typePrefix + "COLOR", LightColor, "MKEYF", v => v.FloatVector.AsVector3().ToAscii());
			WriteKeyframes(typePrefix + "INTENSITY", Intensity, "MKEYF2", v => v.ToAscii());
			WriteKeyframes(typePrefix + "SPOT_FACTORS", Spotlight, "MKEYSPOT", v => $"{v.Near.ToAscii()}, {v.Far.ToAscii()}, {v.InsideAngle.ToAsciiDegrees()}, {v.OutsideAngle.ToAsciiDegrees()}");
			WriteKeyframes(typePrefix + "POINT_FACTORS", Point, "MKEYF2", v => v.ToAscii());
			WriteKeyframes(typePrefix + "QROTATION", QuaternionRotation, "MKEYQ", v => $"{v.W.ToAscii()}, {v.X.ToAscii()}, {v.Y.ToAscii()}, {v.Z.ToAscii()}");
		}

		/// <inheritdoc/>
		public void Write(AsciiWriter writer, AnimationAsciiIOContext context)
		{
			List<int> frameCounts = [];

			writer.Write("\t");

			foreach(KeyframeAttributes type in Enum.GetValues<KeyframeAttributes>())
			{
				if(!context.KeyframeType.HasFlag(type))
				{
					continue;
				}

				ILabel? keyframes = type switch
				{
					KeyframeAttributes.Position => Position,
					KeyframeAttributes.EulerRotation => EulerRotation,
					KeyframeAttributes.Scale => Scale,
					KeyframeAttributes.Vector => Vector,
					KeyframeAttributes.Vertex => Vertex,
					KeyframeAttributes.Normal => Normal,
					KeyframeAttributes.Target => Target,
					KeyframeAttributes.Roll => Roll,
					KeyframeAttributes.Angle => Angle,
					KeyframeAttributes.LightColor => LightColor,
					KeyframeAttributes.Intensity => Intensity,
					KeyframeAttributes.Spot => Spotlight,
					KeyframeAttributes.Point => Point,
					KeyframeAttributes.QuaternionRotation => QuaternionRotation,
					_ => throw new InvalidOperationException($"Unsupported keyframe type \"{type}\""),
				};

				writer.WriteObjectidentifier(keyframes);
				writer.Write(", ");

				frameCounts.Add(((ICollection?)keyframes)?.Count ?? 0);
			}

			foreach(int framecount in frameCounts)
			{
				writer.Write($"{framecount}, ");
			}

			writer.WriteLine();
		}
	}
}