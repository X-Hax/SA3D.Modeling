using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Mesh.Basic;
using SA3D.Modeling.Mesh.Chunk;
using SA3D.Modeling.Mesh.Gamecube;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.Mesh
{
	/// <summary>
	/// 3D mesh data attach. Its possible for multiple attaches to make up one full mesh.
	/// </summary>
	public class Attach : ICloneable, ILabel
	{
		/// <inheritdoc/>
		public string Label { get; set; }

		/// <summary>
		/// Format of the attach.
		/// </summary>
		public virtual AttachFormat Format => AttachFormat.Buffer;

		/// <summary>
		/// Bounding sphere of the attach.
		/// </summary>
		public Bounds MeshBounds { get; set; }

		/// <summary>
		/// Mesh data ready to draw and used for converting to other attach formats.
		/// </summary>
		public BufferMesh[] MeshData { get; set; }


		/// <summary>
		/// Base constructor for derived attach types.
		/// </summary>
		protected Attach()
		{
			MeshData = Array.Empty<BufferMesh>();
			Label = "attach_" + GenerateIdentifier();
		}

		/// <summary>
		/// Create a new attach using existing meshdata.
		/// </summary>
		/// <param name="meshdata">The meshdata to use.</param>
		public Attach(BufferMesh[] meshdata)
		{
			MeshData = meshdata;
			Label = "attach_" + GenerateIdentifier();
		}


		/// <summary>
		/// Checks whether the attaches mesh data has/relies on weights.
		/// </summary>
		/// <returns>Whether the attaches mesh data has/relies on weights</returns>
		public virtual bool CheckHasWeights()
		{
			return MeshData.Any(x => x.ContinueWeight || x.Corners == null);
		}

		/// <summary>
		/// Returns opaque and transparent buffer meshes from <see cref="MeshData"/>. Meshes without polygons will be ignored.
		/// </summary>
		/// <returns></returns>
		public (BufferMesh[] opaque, BufferMesh[] transparent) GetDisplayMeshes()
		{
			List<BufferMesh> opaque = [];
			List<BufferMesh> transparent = [];

			foreach(BufferMesh mesh in MeshData)
			{
				if(mesh.Corners == null)
				{
					continue;
				}

				if(mesh.Material.UseAlpha)
				{
					transparent.Add(mesh);
				}
				else
				{
					opaque.Add(mesh);
				}
			}

			return (opaque.ToArray(), transparent.ToArray());
		}

		/// <summary>
		/// Recalculates <see cref="Bounds"/> from the attach data.
		/// </summary>
		public virtual void RecalculateBounds()
		{
			if(MeshData.Length == 0)
			{
				MeshBounds = default;
				return;
			}

			MeshBounds = Bounds.FromPoints(MeshData.SelectManyIgnoringNull(x => x.Vertices).Select(x => x.Position));
		}


		/// <summary>
		/// Reads an attach from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="format">Type of attach to read.</param>
		/// <param name="lut">Pointer references to use.</param>
		/// <returns>The buffer attach that was read.</returns>
		public static Attach Read(EndianStackReader reader, uint address, ModelFormat format, PointerLUT lut)
		{
			return format switch
			{
				ModelFormat.SA1 or ModelFormat.SADX => BasicAttach.Read(reader, address, format == ModelFormat.SADX, lut),
				ModelFormat.SA2 => ChunkAttach.Read(reader, address, lut),
				ModelFormat.SA2B => GCAttach.Read(reader, address, lut),
				ModelFormat.Buffer => ReadBuffer(reader, address, lut),
				_ => throw new ArgumentException("Invalid format.", nameof(format)),
			};
		}

		/// <summary>
		/// Reads a buffer attach from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="lut">Pointer references to use.</param>
		/// <returns>The buffer attach that was read.</returns>
		public static Attach ReadBuffer(EndianStackReader reader, uint address, PointerLUT lut)
		{
			Attach onRead()
			{
				uint meshCount = reader.ReadUInt(address);
				uint meshAddr = reader.ReadPointer(address + 4);

				uint[] meshAddresses = new uint[meshCount];
				for(int i = 0; i < meshCount; i++)
				{
					meshAddresses[i] = reader.ReadPointer(meshAddr);
					meshAddr += 4;
				}

				BufferMesh[] meshes = new BufferMesh[meshCount];

				for(int i = 0; i < meshCount; i++)
				{
					meshes[i] = BufferMesh.Read(reader, meshAddresses[i]);
				}

				return new Attach(meshes);
			}

			return lut.GetAddLabeledValue(address, "attach_", onRead);
		}


		/// <summary>
		/// Checks whether the attach can be written in the given model format.
		/// </summary>
		/// <param name="format">The format to check.</param>
		/// <returns>Whether the model can be written.</returns>
		public virtual bool CanWrite(ModelFormat format)
		{
			return format is ModelFormat.Buffer;
		}

		/// <summary>
		/// Writes the attach and returns the address to the mesh
		/// </summary>
		/// <param name="writer"></param>
		/// <param name="format"></param>
		/// <param name="lut"></param>
		/// <returns>address pointing to the attach</returns>
		public uint Write(EndianStackWriter writer, ModelFormat format, PointerLUT lut)
		{
			if(!CanWrite(format))
			{
				throw new ArgumentException($"Attach type \"{Format}\" does not support writing in model format \"{format}\".");
			}

			uint onWrite()
			{
				if(format == ModelFormat.Buffer)
				{
					return WriteBuffer(writer);
				}
				else
				{
					return WriteInternal(writer, format, lut);
				}
			}

			return lut.GetAddAddress(this, onWrite);
		}

		/// <summary>
		/// The internal method for writing attach data.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="format">The model format to write as.</param>
		/// <param name="lut">Pointer references to use.</param>
		/// <returns>The address at which the attach was written.</returns>
		protected virtual uint WriteInternal(EndianStackWriter writer, ModelFormat format, PointerLUT lut)
		{
			return WriteBuffer(writer);
		}

		private uint WriteBuffer(EndianStackWriter writer)
		{
			// write the meshes first
			uint[] meshAddresses = new uint[MeshData.Length];
			for(int i = 0; i < MeshData.Length; i++)
			{
				meshAddresses[i] = MeshData[i].Write(writer);
			}

			// write the pointer array
			uint arrayAddr = writer.Position + writer.ImageBase;
			for(int i = 0; i < MeshData.Length; i++)
			{
				writer.WriteUInt(meshAddresses[i]);
			}

			uint address = writer.PointerPosition;

			writer.WriteUInt((uint)meshAddresses.Length);
			writer.WriteUInt(arrayAddr);

			return address;
		}


		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the attach.
		/// </summary>
		/// <returns>The cloned attach.</returns>
		public virtual Attach Clone()
		{
			return new(MeshData.ContentClone()) { Label = Label };
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Label} - Buffer";
		}
	}
}
