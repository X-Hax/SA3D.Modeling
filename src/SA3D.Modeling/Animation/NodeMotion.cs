using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.Animation
{
	/// <summary>
	/// Pairs a node and motion together.
	/// </summary>
	public class NodeMotion : ILabel
	{
		/// <inheritdoc/>
		public string Label { get; set; }

		/// <summary>
		/// Assigned node.
		/// </summary>
		public Node Model { get; set; }

		/// <summary>
		/// Assigned motion.
		/// </summary>
		public Motion Animation { get; set; }


		/// <summary>
		/// Creates a new node motion.
		/// </summary>
		/// <param name="model">The model of the pair.</param>
		/// <param name="animation">The animation of the pair.</param>
		public NodeMotion(Node model, Motion animation)
		{
			Label = "action_" + StringExtensions.GenerateIdentifier();
			Model = model;
			Animation = animation;
		}


		/// <summary>
		/// Writes the node motion and its contents to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="format">The format in which the model should be written.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>Address at which the node motion was written.</returns>
		public uint Write(EndianStackWriter writer, ModelFormat format, PointerLUT lut)
		{
			uint OnWrite(NodeMotion nodeMotion)
			{
				uint nodeAddress = Model.Write(writer, format, lut);
				uint motionAddress = Animation.Write(writer, lut);

				uint result = writer.PointerPosition;

				writer.WriteUInt(nodeAddress);
				writer.WriteUInt(motionAddress);

				return result;
			}

			return lut.GetAddAddress(this, OnWrite);
		}

		/// <summary>
		/// Reads a NodeMotion off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="format">The format that the node should be read in.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The node motion pair that was read</returns>
		public static NodeMotion Read(EndianStackReader reader, uint address, ModelFormat format, PointerLUT lut)
		{
			NodeMotion onRead()
			{
				Node mdl = Node.Read(reader, reader.ReadPointer(address), format, lut);
				Motion mtn = Motion.Read(reader, reader.ReadPointer(address + 4), (uint)mdl.GetTreeNodeCount(), lut);

				return new NodeMotion(mdl, mtn);
			}

			return lut.GetAddLabeledValue(address, "action_", onRead);
		}

	}
}
