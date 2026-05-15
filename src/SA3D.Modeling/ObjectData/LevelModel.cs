using SA3D.Common.IO;
using SA3D.Modeling.Mesh;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.ObjectData.Events;
using SA3D.Modeling.Structs;
using System;
using System.Numerics;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.ObjectData
{
	/// <summary>
	/// Stage Geometry
	/// </summary>
	public class LandEntry
	{
		private Node _model;

		/// <summary>
		/// Model behind the landentry.
		/// </summary>
		public Node Model
		{
			get => _model;
			set
			{
				_model.OnTransformsUpdated -= OnTransformsUpdated;
				_model.OnAttachUpdated -= OnAttachUpdated;

				_model = value;

				_model.OnTransformsUpdated += OnTransformsUpdated;
				_model.OnAttachUpdated += OnAttachUpdated;
			}
		}

		/// <summary>
		/// World space bounds.
		/// <br/> Get automatically updated when the transforms change.
		/// </summary>
		public Bounds ModelBounds { get; set; }

		/// <summary>
		/// Geometry behavior attributes.
		/// </summary>
		public SurfaceAttributes SurfaceAttributes { get; set; }

		/// <summary>
		/// Block mapping bits
		/// </summary>
		public uint BlockBit { get; set; }

		/// <summary>
		/// No idea what this does at all, might be unused
		/// </summary>
		public uint Unknown { get; set; }


		/// <summary>
		/// Creates a new landentry object.
		/// </summary>
		/// <param name="node">Model behind the landentry.</param>
		/// <param name="surfaceAttributes">Geometry behavior attributes.</param>
		public LandEntry(Node node, SurfaceAttributes surfaceAttributes)
		{
			_model = node;
			_model.OnTransformsUpdated += OnTransformsUpdated;
			_model.OnAttachUpdated += OnAttachUpdated;

			SurfaceAttributes = surfaceAttributes;

			UpdateBounds();
		}

		private LandEntry(Node model, SurfaceAttributes attribs, uint blockbit, uint unknown, Bounds modelBounds)
		{
			_model = model;
			_model.OnTransformsUpdated += OnTransformsUpdated;
			_model.OnAttachUpdated += OnAttachUpdated;

			SurfaceAttributes = attribs;
			BlockBit = blockbit;
			Unknown = unknown;
			ModelBounds = modelBounds;
		}


		/// <summary>
		/// Creates a new land entry from an attach.
		/// </summary>
		/// <param name="attach"></param>
		/// <param name="surfaceAttributes">Geometry behavior attributes.</param>
		/// <returns></returns>
		public static LandEntry CreateWithAttach(Attach attach, SurfaceAttributes surfaceAttributes)
		{
			Node node = new()
			{
				Attach = attach,
				Label = "col_" + GenerateIdentifier()
			};

			return new(node, surfaceAttributes);
		}


		private void OnAttachUpdated(Node node, AttachUpdatedEventArgs args)
		{
			UpdateBounds();
		}

		private void OnTransformsUpdated(Node node, TransformsUpdatedEventArgs args)
		{
			UpdateBounds();
		}

		/// <summary>
		/// Copies the Attach-bounds and applies the landentries transform matrix to them
		/// </summary>
		public void UpdateBounds()
		{
			if(Model.Attach == null)
			{
				ModelBounds = default;
				return;
			}

			Vector3 position = Vector3.Transform(Model.Attach.MeshBounds.Position, Model.QuaternionRotation) + Model.Position;
			float radius = Model.Attach.MeshBounds.Radius * Model.Scale.GreatestValue();
			ModelBounds = new(position, radius);
		}


		/// <summary>
		/// Reads a landentry off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="modelFormat">Format to read model data in.</param>
		/// <param name="tableFormat">Landtable format that the landentry belongs to.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The land entry that was read.</returns>
		public static LandEntry Read(EndianStackReader reader, uint address, ModelFormat modelFormat, ModelFormat tableFormat, PointerLUT lut)
		{
			Bounds bounds = Bounds.Read(reader, ref address);
			if(tableFormat < ModelFormat.SA2)
			{
				address += 8; //sa1 has unused radius y and radius z values
			}

			uint modelAddress = reader.ReadPointer(address);
			Node model = Node.Read(reader, modelAddress, modelFormat, lut);

			uint unknown = 0;
			uint blockBit;

			SurfaceAttributes attribs;
			if(tableFormat == ModelFormat.Buffer)
			{
				unknown = reader.ReadUInt(address + 4);
				blockBit = reader.ReadUInt(address + 8);
				attribs = (SurfaceAttributes)reader.ReadULong(address + 12);
			}
			else if(tableFormat >= ModelFormat.SA2)
			{
				unknown = reader.ReadUInt(address + 4);
				blockBit = reader.ReadUInt(address + 8);
				attribs = ((SA2SurfaceAttributes)reader.ReadUInt(address + 12)).ToUniversal();
			}
			else
			{
				blockBit = reader.ReadUInt(address + 4);
				attribs = ((SA1SurfaceAttributes)reader.ReadUInt(address + 8)).ToUniversal();
			}

			return new(model, attribs, blockBit, unknown, bounds);
		}

		/// <summary>
		/// Writes the land entry to an endian stack writer.
		/// </summary>
		/// <remarks>Does not write the node itself.</remarks>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="format">Landtable format.</param>
		/// <param name="lut">Pointer references to utilize</param>
		public void Write(EndianStackWriter writer, ModelFormat format, PointerLUT lut)
		{
			if(!lut.Nodes.TryGetAddress(Model, out uint modelAddress))
			{
				throw new InvalidOperationException("Model has not been written!");
			}

			ModelBounds.Write(writer);
			if(format is ModelFormat.SA1 or ModelFormat.SADX)
			{
				writer.WriteEmpty(8); // unused radius y and radius z values
			}

			writer.WriteUInt(modelAddress);

			if(format is ModelFormat.Buffer)
			{
				writer.WriteUInt(Unknown);
				writer.WriteUInt(BlockBit);
				writer.WriteULong((ulong)SurfaceAttributes);
			}
			else if(format is ModelFormat.SA2 or ModelFormat.SA2B)
			{
				writer.WriteUInt(Unknown);
				writer.WriteUInt(BlockBit);
				writer.WriteUInt((uint)SurfaceAttributes.ToSA2());
			}
			else // SA1 or SADX
			{
				writer.WriteUInt(BlockBit);
				writer.WriteUInt((uint)SurfaceAttributes.ToSA1());
			}
		}


		/// <summary>
		/// Creates a copy of the landentry copies the node tree but reuses attaches.
		/// </summary>
		/// <returns></returns>
		public LandEntry Copy()
		{
			return new(Model.DeepSimpleCopy(), SurfaceAttributes, BlockBit, Unknown, ModelBounds);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Model.ToString();
		}
	}
}
