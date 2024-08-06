using SA3D.Common.IO;
using SA3D.Modeling.Structs;
using System;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	/// <summary>
	/// Triangle string structure for strip chunks.
	/// </summary>
	public struct ChunkStrip : ICloneable
	{
		/// <summary>
		/// Maximum allowed size of a (collection of) strip chunk(s)
		/// </summary>
		public const uint MaxByteSize = (ushort.MaxValue * 2) - 2;

		/// <summary>
		/// Triangle corners. 
		/// <br/> The first two corners are only used for their index.
		/// </summary>
		public ChunkCorner[] Corners { get; private set; }

		/// <summary>
		/// Whether to inverse the culling direction of the triangles.
		/// </summary>
		public bool Reversed { get; private set; }


		/// <summary>
		/// Creates a new strip.
		/// </summary>
		/// <param name="corners">Triangle corners.</param>
		/// <param name="reverse">Whether to inverse the culling direction of the triangles</param>
		public ChunkStrip(ChunkCorner[] corners, bool reverse)
		{
			Reversed = reverse;
			Corners = corners;
		}


		/// <summary>
		/// Calculates the size of the strip in bytes.
		/// </summary>
		/// <param name="texcoordCount">Number of texture coordinate sets in the strip.</param>
		/// <param name="hasNormal">Whether the strip has normals.</param>
		/// <param name="hasColor">Whether the strip has colors.</param>
		/// <param name="triangleAttributeCount">Number of attribute sets for every triangle.</param>
		/// <returns>The size of the strip in bytes.</returns>
		public readonly uint Size(int texcoordCount, bool hasNormal, bool hasColor, int triangleAttributeCount)
		{
			uint structSize = (uint)(2u
				+ (texcoordCount * 4u)
				+ (hasNormal ? 12u : 0u)
				+ (hasColor ? 4u : 0u));

			return (uint)(
				2u // strip header
				+ (Corners.Length * structSize) // individual corners
				+ ((Corners.Length - 2) * triangleAttributeCount * 2)); // triangle attributes
		}

		/// <summary>
		/// Reads a strip off an endian stack reader. Advances the address by the number of bytes read.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="texcoordCount">Number of texture coordinate sets in the strip.</param>
		/// <param name="hdTexcoord">Whether the texture coordinate data ranges from 0-1023, instead of 0-255</param>
		/// <param name="hasNormal">Whether the strip has normals.</param>
		/// <param name="hasColor">Whether the strip has colors.</param>
		/// <param name="triangleAttributeCount">Number of attribute sets for every triangle.</param>
		/// <returns>The strip that was read.</returns>
		public static ChunkStrip Read(EndianStackReader reader, ref uint address, int texcoordCount, bool hdTexcoord, bool hasNormal, bool hasColor, int triangleAttributeCount)
		{
			const float NormalFactor = 1f / short.MaxValue;

			short header = reader.ReadShort(address);
			bool reverse = header < 0;
			ChunkCorner[] corners = new ChunkCorner[Math.Abs(header)];

			bool hasUV = texcoordCount > 0;
			bool hasUV2 = texcoordCount > 1;
			float uvMultiplier = hdTexcoord ? 1f / 1023f : 1f / 255f;

			bool flag1 = triangleAttributeCount > 0;
			bool flag2 = triangleAttributeCount > 1;
			bool flag3 = triangleAttributeCount > 2;

			address += 2;

			for(int i = 0; i < corners.Length; i++)
			{
				ChunkCorner c = ChunkCorner.DefaultValues;
				c.Index = reader.ReadUShort(address);

				address += 2;

				if(hasUV)
				{
					c.Texcoord = reader.ReadVector2(ref address, FloatIOType.Short) * uvMultiplier;

					if(hasUV2)
					{
						c.Texcoord2 = reader.ReadVector2(ref address, FloatIOType.Short) * uvMultiplier;
					}
				}

				if(hasNormal)
				{
					c.Normal = reader.ReadVector3(ref address, FloatIOType.Short) * NormalFactor;
				}
				else if(hasColor)
				{
					c.Color = reader.ReadColor(ref address, ColorIOType.ARGB8_16);
				}

				if(flag1 && i > 1)
				{
					c.Attributes1 = reader.ReadUShort(address);
					address += 2;
					if(flag2)
					{
						c.Attributes2 = reader.ReadUShort(address);
						address += 2;
						if(flag3)
						{
							c.Attributes3 = reader.ReadUShort(address);
							address += 2;
						}
					}
				}

				corners[i] = c;
			}

			return new ChunkStrip(corners, reverse);
		}

		/// <summary>
		/// Writes the strip to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="texcoordCount">Number of texture coordinate sets in the strip.</param>
		/// <param name="hdTexcoord">Whether the texture coordinate data ranges from 0-1023, instead of 0-255</param>
		/// <param name="hasNormal">Whether the strip has normals.</param>
		/// <param name="hasColor">Whether the strip has colors.</param>
		/// <param name="triangleAttributeCount">Number of attribute sets for every triangle.</param>
		public readonly void Write(EndianStackWriter writer, int texcoordCount, bool hdTexcoord, bool hasNormal, bool hasColor, int triangleAttributeCount)
		{
			if(Corners.Length > short.MaxValue)
			{
				throw new InvalidOperationException("Strip has too many corners!");
			}

			writer.WriteShort(Reversed
				? (short)-Corners.Length
				: (short)Corners.Length);

			bool hasUV = texcoordCount > 0;
			bool hasUV2 = texcoordCount > 1;
			float uvMultiplier = hdTexcoord ? 1023f : 255f;

			bool flag1 = triangleAttributeCount > 0;
			bool flag2 = triangleAttributeCount > 1;
			bool flag3 = triangleAttributeCount > 2;

			for(int i = 0; i < Corners.Length; i++)
			{
				ChunkCorner c = Corners[i];
				writer.WriteUShort(c.Index);
				if(hasUV)
				{
					writer.WriteVector2(c.Texcoord * uvMultiplier, FloatIOType.Short);

					if(hasUV2)
					{
						writer.WriteVector2(c.Texcoord2 * uvMultiplier, FloatIOType.Short);
					}
				}

				if(hasNormal)
				{
					writer.WriteVector3(c.Normal * short.MaxValue, FloatIOType.Short);
				}
				else if(hasColor)
				{
					writer.WriteColor(c.Color, ColorIOType.ARGB8_16);
				}

				if(flag1 && i > 1)
				{
					writer.WriteUShort(c.Attributes1);
					if(flag2)
					{
						writer.WriteUShort(c.Attributes2);
						if(flag3)
						{
							writer.WriteUShort(c.Attributes3);
						}
					}
				}
			}
		}


		readonly object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the strip.
		/// </summary>
		/// <returns>The cloned strip.</returns>
		public readonly ChunkStrip Clone()
		{
			return new((ChunkCorner[])Corners.Clone(), Reversed);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{Reversed} : {Corners.Length}";
		}
	}
}
