using SA3D.Common.IO;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.Structs;
using System;

namespace SA3D.Modeling.Animation
{
	/// <summary>
	/// Level geometry animation (only used in sa1)
	/// </summary>
	public class LandEntryMotion
	{
		/// <summary>
		/// Size of the structure in bytes.
		/// </summary>
		public static uint StructSize => 24;

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
		public NodeMotion NodeMotion { get; set; }

		/// <summary>
		/// Texture list address to use.
		/// </summary>
		public uint TextureListPointer { get; set; }


		/// <summary>
		/// Creates a new geometry animation
		/// </summary>
		/// <param name="frame">First keyframe / Keyframe to start the animation at.</param>
		/// <param name="step">Keyframes traversed per frame-update / Animation Speed.</param>
		/// <param name="maxFrame">Last keyframe / Length of the animation.</param>
		/// <param name="model">Model that is being animated.</param>
		/// <param name="motion">Animation to play.</param>
		/// <param name="textureListPointer">Texture list address to use.</param>
		public LandEntryMotion(float frame, float step, float maxFrame, Node model, Motion motion, uint textureListPointer)
			: this(frame, step, maxFrame, model, new NodeMotion(model, motion), textureListPointer) { }

		/// <summary>
		/// Creates a new geometry animation
		/// </summary>
		/// <param name="frame">First keyframe / Keyframe to start the animation at.</param>
		/// <param name="step">Keyframes traversed per frame-update / Animation Speed.</param>
		/// <param name="maxFrame">Last keyframe / Length of the animation.</param>
		/// <param name="nodeMotion">Model and animation to use.</param>
		/// <param name="textureListPointer">Texture list address to use.</param>
		public LandEntryMotion(float frame, float step, float maxFrame, NodeMotion nodeMotion, uint textureListPointer)
			: this(frame, step, maxFrame, nodeMotion.Model, nodeMotion, textureListPointer) { }

		/// <summary>
		/// Creates a new geometry animation
		/// </summary>
		/// <param name="frame">First keyframe / Keyframe to start the animation at.</param>
		/// <param name="step">Keyframes traversed per frame-update / Animation Speed.</param>
		/// <param name="maxFrame">Last keyframe / Length of the animation.</param>
		/// <param name="model">Model that is being animated.</param>
		/// <param name="nodeMotion">Model and animation to use.</param>
		/// <param name="textureListPointer">Texture list address to use.</param>
		public LandEntryMotion(float frame, float step, float maxFrame, Node model, NodeMotion nodeMotion, uint textureListPointer)
		{
			Frame = frame;
			Step = step;
			MaxFrame = maxFrame;
			Model = model;
			NodeMotion = nodeMotion;
			TextureListPointer = textureListPointer;
		}



		/// <summary>
		/// Reads a geometry animation from a byte array
		/// </summary>
		/// <param name="data">Byte source</param>
		/// <param name="address">Address at which the geometry animation is located</param>
		/// <param name="format">Attach format</param>
		/// <param name="lut"></param>
		/// <returns></returns>
		public static LandEntryMotion Read(EndianStackReader data, uint address, ModelFormat format, PointerLUT lut)
		{
			float frame = data.ReadFloat(address);
			float step = data.ReadFloat(address + 4);
			float maxFrame = data.ReadFloat(address + 8);

			uint modelAddress = data.ReadPointer(address + 0xC);
			Node model = Node.Read(data, modelAddress, format, lut);

			uint motionAddress = data.ReadPointer(address + 0x10);
			NodeMotion action = NodeMotion.Read(data, motionAddress, format, lut);

			uint texListPtr = data.ReadUInt(address + 0x14);

			return new LandEntryMotion(frame, step, maxFrame, model, action, texListPtr);
		}

		/// <summary>
		/// Write the model and animation data to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="format">The format to write the model data in.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		public void WriteData(EndianStackWriter writer, ModelFormat format, PointerLUT lut)
		{
			Model.Write(writer, format, lut);
			NodeMotion.Write(writer, format, lut);
		}

		/// <summary>
		/// Writes the landentry motion structure to an endian stack writer.
		/// </summary>
		/// <remarks>
		/// Requires the data to be written before via <see cref="WriteData"/>
		/// </remarks>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <exception cref="NullReferenceException"></exception>
		public void Write(EndianStackWriter writer, PointerLUT lut)
		{
			if(!lut.Nodes.TryGetAddress(Model, out uint mdlAddress))
			{
				throw new NullReferenceException($"Model \"{Model.Label}\" has not been written yet / cannot be found in the pointer LUT!");
			}

			if(!lut.NodeMotions.TryGetAddress(NodeMotion, out uint actionAddress))
			{
				throw new NullReferenceException($"Nodemotion has not been written yet / cannot be found in the pointer LUT!");
			}

			writer.WriteFloat(Frame);
			writer.WriteFloat(Step);
			writer.WriteFloat(MaxFrame);
			writer.WriteUInt(mdlAddress);
			writer.WriteUInt(actionAddress);
			writer.WriteUInt(TextureListPointer);
		}
	}
}
