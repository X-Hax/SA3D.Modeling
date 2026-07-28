using J113D.Json;
using SA3D.Modeling.Mesh;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SA3D.Modeling.ObjectData
{
	[JsonConverter(typeof(JsonConverter))]
	public sealed partial class Node
	{
		private class JsonConverter : SimpleJsonObjectConverter<Node>
		{
			private const string _label = nameof(Label);

			private const string _noPosition = nameof(NoPosition);
			private const string _noRotation = nameof(NoRotation);
			private const string _noScale = nameof(NoScale);
			private const string _skipDraw = nameof(SkipDraw);
			private const string _skipChildren = nameof(SkipChildren);
			private const string _rotateZYX = nameof(RotateZYX);
			private const string _noAnimate = nameof(NoAnimate);
			private const string _noMorph = nameof(NoMorph);
			private const string _useQuaternionRotation = nameof(UseQuaternionRotation);

			private const string _position = nameof(Position);
			private const string _eulerRotation = nameof(EulerRotation);
			private const string _quaternionRotation = nameof(QuaternionRotation);
			private const string _scale = nameof(Scale);

			private const string _meshData = nameof(MeshData);

			private const string _children = "Children";
			private const string _siblings = "Siblings";


			/// <inheritdoc/>
			public override ReadOnlyDictionary<string, PropertyDefinition> PropertyDefinitions { get; } = new(new Dictionary<string, PropertyDefinition>()
			{
				{ _label, new(PropertyTokenType.String, string.Empty) },
				{ _noPosition, new(PropertyTokenType.Bool, false) },
				{ _noRotation, new(PropertyTokenType.Bool, false) },
				{ _noScale, new(PropertyTokenType.Bool, false) },
				{ _skipDraw, new(PropertyTokenType.Bool, false) },
				{ _skipChildren, new(PropertyTokenType.Bool, false) },
				{ _rotateZYX, new(PropertyTokenType.Bool, false) },
				{ _noAnimate, new(PropertyTokenType.Bool, false) },
				{ _noMorph, new(PropertyTokenType.Bool, false) },
				{ _useQuaternionRotation, new(PropertyTokenType.Bool, false) },
				{ _position, new(PropertyTokenType.String, null) },
				{ _eulerRotation, new(PropertyTokenType.String, null) },
				{ _quaternionRotation, new(PropertyTokenType.String, null) },
				{ _scale, new(PropertyTokenType.String, null) },
				{ _meshData, new(PropertyTokenType.Object | PropertyTokenType.String, null) },
				{ _children, new(PropertyTokenType.Array, null) },
				{ _siblings, new(PropertyTokenType.Array, null) },
			});

			/// <inheritdoc/>
			protected override object? ReadValue(ref Utf8JsonReader reader, string propertyName, ReadOnlyDictionary<string, object?> values, JsonSerializerOptions options)
			{
				switch(propertyName)
				{
					case _label:
						return reader.GetString();
					case _noPosition:
					case _noRotation:
					case _noScale:
					case _skipDraw:
					case _skipChildren:
					case _rotateZYX:
					case _noAnimate:
					case _noMorph:
					case _useQuaternionRotation:
						return reader.GetBoolean();
					case _position:
					case _eulerRotation:
					case _scale:
						return JsonSerializer.Deserialize<Vector3>(ref reader, options);
					case _quaternionRotation:
						return JsonSerializer.Deserialize<Quaternion>(ref reader, options);
					case _meshData:
						return JsonSerializer.Deserialize<MeshData>(ref reader, options);
					case _children:
					case _siblings:
						return JsonSerializer.Deserialize<Node[]>(ref reader, options);
					default:
						throw new InvalidPropertyException();
				}
			}

			/// <inheritdoc/>
			protected override Node Create(ReadOnlyDictionary<string, object?> values)
			{
				Node result = new()
				{
					Label = (string)values[_label]!,
					MeshData = (MeshData?)values[_meshData],
					NoPosition = (bool)values[_noPosition]!,
					NoRotation = (bool)values[_noRotation]!,
					NoScale = (bool)values[_noScale]!,
					SkipDraw = (bool)values[_skipDraw]!,
					SkipChildren = (bool)values[_skipChildren]!,
					NoAnimate = (bool)values[_noAnimate]!,
					NoMorph = (bool)values[_noMorph]!,
					UseQuaternionRotation = (bool)values[_useQuaternionRotation]!,
				};

				result.SetRotationZYX((bool)values[_rotateZYX]!, Modeling.ObjectData.Enums.RotationUpdateMode.Keep);

				if(result.UseQuaternionRotation)
				{
					result.UpdateTransforms((Vector3?)values[_position], (Quaternion?)values[_quaternionRotation], (Vector3?)values[_scale]);
				}
				else
				{
					result.UpdateTransforms((Vector3?)values[_position], (Vector3?)values[_eulerRotation], (Vector3?)values[_scale]);
				}

				if(values[_children] is Node[] children)
				{
					foreach(Node child in children)
					{
						result.AppendChild(child);
					}
				}

				// for when no parent but siblings
				if(values[_siblings] is Node[] siblings)
				{
					Node current = result;
					for(int i = 0; i < siblings.Length; i++)
					{
						current.SetNext(siblings[i]);
						current = siblings[i];
					}
				}

				return result;
			}

			/// <inheritdoc/>
			protected override void WriteValues(Utf8JsonWriter writer, Node value, JsonSerializerOptions options)
			{
				writer.WriteString(_label, value.Label);

				void writeBoolean(string name, bool value)
				{
					if(value)
					{
						writer.WriteBoolean(name, value);
					}
				}

				writeBoolean(_noPosition, value.NoPosition);
				writeBoolean(_noRotation, value.NoRotation);
				writeBoolean(_noScale, value.NoScale);
				writeBoolean(_skipDraw, value.SkipDraw);
				writeBoolean(_skipChildren, value.SkipChildren);
				writeBoolean(_rotateZYX, value.RotateZYX);
				writeBoolean(_noAnimate, value.NoAnimate);
				writeBoolean(_noMorph, value.NoMorph);
				writeBoolean(_useQuaternionRotation, value.UseQuaternionRotation);

				if(value.Position != Vector3.Zero)
				{
					writer.WritePropertyName(_position);
					JsonSerializer.Serialize(writer, value.Position, options);
				}

				if(value.UseQuaternionRotation && value.QuaternionRotation != Quaternion.Identity)
				{
					writer.WritePropertyName(_quaternionRotation);
					JsonSerializer.Serialize(writer, value.QuaternionRotation, options);
				}
				else if(!value.UseQuaternionRotation && value.EulerRotation != Vector3.Zero)
				{
					writer.WritePropertyName(_eulerRotation);
					JsonSerializer.Serialize(writer, value.EulerRotation, options);
				}

				if(value.Scale != Vector3.One)
				{
					writer.WritePropertyName(_scale);
					JsonSerializer.Serialize(writer, value.Scale, options);
				}

				if(value.MeshData != null)
				{
					writer.WritePropertyName(_meshData);
					JsonSerializer.Serialize(writer, value.MeshData, options);
				}

				if(value.ChildCount > 0)
				{
					writer.WritePropertyName(_children);
					JsonSerializer.Serialize(writer, value.GetChildren(), options);
				}

				// no parent but siblings
				if(value.Parent == null && value.Previous == null && value.Next != null)
				{
					List<Node> siblings = [];
					Node? next = value.Next;

					while(next != null)
					{
						siblings.Add(next);
						next = next.Next;
					}

					writer.WritePropertyName(_siblings);
					JsonSerializer.Serialize(writer, siblings, options);
				}
			}
		}
	}
}
