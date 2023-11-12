using SA3D.Common.IO;
using SA3D.Modeling.Mesh.Gamecube.Enums;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.Mesh.Buffer
{
	/// <summary>
	/// Rendering properties for a buffer mesh.
	/// </summary>
	public struct BufferMaterial
	{
		/// <summary>
		/// Size of the structure in bytes.
		/// </summary>
		public const uint StructSize = 0x20;

		/// <summary>
		/// Default material values.
		/// </summary>
		public static readonly BufferMaterial DefaultValues = new()
		{
			Diffuse = Color.ColorWhite,
			Specular = Color.ColorWhite,
			SpecularExponent = 11,
			Ambient = Color.ColorBlack,
			SourceBlendMode = BlendMode.SrcAlpha,
			DestinationBlendmode = BlendMode.SrcAlphaInverted,
			TextureFiltering = FilterMode.Bilinear,
			GCShadowStencil = 1,
			GCTexCoordID = GCTexCoordID.TexCoord0,
			GCTexCoordType = GCTexCoordType.Matrix2x4,
			GCTexCoordSource = GCTexCoordSource.TexCoord0,
			GCMatrixID = GCTexCoordMatrix.Identity,
		};

		#region Storage properties

		/// <summary>
		/// The diffuse color.
		/// </summary>
		public Color Diffuse { readonly get; set; }

		/// <summary>
		/// The specular color.
		/// </summary>
		public Color Specular { readonly get; set; }

		/// <summary>
		/// The specular exponent.
		/// </summary>
		public float SpecularExponent { readonly get; set; }

		/// <summary>
		/// The Ambient color.
		/// </summary>
		public Color Ambient { readonly get; set; }

		/// <summary>
		/// Texture Index.
		/// </summary>
		public uint TextureIndex { readonly get; set; }

		/// <summary>
		/// Texture filtering mode.
		/// </summary>
		public FilterMode TextureFiltering { readonly get; set; }

		/// <summary>
		/// Mipmap distance multiplier.
		/// </summary>
		public float MipmapDistanceMultiplier { readonly get; set; }

		/// <summary>
		/// Source blend mode.
		/// </summary>
		public BlendMode SourceBlendMode { readonly get; set; }

		/// <summary>
		/// Destination blend mode.
		/// </summary>
		public BlendMode DestinationBlendmode { readonly get; set; }

		/// <summary>
		/// Additional Material attributes.
		/// </summary>
		public MaterialAttributes Attributes { readonly get; set; }

		/// <summary>
		/// Data container for all gamecube related info.
		/// </summary>
		public uint GamecubeData { readonly get; set; }

		#endregion

		#region Attribute Properties

		/// <summary>
		/// Whether textures should be rendered.
		/// <br/> Wrapper around flag in <see cref="Attributes"/>.
		/// </summary>
		public bool UseTexture
		{
			readonly get => HasAttributes(MaterialAttributes.UseTexture);
			set => SetAttributes(MaterialAttributes.UseTexture, value);
		}

		/// <summary>
		/// Enables anisotropic filtering.
		/// <br/> Wrapper around flag in <see cref="Attributes"/>.
		/// </summary>
		public bool AnisotropicFiltering
		{
			readonly get => HasAttributes(MaterialAttributes.AnisotropicFiltering);
			set => SetAttributes(MaterialAttributes.AnisotropicFiltering, value);
		}

		/// <summary>
		/// Clamps texture corrdinates along the horizontal axis between -1 and 1.
		/// <br/> Wrapper around flag in <see cref="Attributes"/>.
		/// </summary>
		public bool ClampU
		{
			readonly get => HasAttributes(MaterialAttributes.ClampU);
			set => SetAttributes(MaterialAttributes.ClampU, value);
		}

		/// <summary>
		/// Clamps texture corrdinates along the vertical axis between -1 and 1.
		/// <br/> Wrapper around flag in <see cref="Attributes"/>.
		/// </summary>
		public bool ClampV
		{
			readonly get => HasAttributes(MaterialAttributes.ClampV);
			set => SetAttributes(MaterialAttributes.ClampV, value);
		}

		/// <summary>
		/// Mirrors texture coordinates along the horizontal axis every other full unit.
		/// <br/> Wrapper around flag in <see cref="Attributes"/>.
		/// </summary>
		public bool MirrorU
		{
			readonly get => HasAttributes(MaterialAttributes.MirrorU);
			set => SetAttributes(MaterialAttributes.MirrorU, value);
		}

		/// <summary>
		/// Mirrors texture coordinates along the vertical axis every other full unit.
		/// <br/> Wrapper around flag in <see cref="Attributes"/>.
		/// </summary>
		public bool MirrorV
		{
			readonly get => HasAttributes(MaterialAttributes.MirrorV);
			set => SetAttributes(MaterialAttributes.MirrorV, value);
		}

		/// <summary>
		/// Whether to use normal mapping for textures.
		/// <br/> Wrapper around flag in <see cref="Attributes"/>.
		/// </summary>
		public bool NormalMapping
		{
			readonly get => HasAttributes(MaterialAttributes.NormalMapping);
			set => SetAttributes(MaterialAttributes.NormalMapping, value);
		}

		/// <summary>
		/// Ignores lighting as a whole.
		/// <br/> Wrapper around flag in <see cref="Attributes"/>.
		/// </summary>
		public bool NoLighting
		{
			readonly get => HasAttributes(MaterialAttributes.NoLighting);
			set => SetAttributes(MaterialAttributes.NoLighting, value);
		}

		/// <summary>
		/// Ignores ambient lighting.
		/// <br/> Wrapper around flag in <see cref="Attributes"/>.
		/// </summary>
		public bool NoAmbient
		{
			readonly get => HasAttributes(MaterialAttributes.NoAmbient);
			set => SetAttributes(MaterialAttributes.NoAmbient, value);
		}

		/// <summary>
		/// Ignores specular lighting.
		/// <br/> Wrapper around flag in <see cref="Attributes"/>.
		/// </summary>
		public bool NoSpecular
		{
			readonly get => HasAttributes(MaterialAttributes.NoSpecular);
			set => SetAttributes(MaterialAttributes.NoSpecular, value);
		}

		/// <summary>
		/// Ignores interpolated normals and instead renders every polygon flat.
		/// <br/> Wrapper around flag in <see cref="Attributes"/>.
		/// </summary>
		public bool Flat
		{
			readonly get => HasAttributes(MaterialAttributes.Flat);
			set => SetAttributes(MaterialAttributes.Flat, value);
		}

		/// <summary>
		/// Enables transparent rendering.
		/// <br/> Wrapper around flag in <see cref="Attributes"/>.
		/// </summary>
		public bool UseAlpha
		{
			readonly get => HasAttributes(MaterialAttributes.UseAlpha);
			set => SetAttributes(MaterialAttributes.UseAlpha, value);
		}

		/// <summary>
		/// Enables backface culling.
		/// <br/> Wrapper around flag in <see cref="Attributes"/>.
		/// </summary>
		public bool BackfaceCulling
		{
			readonly get => HasAttributes(MaterialAttributes.BackfaceCulling);
			set => SetAttributes(MaterialAttributes.BackfaceCulling, value);
		}

		#endregion

		#region Gamecube Properties

		/// <summary>
		/// GC Specific: Shadow stencil.
		/// </summary>
		public byte GCShadowStencil
		{
			readonly get => (byte)((GamecubeData >> 24) & 0xFF);
			set
			{
				GamecubeData &= 0xFFFFFF;
				GamecubeData |= (uint)value << 24;
			}
		}

		/// <summary>
		/// GC Specific: Output location to use for generated texture coordinates.
		/// </summary>
		public GCTexCoordID GCTexCoordID
		{
			readonly get => (GCTexCoordID)((GamecubeData >> 16) & 0xFF);
			set
			{
				GamecubeData &= 0xFF00FFFF;
				GamecubeData |= (uint)value << 16;
			}
		}

		/// <summary>
		/// GC Specific: The function to use for generating the texture coordinates
		/// </summary>
		public GCTexCoordType GCTexCoordType
		{
			readonly get => (GCTexCoordType)((GamecubeData >> 12) & 0xF);
			set
			{
				GamecubeData &= 0xFFFF0FFF;
				GamecubeData |= (uint)value << 12;
			}
		}

		/// <summary>
		/// GC Specific: The source which should be used to generate the texture coordinates
		/// </summary>
		public GCTexCoordSource GCTexCoordSource
		{
			readonly get => (GCTexCoordSource)((GamecubeData >> 4) & 0xFF);
			set
			{
				GamecubeData &= 0xFFFFF00F;
				GamecubeData |= (uint)value << 4;
			}
		}

		/// <summary>
		/// GC Specific: The ID of the matrix to use for generating the texture coordinates
		/// </summary>
		public GCTexCoordMatrix GCMatrixID
		{
			readonly get => (GCTexCoordMatrix)(GamecubeData & 0xF);
			set
			{
				GamecubeData &= 0xFFFFFFF0;
				GamecubeData |= (uint)value;
			}
		}

		#endregion

		/// <summary>
		/// Creates a new buffer material from a template.
		/// </summary>
		/// <param name="template">The template to use.</param>
		public BufferMaterial(BufferMaterial template)
		{
			Diffuse = template.Diffuse;
			Specular = template.Specular;
			SpecularExponent = template.SpecularExponent;
			Ambient = template.Ambient;
			TextureIndex = template.TextureIndex;
			TextureFiltering = template.TextureFiltering;
			MipmapDistanceMultiplier = template.MipmapDistanceMultiplier;
			SourceBlendMode = template.SourceBlendMode;
			DestinationBlendmode = template.DestinationBlendmode;
			Attributes = template.Attributes;
			GamecubeData = template.GamecubeData;
		}


		/// <summary>
		/// Set material attributes.
		/// </summary>
		/// <param name="attrib">The attributes to set.</param>
		/// <param name="state">New state for the attributes.</param>
		public void SetAttributes(MaterialAttributes attrib, bool state)
		{
			if(state)
			{
				Attributes |= attrib;
			}
			else
			{
				Attributes &= ~attrib;
			}
		}

		/// <summary>
		/// Checks if materials attributes are set.
		/// </summary>
		/// <param name="attrib">The attributes to check.</param>
		/// <returns>Whether all specified attributes are set.</returns>
		public readonly bool HasAttributes(MaterialAttributes attrib)
		{
			return Attributes.HasFlag(attrib);
		}


		/// <summary>
		/// Writes the material to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public readonly void Write(EndianStackWriter writer)
		{
			uint attributes = (uint)Attributes;
			attributes |= (uint)SourceBlendMode << 16;
			attributes |= (uint)DestinationBlendmode << 19;
			attributes |= (uint)TextureFiltering << 22;

			writer.WriteColor(Diffuse, ColorIOType.RGBA8);
			writer.WriteColor(Specular, ColorIOType.RGBA8);
			writer.WriteFloat(SpecularExponent);
			writer.WriteColor(Ambient, ColorIOType.RGBA8);
			writer.WriteUInt(TextureIndex);
			writer.WriteFloat(MipmapDistanceMultiplier);
			writer.WriteUInt(attributes);
			writer.WriteUInt(GamecubeData);
		}

		/// <summary>
		/// Reads a buffer material off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <returns>The buffer material that was read.</returns>
		public static BufferMaterial Read(EndianStackReader reader, uint address)
		{
			BufferMaterial result = default;

			result.Diffuse = reader.ReadColor(address, ColorIOType.RGBA8);
			result.Specular = reader.ReadColor(address + 4, ColorIOType.RGBA8);
			result.SpecularExponent = reader.ReadFloat(address + 8);
			result.Ambient = reader.ReadColor(address + 0xC, ColorIOType.RGBA8);
			result.TextureIndex = reader.ReadUInt(address + 0x10);
			result.MipmapDistanceMultiplier = reader.ReadFloat(address + 0x14);
			uint attributes = reader.ReadUInt(address + 0x18);
			result.GamecubeData = reader.ReadUInt(address + 0x1C);

			result.Attributes = (MaterialAttributes)(attributes & 0xFFFF);
			result.SourceBlendMode = (BlendMode)((attributes >> 16) & 7);
			result.DestinationBlendmode = (BlendMode)((attributes >> 19) & 7);
			result.TextureFiltering = (FilterMode)(attributes >> 22);

			return result;
		}
	}
}
