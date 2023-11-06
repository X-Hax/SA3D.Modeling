using SA3D.Common.IO;
using SA3D.Modeling.Structs;
using System;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Material information for the following strip chunks
	/// </summary>
	public class MaterialChunk : SizedChunk
	{
		private Color? _diffuse;
		private Color? _ambient;
		private Color? _specular;

		/// <summary>
		/// Whether the material type is a second type
		/// </summary>
		public bool Second
		{
			get => ((byte)Type & 0x08) != 0;
			set => TypeAttribute(0x08, value);
		}

		/// <inheritdoc/>
		public override ushort Size
		{
			get
			{
				byte type = (byte)Type;

				return (ushort)(2 *
					((type & 1)
					+ ((type >> 1) & 1)
					+ ((type >> 2) & 1)));
			}
		}

		/// <summary>
		/// Source blendmode
		/// </summary>
		public BlendMode SourceAlpha
		{
			get => (BlendMode)((Attributes >> 3) & 7);
			set => Attributes = (byte)((Attributes & ~0x38) | ((byte)value << 3));
		}

		/// <summary>
		/// Destination blendmode
		/// </summary>
		public BlendMode DestinationAlpha
		{
			get => (BlendMode)(Attributes & 7);
			set => Attributes = (byte)((Attributes & ~7) | (byte)value);
		}

		/// <summary>
		/// Diffuse color
		/// </summary>
		public Color? Diffuse
		{
			get => _diffuse;
			set
			{
				TypeAttribute(0x01, value.HasValue);
				_diffuse = value;
			}
		}

		/// <summary>
		/// Ambient color
		/// </summary>
		public Color? Ambient
		{
			get => _ambient;
			set
			{
				TypeAttribute(0x02, value.HasValue);
				_ambient = value;
			}
		}

		/// <summary>
		/// Specular color
		/// </summary>
		public Color? Specular
		{
			get => _specular;
			set
			{
				TypeAttribute(0x04, value.HasValue);
				_specular = value;
			}
		}

		/// <summary>
		/// Specular exponent <br/>
		/// Requires <see cref="Specular"/> to be set
		/// </summary>
		public byte SpecularExponent { get; set; }

		/// <summary>
		/// Creates a new material chunk. Defaults to <see cref="PolyChunkType.Material_Diffuse"/> with a white diffuse color.
		/// </summary>
		public MaterialChunk() : base(PolyChunkType.Material_Diffuse)
		{
			_diffuse = Color.ColorWhite;
		}

		private void TypeAttribute(byte val, bool state)
		{
			byte type = (byte)Type;
			Type = (PolyChunkType)(byte)(state
				? type | val
				: type & ~val);
		}

		internal static MaterialChunk Read(EndianStackReader reader, ref uint address)
		{
			ushort header = reader.ReadUShort(address);
			PolyChunkType type = (PolyChunkType)(header & 0xFF);
			// skipping size
			address += 4;

			MaterialChunk mat = new()
			{
				Attributes = (byte)(header >> 8)
			};

			if(((byte)type & 0x01) != 0)
			{
				mat.Diffuse = reader.ReadColor(ref address, ColorIOType.ARGB8_16);
			}

			if(((byte)type & 0x02) != 0)
			{
				mat.Ambient = reader.ReadColor(ref address, ColorIOType.ARGB8_16);
			}

			if(((byte)type & 0x04) != 0)
			{
				Color spec = reader.ReadColor(ref address, ColorIOType.ARGB8_16);
				mat.SpecularExponent = spec.Alpha;
				spec.Alpha = 255;
				mat.Specular = spec;
			}

			mat.Second = ((byte)type & 0x08) != 0;

			return mat;
		}

		/// <inheritdoc/>
		protected override void InternalWrite(EndianStackWriter writer)
		{
			if(_diffuse == null && _specular == null && _ambient == null)
			{
				throw new InvalidOperationException("Material has no colors and thus no valid type!");
			}

			base.InternalWrite(writer);

			if(_diffuse.HasValue)
			{
				writer.WriteColor(_diffuse.Value, ColorIOType.ARGB8_16);
			}

			if(_ambient.HasValue)
			{
				writer.WriteColor(_ambient.Value, ColorIOType.ARGB8_16);
			}

			if(_specular.HasValue)
			{
				Color wSpecular = _specular.Value;
				wSpecular.Alpha = SpecularExponent;
				writer.WriteColor(wSpecular, ColorIOType.ARGB8_16);
			}
		}
	}
}
