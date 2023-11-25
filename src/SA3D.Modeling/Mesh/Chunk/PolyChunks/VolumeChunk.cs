using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Modeling.Mesh.Chunk.Structs;
using System;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Chunk containing a volume build from polygons.
	/// </summary>
	public class VolumeChunk : SizedChunk
	{
		private int _polygonAttributeCount;

		/// <summary>
		/// Polygons of the volume
		/// </summary>
		public IChunkVolumePolygon[] Polygons { get; }

		/// <summary>
		/// User attribute count (ranges from 0 to 3)
		/// </summary>
		public int PolygonAttributeCount
		{
			get => _polygonAttributeCount;
			set
			{
				if(value is < 0 or > 3)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "Value out of range. Must be between 0 and 3.");
				}

				_polygonAttributeCount = value;
			}
		}

		/// <summary>
		/// Raw size not constrained to 16 bits.
		/// </summary>
		public uint RawSize
		{
			get
			{
				uint size = 2;
				foreach(IChunkVolumePolygon p in Polygons)
				{
					size += p.Size(PolygonAttributeCount);
				}

				return size / 2;
			}
		}

		/// <inheritdoc/>
		public override ushort Size
		{
			get
			{
				uint result = RawSize;

				if(result > ushort.MaxValue)
				{
					throw new InvalidOperationException($"Strip chunk size ({result}) exceeds maximum size ({ushort.MaxValue}).");
				}

				return (ushort)result;
			}
		}


		/// <summary>
		/// Creates a new volume chunk.
		/// </summary>
		/// <param name="type">Type of volume chunk.</param>
		/// <param name="polygons">Polygons to use.</param>
		/// <param name="polygonAttributeCount">Number of attributes for each polygon.</param>
		public VolumeChunk(PolyChunkType type, IChunkVolumePolygon[] polygons, int polygonAttributeCount) : base(type)
		{
			if(type is < PolyChunkType.Volume_Polygon3 or > PolyChunkType.Volume_Strip)
			{
				throw new ArgumentException($"Type \"{type}\" is not a valid volume chunk type!");
			}

			Polygons = polygons;
			PolygonAttributeCount = polygonAttributeCount;
		}

		/// <summary>
		/// Creates a new, empty volume chunk.
		/// </summary>
		/// <param name="type">Type of volume chunk.</param>
		/// <param name="polygonCount">Number of polygons in the chunk.</param>
		/// <param name="polygonAttributeCount">Number of attributes for each polygon.</param>
		public VolumeChunk(PolyChunkType type, ushort polygonCount, int polygonAttributeCount)
			: this(type, new IChunkVolumePolygon[polygonCount], polygonAttributeCount) { }


		/// <inheritdoc/>
		protected override void InternalWrite(EndianStackWriter writer)
		{
			if(Polygons.Length > 0x3FFF)
			{
				throw new InvalidOperationException($"Poly count ({Polygons.Length}) exceeds maximum ({0x3FFF})");
			}

			base.InternalWrite(writer);

			writer.WriteUShort((ushort)(Polygons.Length | (PolygonAttributeCount << 14)));

			foreach(IChunkVolumePolygon p in Polygons)
			{
				p.Write(writer, PolygonAttributeCount);
			}
		}

		internal static VolumeChunk Read(EndianStackReader reader, ref uint address)
		{
			ushort header = reader.ReadUShort(address);
			ushort Header2 = reader.ReadUShort(address + 4);

			PolyChunkType type = (PolyChunkType)(header & 0xFF);
			byte attrib = (byte)(header >> 8);
			ushort polyCount = (ushort)(Header2 & 0x3FFFu);
			byte userAttribs = (byte)(Header2 >> 14);

			VolumeChunk result = new(type, polyCount, userAttribs)
			{
				Attributes = attrib,
			};

			address += 6;

			if(type == PolyChunkType.Volume_Polygon3)
			{
				for(int i = 0; i < polyCount; i++)
				{
					result.Polygons[i] = ChunkVolumeTriangle.Read(reader, ref address, userAttribs);
				}
			}
			else if(type == PolyChunkType.Volume_Polygon4)
			{
				for(int i = 0; i < polyCount; i++)
				{
					result.Polygons[i] = ChunkVolumeQuad.Read(reader, ref address, userAttribs);
				}
			}
			else // Volume_Strip
			{
				for(int i = 0; i < polyCount; i++)
				{
					result.Polygons[i] = ChunkVolumeStrip.Read(reader, ref address, userAttribs);
				}
			}

			return result;
		}


		/// <inheritdoc/>
		public override VolumeChunk Clone()
		{
			return new(Type, Polygons.ContentClone(), PolygonAttributeCount)
			{
				Attributes = Attributes
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Type} - {PolygonAttributeCount} : {Polygons.Length}";
		}
	}
}
