using SA3D.Common.IO;
using SA3D.Modeling.Mesh.Chunk.Structs;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Chunk
{
	/// <summary>
	/// Set of vertex data of a chunk model
	/// </summary>
	public class VertexChunk : ICloneable
	{
		/// <summary>
		/// Type of vertex chunk.
		/// </summary>
		public VertexChunkType Type { get; }

		/// <summary>
		/// Various attributes.
		/// </summary>
		public byte Attributes { get; }

		/// <summary>
		/// Determines how vertices are applied to the vertex cache.
		/// </summary>
		public WeightStatus WeightStatus => (WeightStatus)(Attributes & 3);

		/// <summary>
		/// Offset that gets added to every index in the vertices.
		/// </summary>
		public ushort IndexOffset { get; set; }

		/// <summary>
		/// Whether the chunk has weighted vertex data.
		/// </summary>
		public bool HasWeight => Type.CheckHasWeights();

		/// <summary>
		/// Whether the vertices contain normals.
		/// </summary>
		public bool HasNormals => Type.CheckHasNormal();

		/// <summary>
		/// Whether the vertices contain diffuse colors.
		/// </summary>
		public bool HasDiffuseColors => Type.CheckHasDiffuseColor();

		/// <summary>
		/// Whether the vertices contain specular colors.
		/// </summary>
		public bool HasSpecularColors => Type.CheckHasSpecularColor();

		/// <summary>
		/// Vertices of the chunk
		/// </summary>
		public ChunkVertex[] Vertices { get; }


		/// <summary>
		/// Creates a new Vertex chunk.
		/// </summary>
		/// <param name="type">Vertex chunk type.</param>
		/// <param name="attributes">Attributes of the chunk.</param>
		/// <param name="indexOffset">Index offset for all vertices.</param>
		/// <param name="vertices">Vertex data.</param>
		public VertexChunk(VertexChunkType type, byte attributes, ushort indexOffset, ChunkVertex[] vertices)
		{
			if(!Enum.IsDefined(type) || type is VertexChunkType.End or VertexChunkType.Null)
			{
				throw new ArgumentException($"Vertex chunk type is invalid: {type}", nameof(type));
			}

			Type = type;
			Attributes = attributes;
			IndexOffset = indexOffset;
			Vertices = vertices;
		}

		/// <summary>
		/// Creates a new Vertex chunk with all relevant data
		/// </summary>
		/// <param name="type">Vertex chunk type.</param>
		/// <param name="weightstatus">Determines how vertices are applied to the vertex cache.</param>
		/// <param name="indexOffset">Index offset for all vertices.</param>
		/// <param name="vertices">Vertex data.</param>
		public VertexChunk(VertexChunkType type, WeightStatus weightstatus, ushort indexOffset, ChunkVertex[] vertices)
			: this(type, (byte)weightstatus, indexOffset, vertices) { }


		/// <summary>
		/// Writes a vertex chunk to an endian stack writer. Splits it up into multiple chunks if necessary.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public void Write(EndianStackWriter writer)
		{
			if(Vertices.Length > short.MaxValue)
			{
				throw new InvalidOperationException($"Vertex count ({Vertices.Length}) exceeds maximum vertex count (32767)");
			}

			ushort vertSize = Type.GetIntegerSize();
			ushort vertexLimitPerChunk = (ushort)((ushort.MaxValue - 1) / vertSize); // -1 because header2 also counts as part of the size, which is always there
			ChunkVertex[] remainingVerts = (ChunkVertex[])Vertices.Clone();
			uint header1Base = (uint)Type | (uint)(Attributes << 8);
			ushort offset = IndexOffset;

			bool hasNormal = Type.CheckHasNormal();
			bool vec4 = Type.CheckIsVec4();
			bool normal32 = Type.CheckIsNormal32();

			while(remainingVerts.Length > 0)
			{
				ushort vertCount = ushort.Min((ushort)remainingVerts.Length, vertexLimitPerChunk);
				ushort size = (ushort)((vertCount * vertSize) + 1);

				writer.WriteUInt(header1Base | (uint)(size << 16));
				writer.WriteUInt(offset | (uint)(vertCount << 16));

				for(int i = 0; i < vertCount; i++)
				{
					ChunkVertex vtx = remainingVerts[i];
					writer.WriteVector3(vtx.Position);
					if(vec4)
					{
						writer.WriteFloat(1.0f);
					}

					if(hasNormal)
					{
						if(normal32)
						{
							ushort x = (ushort)Math.Round((vtx.Normal.X + 1) * 0x3FF);
							ushort y = (ushort)Math.Round((vtx.Normal.Y + 1) * 0x3FF);
							ushort z = (ushort)Math.Round((vtx.Normal.Z + 1) * 0x3FF);

							uint composed = (uint)((x << 20) | (y << 10) | z);
							writer.WriteUInt(composed);
						}
						else
						{
							writer.WriteVector3(vtx.Normal);
							if(vec4)
							{
								writer.WriteFloat(0.0f);
							}
						}
					}

					switch(Type)
					{
						case VertexChunkType.Diffuse:
						case VertexChunkType.NormalDiffuse:
						case VertexChunkType.Normal32Diffuse:
							writer.WriteColor(vtx.Diffuse, ColorIOType.ARGB8_32);
							break;
						case VertexChunkType.DiffuseSpecular5:
						case VertexChunkType.NormalDiffuseSpecular5:
							writer.WriteColor(vtx.Diffuse, ColorIOType.RGB565);
							writer.WriteColor(vtx.Specular, ColorIOType.RGB565);
							break;
						case VertexChunkType.DiffuseSpecular4:
						case VertexChunkType.NormalDiffuseSpecular4:
							writer.WriteColor(vtx.Diffuse, ColorIOType.ARGB4);
							writer.WriteColor(vtx.Specular, ColorIOType.RGB565);
							break;
						case VertexChunkType.Intensity:
						case VertexChunkType.NormalIntensity:
							writer.WriteUShort((ushort)Math.Round(vtx.Diffuse.GetLuminance() * ushort.MaxValue));
							writer.WriteUShort((ushort)Math.Round(vtx.Specular.GetLuminance() * ushort.MaxValue));
							break;
						case VertexChunkType.Attributes:
						case VertexChunkType.UserAttributes:
						case VertexChunkType.NormalAttributes:
						case VertexChunkType.NormalUserAttributes:
						case VertexChunkType.Normal32UserAttributes:
							writer.WriteUInt(vtx.Attributes);
							break;
						case VertexChunkType.Blank:
						case VertexChunkType.BlankVec4:
						case VertexChunkType.Normal:
						case VertexChunkType.NormalVec4:
						case VertexChunkType.Normal32:
							break;
						case VertexChunkType.Null:
						case VertexChunkType.End:
						default:
							throw new InvalidOperationException(); // cant be reached
					}
				}

				remainingVerts = remainingVerts.Skip(vertCount).ToArray();
				if(!Type.CheckHasWeights())
				{
					offset += vertCount;
				}
			}
		}

		/// <summary>
		/// Writes an array of vertex chunks to an endian stack writer. Includes NULL and END chunks.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="chunks">Chunks to write.</param>
		/// <returns>The address at wich the chunks were written</returns>
		public static uint WriteArray(EndianStackWriter writer, IEnumerable<VertexChunk?> chunks)
		{
			uint result = writer.PointerPosition;

			foreach(VertexChunk? cnk in chunks)
			{
				if(cnk == null)
				{
					writer.WriteEmpty(8);
				}
				else
				{
					cnk.Write(writer);
				}
			}

			// end chunk
			writer.WriteUInt(0xFF);
			writer.WriteEmpty(4);

			return result;
		}

		/// <summary>
		/// Reads a vertex chunk off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <returns>The vertex chunk that was read.</returns>
		public static VertexChunk Read(EndianStackReader reader, ref uint address)
		{
			uint header1 = reader.ReadUInt(address);
			byte attribs = (byte)((header1 >> 8) & 0xFF);
			VertexChunkType type = (VertexChunkType)(header1 & 0xFF);

			if(!Enum.IsDefined(type) || type is VertexChunkType.End or VertexChunkType.Null)
			{
				throw new FormatException($"Vertex chunk type is invalid: {type}");
			}

			uint header2 = reader.ReadUInt(address + 4);
			ushort indexOffset = (ushort)(header2 & 0xFFFF);
			ChunkVertex[] vertices = new ChunkVertex[(ushort)(header2 >> 16)];

			address += 8;

			uint vec4 = type.CheckIsVec4() ? 4u : 0u;
			bool hasNormal = type.CheckHasNormal();
			bool normal32 = type.CheckIsNormal32();

			for(int i = 0; i < vertices.Length; i++)
			{
				ChunkVertex vtx = new(reader.ReadVector3(ref address), Color.ColorWhite, Color.ColorWhite);
				address += vec4;

				if(!hasNormal)
				{
					vtx.Normal = Vector3.UnitY;
				}
				else if(normal32)
				{
					const float componentFactor = 1f / ushort.MaxValue;

					uint composed = reader.ReadUInt(address);
					ushort x = (ushort)((composed >> 20) & 0x3FF);
					ushort y = (ushort)((composed >> 10) & 0x3FF);
					ushort z = (ushort)(composed & 0x3FF);

					vtx.Normal = new Vector3(
						(x * componentFactor) - 1f,
						(y * componentFactor) - 1f,
						(z * componentFactor) - 1f);

					address += 4;
				}
				else
				{
					vtx.Normal = reader.ReadVector3(ref address);
					address += vec4;
				}

				switch(type)
				{
					case VertexChunkType.Diffuse:
					case VertexChunkType.NormalDiffuse:
					case VertexChunkType.Normal32Diffuse:
						vtx.Diffuse = reader.ReadColor(ref address, ColorIOType.ARGB8_32);
						break;
					case VertexChunkType.DiffuseSpecular5:
					case VertexChunkType.NormalDiffuseSpecular5:
						vtx.Diffuse = reader.ReadColor(ref address, ColorIOType.RGB565);
						vtx.Specular = reader.ReadColor(ref address, ColorIOType.RGB565);
						break;
					case VertexChunkType.DiffuseSpecular4:
					case VertexChunkType.NormalDiffuseSpecular4:
						vtx.Diffuse = reader.ReadColor(ref address, ColorIOType.ARGB4);
						vtx.Specular = reader.ReadColor(ref address, ColorIOType.RGB565);
						break;
					case VertexChunkType.Intensity:
					case VertexChunkType.NormalIntensity:
						byte diffuseIntensity = (byte)(reader.ReadUShort(address) >> 16);
						byte specularIntensity = (byte)(reader.ReadUShort(address + 2) >> 16);

						vtx.Diffuse = new(diffuseIntensity, diffuseIntensity, diffuseIntensity);
						vtx.Specular = new(specularIntensity, specularIntensity, specularIntensity);
						address += 4;
						break;
					case VertexChunkType.Attributes:
					case VertexChunkType.UserAttributes:
					case VertexChunkType.NormalAttributes:
					case VertexChunkType.NormalUserAttributes:
					case VertexChunkType.Normal32UserAttributes:
						vtx.Attributes = reader.ReadUInt(address);
						address += 4;
						break;
					case VertexChunkType.Blank:
					case VertexChunkType.BlankVec4:
					case VertexChunkType.Normal:
					case VertexChunkType.NormalVec4:
					case VertexChunkType.Normal32:
						break;
					case VertexChunkType.Null:
					case VertexChunkType.End:
					default:
						throw new InvalidOperationException(); // cant be reached
				}

				vertices[i] = vtx;
			}

			return new VertexChunk(type, attribs, indexOffset, vertices);
		}

		/// <summary>
		/// Reads an array of chunk (respects NULL and END chunks).
		/// </summary>
		/// <param name="reader">The reader to read form.</param>
		/// <param name="address">Addres at which to start reading.</param>
		/// <returns>The read vertex chunks.</returns>
		public static VertexChunk?[] ReadArray(EndianStackReader reader, uint address)
		{
			List<VertexChunk?> result = new();

			VertexChunkType readType()
			{
				return (VertexChunkType)(reader.ReadUInt(address) & 0xFF);
			}

			for(VertexChunkType type = readType(); type != VertexChunkType.End; type = readType())
			{
				if(type == VertexChunkType.Null)
				{
					result.Add(null);
					address += 8;
					continue;
				}

				result.Add(Read(reader, ref address));
			}

			return result.ToArray();
		}


		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the vertex chunk.
		/// </summary>
		/// <returns></returns>
		public VertexChunk Clone()
		{
			return new VertexChunk(Type, Attributes, IndexOffset, (ChunkVertex[])Vertices.Clone());
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Type}, {WeightStatus}, {IndexOffset} : [{Vertices.Length}]";
		}
	}
}

