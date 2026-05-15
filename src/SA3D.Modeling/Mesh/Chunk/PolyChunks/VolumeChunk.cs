using Amicitia.IO.Binary;
using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Modeling.Mesh.Chunk.Structs;
using System;
using System.IO;
using System.Linq;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Chunk containing a volume build from polygons.
	/// </summary>
	public class VolumeChunk : SizedChunk
	{
		/// <summary>
		/// Polygons of the volume
		/// </summary>
		public IChunkVolumePolygon[] Polygons { get; set; }

		/// <summary>
		/// User attribute count (ranges from 0 to 3)
		/// </summary>
		public int PolygonAttributeCount
		{
			get;
			set
			{
				if(value is < 0 or > 3)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "Value out of range. Must be between 0 and 3.");
				}

				field = value;
			}
		}

		/// <inheritdoc/>
		public override ushort Size
		{
			get
			{
				uint result = CalculateByteSize() / 2;

				if(result > ushort.MaxValue)
				{
					throw new InvalidOperationException($"Strip chunk size ({result}) exceeds maximum size ({ushort.MaxValue}).");
				}

				return (ushort)uint.Clamp(CalculateByteSize() / 2, 0, ushort.MaxValue);
			}
		}


		/// <summary>
		/// Creates a new empty volume chunk (using <see cref="PolyChunkType.Volume_Triangle"/>)
		/// </summary>
		public VolumeChunk() : base(PolyChunkType.Volume_Triangle)
		{
			Polygons = [];
		}


		/// <inheritdoc/>
		protected override bool IsTypeApplicable(PolyChunkType type)
		{
			return type is >= PolyChunkType.Volume_Triangle and <= PolyChunkType.Volume_Strip;
		}


		/// <summary>
		/// Changes the type of the volume chunk.
		/// </summary>
		public void ChangeType(PolyChunkType type)
		{
			Type = type;
		}

		/// <summary>
		/// Checks whether polygon data is valid and throws an <see cref="InvalidDataException"/> if not.
		/// </summary>
		/// <exception cref="InvalidDataException"></exception>
		public void VerifyPolygonData()
		{
			Type expectedPolygonType = Type switch
			{
				PolyChunkType.Volume_Triangle => typeof(ChunkVolumeTriangle),
				PolyChunkType.Volume_Quad => typeof(ChunkVolumeQuad),
				PolyChunkType.Volume_Strip => typeof(ChunkVolumeStrip),
				_ => throw new InvalidDataException(),
			};

			if(Polygons.Any(x => x.GetType() != expectedPolygonType))
			{
				throw new InvalidDataException($"Not all polygons are of the expected type {expectedPolygonType}!");
			}

			if(Type == PolyChunkType.Volume_Strip)
			{
				foreach(ChunkVolumeStrip strip in Polygons.Cast<ChunkVolumeStrip>())
				{
					strip.VerifyPolygonData();
				}
			}
		}

		/// <summary>
		/// Calculate the chunks size.
		/// </summary>
		/// <returns></returns>
		public uint CalculateByteSize()
		{
			uint result = 2; // header ushort; strip count and triangle attributes

			result += Type switch
			{
				PolyChunkType.Volume_Triangle => (ushort)(Polygons.Length * (6u + (PolygonAttributeCount * 2u))),
				PolyChunkType.Volume_Quad => (ushort)(Polygons.Length * (8u + (PolygonAttributeCount * 2u))),
				PolyChunkType.Volume_Strip => (ushort)Polygons.Sum(x => 2u + (2 * (x.NumIndices + ((x.NumIndices - 2) * PolygonAttributeCount)))),
				_ => throw new InvalidDataException(),
			};

			return result;
		}

		/// <inheritdoc/>
		public override void Read(BinaryObjectReader reader)
		{
			base.Read(reader);

			ushort data = reader.ReadUInt16();
			int polygonCount = data & 0x3FFF;
			PolygonAttributeCount = (byte)(data >> 14);

			Polygons = Type switch
			{
				PolyChunkType.Volume_Triangle => reader.ReadObjectArray<ChunkVolumeTriangle>(polygonCount).Cast<IChunkVolumePolygon>().ToArray(),
				PolyChunkType.Volume_Quad => reader.ReadObjectArray<ChunkVolumeQuad>(polygonCount).Cast<IChunkVolumePolygon>().ToArray(),
				PolyChunkType.Volume_Strip => reader.ReadObjectArray<ChunkVolumeStrip>(polygonCount).Cast<IChunkVolumePolygon>().ToArray(),
				_ => throw new InvalidOperationException(),
			};
		}

		/// <inheritdoc/>
		protected override void WriteData(BinaryObjectWriter writer)
		{
			if(Polygons.Length > 0x3FFF)
			{
				throw new InvalidOperationException($"Poly count ({Polygons.Length}) exceeds maximum ({0x3FFF})");
			}

			VerifyPolygonData();

			base.WriteData(writer);

			writer.WriteUInt16((ushort)(Polygons.Length | (PolygonAttributeCount << 14)));

			foreach(IChunkVolumePolygon p in Polygons)
			{
				p.Write(writer, PolygonAttributeCount);
			}
		}


		/// <inheritdoc/>
		public override VolumeChunk Clone()
		{
			return new()
			{
				Type = Type,
				Attributes = Attributes,
				Polygons = Polygons.ContentClone(),
				PolygonAttributeCount = PolygonAttributeCount
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Type} - {PolygonAttributeCount} : {Polygons.Length}";
		}
	}
}
