using SA3D.Common.IO;
using SA3D.Modeling.Structs;
using System;

namespace SA3D.Modeling.Mesh.Basic
{
	/// <summary>
	/// BASIC format material
	/// </summary>
	public struct BasicMaterial
	{
		/// <summary>
		/// Number of bytes the structure occupies.
		/// </summary>
		public const uint StructSize = 20;

		/// <summary>
		/// Material with default values.
		/// </summary>
		public static readonly BasicMaterial DefaultValues = new()
		{
			DiffuseColor = Color.ColorWhite,
			SpecularColor = new Color(0xFF, 0xFF, 0xFF, 0),
			UseAlpha = true,
			UseTexture = true,
			DoubleSided = true,
			FlatShading = false,
			IgnoreLighting = false,
			ClampU = false,
			ClampV = false,
			MirrorU = false,
			MirrorV = false,
			EnvironmentMap = false,
			DestinationAlpha = BlendMode.SrcAlphaInverted,
			SourceAlpha = BlendMode.SrcAlpha,
		};

		/// <summary>
		/// Diffuse color.
		/// </summary>
		public Color DiffuseColor { get; set; }

		/// <summary>
		/// Specular color.
		/// </summary>
		public Color SpecularColor { get; set; }

		/// <summary>
		/// Specular exponent.
		/// </summary>
		public float SpecularExponent { get; set; }

		/// <summary>
		/// Texture ID.
		/// </summary>
		public uint TextureID { get; set; }

		/// <summary>
		/// Attributes containing various information.
		/// </summary>
		public uint Attributes { get; set; }

		#region Attribute Properties

		/// <summary>
		/// User defined attributes.
		/// <br/> <see cref="Attributes"/> | 0x0000007F
		/// </summary>
		public byte UserAttributes
		{
			readonly get => (byte)(Attributes & 0x7Fu);
			set => Attributes = (Attributes & ~0x7Fu) | (value & 0x7Fu);
		}

		/// <summary>
		/// Editor property (?).
		/// <br/> <see cref="Attributes"/> | 0x00000080
		/// </summary>
		public bool PickStatus
		{
			readonly get => (Attributes & 0x80u) != 0;
			set => SetAttributeBit(0x80u, value);
		}

		/// <summary>
		/// Mipmad distance multiplier.
		/// <br/> <see cref="Attributes"/> | 0x00000F00
		/// </summary>
		public float MipmapDistanceMultiplier
		{
			readonly get => ((Attributes & 0xF00u) >> 8) * 0.25f;
			set => Attributes = (Attributes & ~0xF00u) | ((uint)Math.Max(0, Math.Min(0xF, Math.Round(value / 0.25, MidpointRounding.AwayFromZero))) << 8);
		}

		/// <summary>
		/// Super sampling (Anisotropic filtering?).
		/// <br/> <see cref="Attributes"/> | 0x00001000
		/// </summary>
		public bool SuperSample
		{
			readonly get => GetAttributeBit(0x1000u);
			set => SetAttributeBit(0x1000u, value);
		}

		/// <summary>
		/// Texture filter mode.
		/// <br/> <see cref="Attributes"/> | 0x00006000
		/// </summary>
		public FilterMode FilterMode
		{
			readonly get => (FilterMode)((Attributes >> 13) & 3);
			set => Attributes = (Attributes & ~0x6000u) | ((uint)value << 13);
		}

		/// <summary>
		/// Texture clamp along the V axis.
		/// <br/> <see cref="Attributes"/> | 0x00008000
		/// </summary>
		public bool ClampV
		{
			readonly get => GetAttributeBit(0x8000u);
			set => SetAttributeBit(0x8000u, value);
		}

		/// <summary>
		/// Texture clamp along the U axis.
		/// <br/> <see cref="Attributes"/> | 0x00010000
		/// </summary>
		public bool ClampU
		{
			readonly get => GetAttributeBit(0x10000u);
			set => SetAttributeBit(0x10000u, value);
		}

		/// <summary>
		/// Texture mirror along the V axis.
		/// <br/> <see cref="Attributes"/> | 0x00020000
		/// </summary>
		public bool MirrorV
		{
			readonly get => GetAttributeBit(0x20000u);
			set => SetAttributeBit(0x20000u, value);
		}

		/// <summary>
		/// Texture mirror along the U axis.
		/// <br/> <see cref="Attributes"/> | 0x00040000
		/// </summary>
		public bool MirrorU
		{
			readonly get => GetAttributeBit(0x40000u);
			set => SetAttributeBit(0x40000u, value);
		}

		/// <summary>
		/// Disables specular shading.
		/// <br/> <see cref="Attributes"/> | 0x00080000
		/// </summary>
		public bool IgnoreSpecular
		{
			readonly get => GetAttributeBit(0x80000u);
			set => SetAttributeBit(0x80000u, value);
		}

		/// <summary>
		/// Enables alpha blending.
		/// <br/> <see cref="Attributes"/> | 0x00100000
		/// </summary>
		public bool UseAlpha
		{
			readonly get => GetAttributeBit(0x100000u);
			set => SetAttributeBit(0x100000u, value);
		}

		/// <summary>
		/// Enables texture rendering.
		/// <br/> <see cref="Attributes"/> | 0x00200000
		/// </summary>
		public bool UseTexture
		{
			readonly get => GetAttributeBit(0x200000u);
			set => SetAttributeBit(0x200000u, value);
		}

		/// <summary>
		/// Applies the texture based on angle between camera and mesh normals (matcap method).
		/// <br/> <see cref="Attributes"/> | 0x00400000
		/// </summary>
		public bool EnvironmentMap
		{
			readonly get => GetAttributeBit(0x400000);
			set => SetAttributeBit(0x400000u, value);
		}

		/// <summary>
		/// Disables backface culling.
		/// <br/> <see cref="Attributes"/> | 0x00800000
		/// </summary>
		public bool DoubleSided
		{
			readonly get => GetAttributeBit(0x800000);
			set => SetAttributeBit(0x800000u, value);
		}

		/// <summary>
		/// Ignores interpolated normals and instead uses polygon-wide normals.
		/// <br/> <see cref="Attributes"/> | 0x01000000
		/// </summary>
		public bool FlatShading
		{
			readonly get => GetAttributeBit(0x1000000);
			set => SetAttributeBit(0x1000000u, value);
		}

		/// <summary>
		/// Disables shading altogether.
		/// <br/> <see cref="Attributes"/> | 0x02000000
		/// </summary>
		public bool IgnoreLighting
		{
			readonly get => GetAttributeBit(0x2000000);
			set => SetAttributeBit(0x2000000u, value);
		}

		/// <summary>
		/// Destination blend mode.
		/// <br/> <see cref="Attributes"/> | 0x1C000000
		/// </summary>
		public BlendMode DestinationAlpha
		{
			readonly get => (BlendMode)((Attributes >> 26) & 7);
			set => Attributes = (uint)((Attributes & ~0x1C000000) | ((uint)value << 26));
		}

		/// <summary>
		/// Source blend mode.
		/// <br/> <see cref="Attributes"/> | 0xE0000000
		/// </summary>
		public BlendMode SourceAlpha
		{
			readonly get => (BlendMode)((Attributes >> 29) & 7);
			set => Attributes = (Attributes & ~0xE0000000) | ((uint)value << 29);
		}

		private readonly bool GetAttributeBit(uint mask)
		{
			return (Attributes & mask) != 0;
		}

		private void SetAttributeBit(uint mask, bool value)
		{
			if(value)
			{
				Attributes |= mask;
			}
			else
			{
				Attributes &= ~mask;
			}
		}

		#endregion


		/// <summary>
		/// Creates a new basic material from a template.
		/// </summary>
		/// <param name="template">The template.</param>
		public BasicMaterial(BasicMaterial template)
		{
			DiffuseColor = template.DiffuseColor;
			SpecularColor = template.SpecularColor;
			SpecularExponent = template.SpecularExponent;
			TextureID = template.TextureID;
			Attributes = template.Attributes;
		}


		/// <summary>
		/// Reads a material from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which the material is located.</param>
		/// <returns>The read material.</returns>
		public static BasicMaterial Read(EndianStackReader reader, uint address)
		{
			Color dif = reader.ReadColor(ref address, ColorIOType.ARGB8_32);
			Color spec = reader.ReadColor(ref address, ColorIOType.ARGB8_32);
			float exp = reader.ReadFloat(address);
			uint texID = reader.ReadUInt(address + 4);
			uint attribs = reader.ReadUInt(address + 8);

			return new BasicMaterial()
			{
				DiffuseColor = dif,
				SpecularColor = spec,
				SpecularExponent = exp,
				TextureID = texID,
				Attributes = attribs
			};
		}

		/// <summary>
		/// Writes the materials structure to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public readonly void Write(EndianStackWriter writer)
		{
			writer.WriteColor(DiffuseColor, ColorIOType.ARGB8_32);
			writer.WriteColor(SpecularColor, ColorIOType.ARGB8_32);
			writer.WriteFloat(SpecularExponent);
			writer.WriteUInt(TextureID);
			writer.WriteUInt(Attributes);
		}


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is BasicMaterial material &&
				   DiffuseColor == material.DiffuseColor &&
				   SpecularColor == material.SpecularColor &&
				   SpecularExponent == material.SpecularExponent &&
				   TextureID == material.TextureID &&
				   Attributes == material.Attributes;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(DiffuseColor, SpecularColor, SpecularExponent, TextureID, Attributes);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Texture: {TextureID} / Use Alpha: {UseAlpha}";
		}

		/// <summary>
		/// Compares two materials for equality.
		/// </summary>
		/// <param name="left">Lefthand material</param>
		/// <param name="right">Righthand material</param>
		/// <returns>Whether the materials are equal.</returns>
		public static bool operator ==(BasicMaterial left, BasicMaterial right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two materials for inequality.
		/// </summary>
		/// <param name="left">Lefthand material</param>
		/// <param name="right">Righthand material</param>
		/// <returns>Whether the materials are inequal.</returns>
		public static bool operator !=(BasicMaterial left, BasicMaterial right)
		{
			return !(left == right);
		}
	}
}
