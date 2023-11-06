using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SA3D.Modeling.ObjectData
{
	/// <summary>
	/// Hierarchy object making up models.
	/// </summary>
	public partial class Node : ILabel
	{
		/// <summary>
		/// Byte size of a node structure.
		/// </summary>
		public const uint StructSize = 0x34;

		/// <inheritdoc/>
		public string Label { get; set; }


		/// <summary>
		/// Creates a new blank node.
		/// </summary>
		public Node()
		{
			Label = "object_" + StringExtensions.GenerateIdentifier();
		}


		/// <summary>
		/// Writes the node and its contents to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="format">Format to write the model in.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The address at which the node was written.</returns>
		/// <exception cref="NullReferenceException"></exception>
		public uint Write(EndianStackWriter writer, ModelFormat format, PointerLUT lut)
		{
			uint onWrite()
			{
				uint childAddress = Child?.Write(writer, format, lut) ?? 0;
				uint nextAddress = Next?.Write(writer, format, lut) ?? 0;
				uint attachAddress = Attach?.Write(writer, format, lut) ?? 0;

				uint result = writer.PointerPosition;

				writer.WriteUInt((uint)Attributes);
				writer.WriteUInt(attachAddress);

				writer.WriteVector3(Position);

				if(UseQuaternionRotation)
				{
					writer.WriteFloat(QuaternionRotation.X);
					writer.WriteFloat(QuaternionRotation.Y);
					writer.WriteFloat(QuaternionRotation.Z);
				}
				else
				{
					writer.WriteVector3(EulerRotation, FloatIOType.BAMS32);
				}

				writer.WriteVector3(Scale);

				writer.WriteUInt(childAddress);
				writer.WriteUInt(nextAddress);

				if(UseQuaternionRotation)
				{
					writer.WriteFloat(QuaternionRotation.W);
				}
				else
				{
					writer.WriteEmpty(4);
				}

				return result;
			}

			return lut.GetAddAddress(this, onWrite);
		}

		/// <summary>
		/// Reads a node and its contents off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="format">Format of the model to read.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The node that was read.</returns>
		public static Node Read(EndianStackReader reader, uint address, ModelFormat format, PointerLUT lut)
		{
			Node onRead()
			{
				Node result = new();

				NodeAttributes attributes = (NodeAttributes)reader.ReadUInt(address);
				result.SetAllNodeAttributes(attributes, RotationUpdateMode.Keep);

				if(reader.TryReadPointer(address + 4, out uint attachAddress))
				{
					result.Attach = Mesh.Attach.Read(reader, attachAddress, format, lut);
				}

				address += 8;
				Vector3 position = reader.ReadVector3(ref address);
				Vector3? eulerRotation = null;
				Quaternion? quaternionRotation = null;

				if(result.UseQuaternionRotation)
				{
					Vector3 vectorPart = reader.ReadVector3(ref address);
					float scalaPart = reader.ReadFloat(address + 20);

					quaternionRotation = new(vectorPart, scalaPart);
				}
				else
				{
					eulerRotation = reader.ReadVector3(ref address, FloatIOType.BAMS32);
				}

				Vector3 scale = reader.ReadVector3(ref address);

				result.UpdateTransforms(position, eulerRotation, quaternionRotation, scale, RotationUpdateMode.Keep);

				if(reader.TryReadPointer(address, out uint childAddr))
				{
					result.SetChild(Read(reader, childAddr, format, lut));
				}

				if(reader.TryReadPointer(address + 4, out uint siblingAddr))
				{
					result.SetNext(Read(reader, siblingAddr, format, lut));
				}

				return result;
			}

			return lut.GetAddLabeledValue(address, "object_", onRead);
		}


		/// <summary>
		/// Creates a shallow copy of the node with no relationships.
		/// </summary>
		/// <returns>The cloned node.</returns>
		public Node SimpleCopy()
		{
			Node result = (Node)MemberwiseClone();

			result.Parent = null;
			result.Child = null;
			result.Previous = null;
			result.Next = null;

			return result;
		}

		/// <summary>
		/// Creates a copy of the node with no relationships and a deep cloned attach.
		/// </summary>
		/// <returns>The cloned node.</returns>
		public Node AttachCopy()
		{
			Node result = SimpleCopy();
			if(result.Attach != null)
			{
				result.Attach = result.Attach.Clone();
			}

			return result;
		}

		/// <summary>
		/// Duplicated the node in place and inserts it after the original node.
		/// </summary>
		public Node Duplicate()
		{
			Node result = SimpleCopy();
			result.Label += "_Clone";
			InsertAfter(result);
			return result;
		}

		private Node BaseClone(Func<Node, Node> cloneFunc)
		{
			Dictionary<Node, Node> clones = new();

			foreach(Node node in GetTreeNodeEnumerable())
			{
				clones.Add(node, cloneFunc(node));
			}

			foreach(KeyValuePair<Node, Node> item in clones)
			{
				Node original = item.Key;
				Node clone = item.Value;

				clone.Parent = original.Parent == null ? null : clones[original.Parent];
				clone.Child = original.Child == null ? null : clones[original.Child];
				clone.Previous = original.Previous == null ? null : clones[original.Previous];
				clone.Next = original.Next == null ? null : clones[original.Next];
			}

			return clones[this];
		}

		/// <summary>
		/// Clones the entire tree and returns the clone of the node that the calling node. Attaches are reused.
		/// </summary>
		/// <returns>The cloned instance of the calling node</returns>
		public Node DeepSimpleCopy()
		{
			return BaseClone((n) => n.SimpleCopy());
		}

		/// <summary>
		/// Clones the entire tree including attaches and returns the clone of the node that the calling node.
		/// </summary>
		/// <returns>The cloned instance of the calling node</returns>
		public Node DeepAttachCopy()
		{
			return BaseClone((n) => n.AttachCopy());
		}


		/// <inheritdoc/>
		public override string ToString()
		{
			return Attach == null ? $"{Label} - /" : $"{Label} - {Attach}";
		}
	}
}
