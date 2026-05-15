using Amicitia.IO.Binary;
using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Basic;
using SA3D.Modeling.Mesh.Chunk;
using SA3D.Modeling.Mesh.Ginja;
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
	public partial class Node : ILabel, IBinarySerializable<IOContext>
	{
		/// <inheritdoc/>
		public string Label { get; set; }

		/// <inheritdoc/>
		public string LabelPrefix => "object_";


		/// <summary>
		/// Creates a new blank node.
		/// </summary>
		public Node()
		{
			Label = LabelPrefix.GenerateIdentifier();
		}


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, IOContext context)
		{
			SetAllNodeAttributes((NodeAttributes)reader.ReadUInt32(), RotationUpdateMode.Keep);

			Attach = context.MeshFormat switch
			{
				Format.Basic
				or Format.BasicDX => reader.ReadObject<BasicMesh, IOContext>(context, context.PointerLUT),
				Format.Chunk => reader.ReadObject<ChunkMesh, IOContext>(context, context.PointerLUT),
				Format.Ginja => reader.ReadObject<GinjaMesh, IOContext>(context, context.PointerLUT),
				_ => throw new InvalidOperationException(),
			};

			Vector3 position = reader.ReadVector3();
			Vector3 rotation = reader.ReadVector3(UseQuaternionRotation ? FloatIOType.Float : FloatIOType.BAMS32);
			Vector3 scale = reader.ReadVector3();

			long childOffset = reader.ReadOffsetValue();
			long siblingOffset = reader.ReadOffsetValue();

			Quaternion quaternion = new(rotation, reader.ReadSingle());

			UpdateTransforms(
				position,
				UseQuaternionRotation ? null : rotation,
				UseQuaternionRotation ? quaternion : null,
				scale,
				RotationUpdateMode.Keep
			);

			if(reader.ReadObject<Node, IOContext>(context, context.PointerLUT) is Node child)
			{
				SetChild(child);
			}

			if(reader.ReadObject<Node, IOContext>(context, context.PointerLUT) is Node next)
			{
				SetNext(next);
			}
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, IOContext context)
		{
			writer.WriteUInt32((uint)Attributes);
			writer.WriteObjectOffset(Attach, context, context.PointerLUT);

			writer.WriteVector3(Position);

			if(UseQuaternionRotation)
			{
				writer.WriteVector3(
					new(QuaternionRotation.X, QuaternionRotation.Y, QuaternionRotation.Z),
					FloatIOType.Float
				);
			}
			else
			{
				writer.WriteVector3(EulerRotation, FloatIOType.BAMS32);
			}

			writer.WriteVector3(Scale);

			writer.WriteObjectOffset(Child, context, context.PointerLUT);
			writer.WriteObjectOffset(Next, context, context.PointerLUT);

			if(UseQuaternionRotation)
			{
				writer.WriteSingle(QuaternionRotation.W);
			}
			else
			{
				writer.WriteInt32(0);
			}
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
			Dictionary<Node, Node> clones = [];

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
