using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Modeling.Mesh.Chunk.Structs;
using System;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Chunk holding polygon data for rendering.
	/// </summary>
	public class StripChunk : SizedChunk
	{
		private int _triangleAttributeCount;

		#region Type dependent properties

		/// <summary>
		/// The number of texture coordinate sets the polygons utilize.
		/// </summary>
		public int TexcoordCount => Type.GetStripTexCoordCount();

		/// <summary>
		/// Whether texture coordinates are in the 0-1023 range, instead of 0-255.
		/// </summary>
		public bool HasHDTexcoords => Type.CheckStripHasHDTexcoords();

		/// <summary>
		/// Whether polygons utilize normals.
		/// </summary>
		public bool HasNormals => Type.CheckStripHasNormals();

		/// <summary>
		/// Whether polygons utilize colors.
		/// </summary>
		public bool HasColors => Type.CheckStripHasColors();

		#endregion

		#region Attribute Properties

		/// <summary>
		/// Ignores lighting as a whole.
		/// <br/> 0x01 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool IgnoreLight
		{
			get => GetAttributeBit(1);
			set => SetAttributeBit(0x01, value);
		}

		/// <summary>
		/// Ignores specular lighting.
		/// <br/> 0x02 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool IgnoreSpecular
		{
			get => GetAttributeBit(2);
			set => SetAttributeBit(0x02, value);
		}

		/// <summary>
		/// Ignores ambient lighting.
		/// <br/> 0x04 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool IgnoreAmbient
		{
			get => GetAttributeBit(4);
			set => SetAttributeBit(0x04, value);
		}

		/// <summary>
		/// Renders polygons with transparency enabled.
		/// <br/> 0x08 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool UseAlpha
		{
			get => GetAttributeBit(8);
			set => SetAttributeBit(0x08, value);
		}

		/// <summary>
		/// Disables backface culling.
		/// <br/> 0x10 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool DoubleSide
		{
			get => GetAttributeBit(0x10);
			set => SetAttributeBit(0x10, value);
		}

		/// <summary>
		/// Ignore normals and render every polygon flat.
		/// <br/> 0x20 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool FlatShading
		{
			get => GetAttributeBit(0x20);
			set => SetAttributeBit(0x20, value);
		}

		/// <summary>
		/// Ignore texture coordinates and use normals for environment (matcap/normal) mapping.
		/// <br/> 0x40 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool EnvironmentMapping
		{
			get => GetAttributeBit(0x40);
			set => SetAttributeBit(0x40, value);
		}

		/// <summary>
		/// Unknown effect.
		/// <br/> 0x80 in <see cref="PolyChunk.Attributes"/>.
		/// </summary>
		public bool UnknownAttribute
		{
			get => GetAttributeBit(0x80);
			set => SetAttributeBit(0x80, value);
		}

		private bool GetAttributeBit(byte bits)
		{
			return (Attributes & bits) != 0;
		}

		private void SetAttributeBit(byte bits, bool value)
		{
			if(value)
			{
				Attributes |= bits;
			}
			else
			{
				Attributes &= (byte)~bits;
			}
		}

		#endregion


		/// <summary>
		/// Triangle strips making up the polygons.
		/// </summary>
		public ChunkStrip[] Strips { get; private set; }

		/// <summary>
		/// Number of custom attributes for each triangle.
		/// </summary>
		public int TriangleAttributeCount
		{
			get => _triangleAttributeCount;
			set
			{
				if(value is < 0 or > 3)
				{
					throw new ArgumentOutOfRangeException(nameof(value), "Value out of range. Must be between 0 and 3.");
				}

				_triangleAttributeCount = value;
			}
		}

		/// <summary>
		/// Raw size not constrained to 16 bits.
		/// </summary>
		public uint RawSize
		{
			get
			{
				uint result = 2; // header ushort; strip count and triangle attributes

				int texcoordCount = TexcoordCount;
				bool hasNormals = HasNormals;
				bool hasColors = HasColors;

				foreach(ChunkStrip str in Strips)
				{
					result += str.Size(texcoordCount, hasNormals, hasColors, _triangleAttributeCount);
				}

				return result / 2;
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
		/// Creates a new strip chunk.
		/// </summary>
		/// <param name="type">Type of strip chunk.</param>
		/// <param name="strips">Triangle strips.</param>
		/// <param name="triangleAttributeCount">Number of custom attributes for each triangle.</param>
		/// <exception cref="ArgumentException"></exception>
		public StripChunk(PolyChunkType type, ChunkStrip[] strips, int triangleAttributeCount) : base(type)
		{
			if(type is < PolyChunkType.Strip_Blank or > PolyChunkType.Strip_HDTexDouble)
			{
				throw new ArgumentException($"Type \"{type}\" is not a valid strip chunk type!");
			}

			Strips = strips;
			TriangleAttributeCount = triangleAttributeCount;
		}

		/// <summary>
		/// Creates a new strip chunk.
		/// </summary>
		/// <param name="type">Type of strip chunk.</param>
		/// <param name="stripCount">Number of strips to create the stripchunk with.</param>
		/// <param name="triangleAttributeCount">Number of custom attributes for each triangle.</param>
		/// <exception cref="ArgumentException"></exception>
		public StripChunk(PolyChunkType type, ushort stripCount, int triangleAttributeCount)
			: this(type, new ChunkStrip[stripCount], triangleAttributeCount) { }


		/// <summary>
		/// Changes the type of the strip chunk.
		/// </summary>
		public void ChangeType(PolyChunkType type)
		{
			if(type is < PolyChunkType.Strip_Blank or > PolyChunkType.Strip_HDTexDouble)
			{
				throw new ArgumentException($"Type \"{type}\" is not a valid strip chunk type!");
			}

			Type = type;
		}


		internal static StripChunk Read(EndianStackReader reader, ref uint address)
		{
			ushort header = reader.ReadUShort(address);
			ushort header2 = reader.ReadUShort(address + 4);

			PolyChunkType type = (PolyChunkType)(header & 0xFF);
			byte attribs = (byte)(header >> 8);
			ushort polyCount = (ushort)(header2 & 0x3FFFu);
			byte triangleAttributeCount = (byte)(header2 >> 14);

			StripChunk result = new(type, polyCount, triangleAttributeCount)
			{
				Attributes = attribs
			};

			address += 6;

			int texcoordCount = result.TexcoordCount;
			bool hasNormals = result.HasNormals;
			bool hasColors = result.HasColors;
			bool hdTexcoord = result.HasHDTexcoords;

			for(int i = 0; i < polyCount; i++)
			{
				result.Strips[i] = ChunkStrip.Read(reader, ref address, texcoordCount, hdTexcoord, hasNormals, hasColors, triangleAttributeCount);
			}

			return result;
		}

		/// <inheritdoc/>
		protected override void InternalWrite(EndianStackWriter writer)
		{
			if(Strips.Length > 0x3FFF)
			{
				throw new InvalidOperationException($"Strip count ({Strips.Length}) exceeds maximum ({0x3FFF})");
			}

			base.InternalWrite(writer);

			writer.WriteUShort((ushort)(Strips.Length | (TriangleAttributeCount << 14)));

			int texcoordCount = TexcoordCount;
			bool hasNormals = HasNormals;
			bool hasColors = HasColors;
			bool hdTexcoord = HasHDTexcoords;

			foreach(ChunkStrip s in Strips)
			{
				s.Write(writer, texcoordCount, hdTexcoord, hasNormals, hasColors, _triangleAttributeCount);
			}
		}


		/// <inheritdoc/>
		public override StripChunk Clone()
		{
			return new(
				Type,
				Strips.ContentClone(),
				TriangleAttributeCount)
			{
				Attributes = Attributes
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Type} - 0x{Attributes:X2}, {TriangleAttributeCount} : {Strips.Length}";
		}
	}
}
