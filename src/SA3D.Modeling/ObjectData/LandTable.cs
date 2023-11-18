using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Animation;
using SA3D.Modeling.Mesh;
using SA3D.Modeling.Mesh.Basic;
using SA3D.Modeling.Mesh.Weighted;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.ObjectData
{
	/// <summary>
	/// Stage geometry information
	/// </summary>
	public class LandTable : ILabel
	{
		#region Properties

		/// <summary>
		/// Landtable name / C struct label
		/// </summary>
		public string Label { get; set; }

		/// <summary>
		/// Level geometry
		/// </summary>
		public ILabeledArray<LandEntry> Geometry { get; set; }

		/// <summary>
		/// Geometry animations (sa1)
		/// </summary>
		public ILabeledArray<LandEntryMotion> GeometryAnimations { get; set; }

		/// <summary>
		/// Landtable attributes
		/// </summary>
		public LandtableAttributes Attributes { get; set; }

		/// <summary>
		/// Draw distance
		/// </summary>
		public float DrawDistance { get; set; }

		/// <summary>
		/// Texture file name
		/// </summary>
		public string? TextureFileName { get; set; }

		/// <summary>
		/// Texture list pointer
		/// </summary>
		public uint TexListPtr { get; set; }

		/// <summary>
		/// Format of the landtable
		/// </summary>
		public ModelFormat Format { get; private set; }

		#endregion


		/// <summary>
		/// Creates a new Landtable.
		/// </summary>
		/// <param name="geometry">Level geometry.</param>
		/// <param name="geometryAnimations">Geometry animations.</param>
		/// <param name="format">Landtable Format.</param>
		public LandTable(ILabeledArray<LandEntry> geometry, ILabeledArray<LandEntryMotion> geometryAnimations, ModelFormat format)
		{
			string identifier = GenerateIdentifier();

			Label = "landtable_" + identifier;
			Geometry = geometry;
			GeometryAnimations = geometryAnimations;

			Format = format;
		}

		/// <summary>
		/// Creates a new Landtable.
		/// </summary>
		/// <param name="geometry">Level geometry.</param>
		/// <param name="format">Landtable Format.</param>
		public LandTable(ILabeledArray<LandEntry> geometry, ModelFormat format)
			: this(geometry, new LabeledArray<LandEntryMotion>(0), format) { }


		/// <summary>
		/// Generates buffer mesh data for the attaches of every land entry.
		/// </summary>
		/// <param name="optimize">Whether to optimize vertex and polygon data of the buffered meshes.</param>
		public void BufferMeshData(bool optimize)
		{
			HashSet<Attach> buffered = new();

			foreach(LandEntry landEntry in Geometry)
			{
				Node node = landEntry.Model;

				if(node.Next != null || node.Child != null)
				{
					node.BufferMeshData(optimize);
				}

				if(node.Attach == null || buffered.Contains(node.Attach))
				{
					continue;
				}

				node.BufferMeshData(optimize);
				buffered.Add(node.Attach);
			}
		}

		/// <summary>
		/// Converts the the model data and land entry structure to a different model format.
		/// </summary>
		/// <param name="newFormat">The model format to convert to.</param>
		/// <param name="bufferMode">How to handle buffered mesh data of the model.</param>
		/// <param name="optimize">Whether to optimize the converted model data.</param>
		/// <param name="forceUpdate">Whether to convert data even if it already is the same format.</param>
		/// <param name="updateBuffer">Whether to generate mesh data after conversion.</param>
		public void ConvertToFormat(ModelFormat newFormat, BufferMode bufferMode, bool optimize, bool forceUpdate = false, bool updateBuffer = false)
		{
			if(newFormat == Format && !forceUpdate)
			{
				return;
			}

			AttachFormat newAtcFormat = newFormat switch
			{
				ModelFormat.SA1 or ModelFormat.SADX => AttachFormat.BASIC,
				ModelFormat.SA2 => AttachFormat.CHUNK,
				ModelFormat.SA2B => AttachFormat.GC,
				ModelFormat.Buffer or _ => AttachFormat.Buffer,
			};

			void convertModels(AttachFormat format, IEnumerable<LandEntry> landEntries)
			{
				Dictionary<Attach, Attach?> convertedAttaches = new();

				foreach(LandEntry landEntry in Geometry)
				{
					Node node = landEntry.Model;

					if(node.Next != null || node.Child != null)
					{
						node.ConvertAttachFormat(format, bufferMode, optimize, false, forceUpdate, updateBuffer);
					}

					if(node.Attach == null)
					{
						continue;
					}
					else if(convertedAttaches.TryGetValue(node.Attach, out Attach? convertedAttach))
					{
						node.Attach = convertedAttach;
					}
					else
					{
						Attach previous = node.Attach;
						node.ConvertAttachFormat(format, bufferMode, optimize, false, forceUpdate, updateBuffer);
						convertedAttaches.Add(previous, node.Attach);
					}
				}
			}

			if(newAtcFormat is AttachFormat.Buffer)
			{
				BufferMeshData(optimize);
			}
			else if(newAtcFormat is AttachFormat.BASIC)
			{
				convertModels(newAtcFormat, Geometry);
			}
			else if(Format is ModelFormat.SA2 or ModelFormat.SA2B)
			{
				// we only need to convert visual geometry, since we are converting between SA2 formats here.
				convertModels(newAtcFormat, Geometry.Where(x => x.Model.Attach is not BasicAttach));
			}
			else
			{
				// converting from some hybrid format to SA2/B format
				List<LandEntry> visual = new();
				List<LandEntry> collision = new();

				foreach(LandEntry le in Geometry)
				{
					bool isCollision = le.SurfaceAttributes.CheckIsCollision();
					bool isVisual = !isCollision || le.SurfaceAttributes.HasFlag(SurfaceAttributes.Visible);

					if(isVisual && isCollision)
					{
						LandEntry collisionLE = le.Copy();
						LandEntry visualLE = le;

						collisionLE.SurfaceAttributes &= SurfaceAttributes.CollisionMask;
						visualLE.SurfaceAttributes &= SurfaceAttributes.VisualMask;

						collision.Add(collisionLE);
						visual.Add(visualLE);
					}
					else if(isVisual)
					{
						visual.Add(le);
					}
					else if(isCollision)
					{
						collision.Add(le);
					}

				}

				convertModels(newAtcFormat, visual);
				convertModels(AttachFormat.BASIC, collision);

				visual.AddRange(collision);

				Geometry = new LabeledArray<LandEntry>(Geometry.Label, visual.ToArray());
			}

			Format = newFormat;
		}

		/// <summary>
		/// Sorts land entries to be viable for SA2 / SA2B export.
		/// </summary>
		public void SortLandEntries()
		{
			if(Format is not ModelFormat.SA2 or ModelFormat.SA2B)
			{
				return;
			}

			LandEntry[] newOrder = new LandEntry[Geometry.Length];
			int count = 0;

			foreach(LandEntry le in Geometry)
			{
				if(le.Model.Attach?.Format == AttachFormat.BASIC)
				{
					continue;
				}

				newOrder[count] = le;
				count++;
			}

			foreach(LandEntry le in Geometry)
			{
				if(!(le.Model.Attach?.Format == AttachFormat.BASIC))
				{
					continue;
				}

				newOrder[count] = le;
				count++;
			}

			IList<LandEntry> list = Geometry;
			if(list.IsReadOnly)
			{
				Geometry = new LabeledReadOnlyArray<LandEntry>(Geometry.Label, newOrder);
			}
			else
			{
				for(int i = 0; i < newOrder.Length; i++)
				{
					list[i] = newOrder[i];
				}
			}
		}


		/// <summary>
		/// Writes the landtable to a stream
		/// </summary>
		/// <param name="writer">Output stream</param>
		/// <param name="lut"></param>
		public uint Write(EndianStackWriter writer, PointerLUT lut)
		{
			if(Format is ModelFormat.SA2 or ModelFormat.SA2B)
			{
				return lut.GetAddAddress(this, () => WriteSA2(writer, lut));
			}
			else
			{
				return lut.GetAddAddress(this, () => WriteSA1(writer, lut));
			}

		}

		private uint WriteSA2(EndianStackWriter writer, PointerLUT lut)
		{
			ushort visualCount = 0;
			bool visualFinished = false;

			// verifying order of land entries
			foreach(LandEntry le in Geometry)
			{
				AttachFormat? attachFormat = le.Model.GetAttachFormat();

				if(visualFinished && attachFormat != null && attachFormat != AttachFormat.BASIC)
				{
					throw new FormatException("Landtable entries are not ordered propertly! Visual models (BASIC) need to come after visual models.");
				}
				else if(!visualFinished)
				{
					if(attachFormat == AttachFormat.BASIC)
					{
						visualFinished = true;
					}
					else
					{
						visualCount++;
					}
				}
			}

			ModelFormat format = Format;
			int entryNum = 0;
			foreach(LandEntry landEntry in Geometry)
			{
				if(entryNum == visualCount)
				{
					format = ModelFormat.SA1;
				}

				landEntry.Model.Write(writer, format, lut);
				entryNum++;
			}

			uint geomAddr = lut.GetAddAddress(Geometry, () => WriteGeometry(writer, lut));
			uint texNameAddr = WriteTextureName(writer);

			uint result = writer.PointerPosition;

			writer.WriteUShort((ushort)Geometry.Length);
			writer.WriteUShort(visualCount);
			writer.WriteEmpty(8); // todo: figure out what these do
			writer.WriteFloat(DrawDistance);
			writer.WriteUInt(geomAddr);
			writer.WriteEmpty(4); // unused geometry animations
			writer.WriteUInt(texNameAddr);
			writer.WriteUInt(TexListPtr);

			return result;
		}

		private uint WriteSA1(EndianStackWriter writer, PointerLUT lut)
		{
			foreach(LandEntry le in Geometry)
			{
				le.Model.Write(writer, Format, lut);
			}

			foreach(LandEntryMotion lem in GeometryAnimations)
			{
				lem.WriteData(writer, Format, lut);
			}

			uint geomAddr = lut.GetAddAddress(Geometry, () => WriteGeometry(writer, lut));

			uint animAddr = 0;
			if(GeometryAnimations.Length > 0)
			{
				uint onWriteAnimations()
				{
					uint result = writer.PointerPosition;

					foreach(LandEntryMotion lem in GeometryAnimations)
					{
						lem.Write(writer, lut);
					}

					return result;
				}

				animAddr = lut.GetAddAddress(GeometryAnimations, onWriteAnimations);
			}

			uint texNameAddr = WriteTextureName(writer);

			uint result = writer.PointerPosition;

			writer.WriteUShort((ushort)Geometry.Length);
			writer.WriteUShort((ushort)GeometryAnimations.Length);
			writer.WriteUInt((uint)Attributes);
			writer.WriteFloat(DrawDistance);
			writer.WriteUInt(geomAddr);
			writer.WriteUInt(animAddr);
			writer.WriteUInt(texNameAddr);
			writer.WriteUInt(TexListPtr);
			writer.WriteEmpty(8); // two unused pointers

			return result;
		}

		private uint WriteGeometry(EndianStackWriter writer, PointerLUT lut)
		{
			uint result = writer.PointerPosition;

			foreach(LandEntry le in Geometry)
			{
				le.Write(writer, Format, lut);
			}

			return result;
		}

		private uint WriteTextureName(EndianStackWriter writer)
		{
			uint texNameAddr = 0;
			if(TextureFileName != null)
			{
				texNameAddr = writer.PointerPosition;
				writer.Write(Encoding.ASCII.GetBytes(TextureFileName + '\0'));
				writer.Align(4);
			}

			return texNameAddr;
		}


		/// <summary>
		/// Reads a landtable from a byte array
		/// </summary>
		/// <param name="data"></param>
		/// <param name="address"></param>
		/// <param name="format"></param>
		/// <param name="lut"></param>
		/// <returns></returns>
		public static LandTable Read(EndianStackReader data, uint address, ModelFormat format, PointerLUT lut)
		{
			LandTable onRead()
			{
				float radius;
				LandtableAttributes attribs = 0;

				string identifier = GenerateIdentifier();

				ILabeledArray<LandEntry> geometry;
				ILabeledArray<LandEntryMotion> anim;

				string texName = "";
				uint texListPtr;

				ushort geometryCount = data.ReadUShort(address);

				ushort nonBasicCount;
				uint geometryLoc;
				uint geometrySize;
				short animCount;
				uint animAddr;
				uint texNameLoc;
				uint texlistLoc;

				switch(format)
				{
					case ModelFormat.SA1:
					case ModelFormat.SADX:
					case ModelFormat.Buffer:
						attribs = (LandtableAttributes)data.ReadUInt(address + 4);
						radius = data.ReadFloat(address + 8);

						geometryLoc = address + 0xC;
						geometrySize = 0x24;
						nonBasicCount = geometryCount;

						animCount = data.ReadShort(address + 2);
						animAddr = data.ReadPointer(address + 0x10);

						texNameLoc = address + 0x14;
						texlistLoc = address + 0x18;
						break;
					case ModelFormat.SA2:
					case ModelFormat.SA2B:
						radius = data.ReadFloat(address + 0xC);

						geometryLoc = address + 0x10;
						geometrySize = 0x20;
						nonBasicCount = data.ReadUShort(address + 2);

						animAddr = 0;
						animCount = 0;

						texNameLoc = address + 0x18;
						texlistLoc = address + 0x1C;
						break;
					default:
						throw new InvalidDataException("Landtable format not valid");
				}

				LabeledArray<LandEntry> onReadGeometry(uint geometryAddr)
				{
					LabeledArray<LandEntry> result = new(geometryCount);

					for(int i = 0; i < geometryCount; i++)
					{
						result[i] = LandEntry.Read(data, geometryAddr, i >= nonBasicCount ? ModelFormat.SA1 : format, format, lut);
						geometryAddr += geometrySize;
					}

					return result;
				}

				geometry = data.TryReadPointer(geometryLoc, out uint geometryAddr)
					? lut.GetAddLabeledValue(geometryAddr, "collist_", onReadGeometry)
					: (ILabeledArray<LandEntry>)new LabeledArray<LandEntry>("collist_" + identifier, 0);

				LabeledArray<LandEntryMotion> onReadAnims()
				{
					LabeledArray<LandEntryMotion> result = new(animCount);

					for(int i = 0; i < animCount; i++)
					{
						result[i] = LandEntryMotion.Read(data, animAddr, format, lut);
						animAddr += LandEntryMotion.StructSize;
					}

					return result;
				}

				anim = animAddr != 0
					? lut.GetAddLabeledValue(animAddr, "animlist_", onReadAnims)
					: (ILabeledArray<LandEntryMotion>)new LabeledArray<LandEntryMotion>("animlist_" + identifier, 0);

				if(data.TryReadPointer(texNameLoc, out uint texNameAddr))
				{
					texName = data.ReadNullterminatedString(texNameAddr, Encoding.ASCII);
				}

				texListPtr = data.ReadUInt(texlistLoc);

				return new LandTable(geometry, anim, format)
				{
					DrawDistance = radius,
					Attributes = attribs,
					TexListPtr = texListPtr,
					TextureFileName = texName
				};
			}

			return lut.GetAddLabeledValue(address, "landtable_", onRead);
		}


		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Format} LandTable";
		}
	}
}
