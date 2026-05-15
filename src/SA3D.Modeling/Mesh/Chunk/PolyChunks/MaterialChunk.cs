using Amicitia.IO.Binary;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Material information for the following strip chunks
	/// </summary>
	public class MaterialChunk : SizedChunk
	{
		/// <summary>
		/// Whether the chunk is for the second material slot
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

				return (ushort)(2 * (
					(type & 1)
					+ ((type >> 1) & 1)
					+ ((type >> 2) & 1)
				));
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
			get;
			set
			{
				TypeAttribute(0x01, value.HasValue);
				field = value;
			}
		}

		/// <summary>
		/// Ambient color
		/// </summary>
		public Color? Ambient
		{
			get;
			set
			{
				TypeAttribute(0x02, value.HasValue);
				field = value;
			}
		}

		/// <summary>
		/// Specular color
		/// </summary>
		public Color? Specular
		{
			get;
			set
			{
				TypeAttribute(0x04, value.HasValue);
				field = value;
			}
		}

		/// <summary>
		/// Specular exponent <br/>
		/// Requires <see cref="Specular"/> to be set
		/// </summary>
		public byte SpecularExponent { get; set; }


		/// <summary>
		/// Creates a new, empty material chunk.
		/// </summary>
		public MaterialChunk() : base(PolyChunkType.Material_Empty) { }


		/// <inheritdoc/>
		protected override bool IsTypeApplicable(PolyChunkType type)
		{
			return type is >= PolyChunkType.Material_Empty and <= PolyChunkType.Material_DiffuseAmbientSpecular2;
		}

		private void TypeAttribute(byte val, bool state)
		{
			byte type = (byte)Type;
			Type = (PolyChunkType)(byte)(state
				? type | val
				: type & ~val);
		}

		/// <inheritdoc/>
		public override void Read(BinaryObjectReader reader)
		{
			base.Read(reader);

			byte type = (byte)Type;
			if((type & 0x01) != 0)
			{
				Diffuse = reader.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_16);
			}

			if((type & 0x02) != 0)
			{
				Ambient = reader.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_16);
			}

			if((type & 0x04) != 0)
			{
				Color spec = reader.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_16);
				SpecularExponent = spec.Alpha;
				spec.Alpha = 255;
				Specular = spec;
			}

		}

		/// <inheritdoc/>
		protected override void WriteData(BinaryObjectWriter writer)
		{
			base.WriteData(writer);

			if(Diffuse.HasValue)
			{
				writer.WriteObject(Diffuse.Value, ColorIOType.ARGB8_16);
			}

			if(Ambient.HasValue)
			{
				writer.WriteObject(Ambient.Value, ColorIOType.ARGB8_16);
			}

			if(Specular.HasValue)
			{
				Color wSpecular = Specular.Value;
				wSpecular.Alpha = SpecularExponent;
				writer.WriteObject(wSpecular, ColorIOType.ARGB8_16);
			}
		}
	}
}
