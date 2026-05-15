using Amicitia.IO.Binary;
using SA3D.Common.IO;
using SA3D.Modeling.Structs;
using System;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	/// <summary>
	/// Single vertex of a vertex chunk
	/// </summary>
	public struct ChunkVertex : IEquatable<ChunkVertex>
	{
		/// <summary>
		/// Default chunk vertex values.
		/// </summary>
		public static readonly ChunkVertex DefaultValues = new()
		{
			Normal = Vector3.UnitY,
			Diffuse = Color.ColorWhite,
			Specular = Color.ColorWhite,
		};

		/// <summary>
		/// Position in 3D space.
		/// </summary>
		public Vector3 Position { get; set; }

		/// <summary>
		/// Normalized direction.
		/// </summary>
		public Vector3 Normal { get; set; }

		/// <summary>
		/// Diffuse Color.
		/// </summary>
		public Color Diffuse { get; set; }

		/// <summary>
		/// Specular color.
		/// </summary>
		public Color Specular { get; set; }

		/// <summary>
		/// Additonal Attributes.
		/// </summary>
		public uint Attributes { get; set; }

		/// <summary>
		/// Vertex cache index.
		/// </summary>
		public ushort Index
		{
			readonly get => (ushort)(Attributes & 0xFFFF);
			set => Attributes = (Attributes & ~0xFFFFu) | value;
		}

		/// <summary>
		/// Node influence.
		/// </summary>
		public float Weight
		{
			readonly get => (Attributes >> 16) / 255f;
			set => Attributes = (Attributes & 0xFFFFF) | ((uint)Math.Round(value * 255f) << 16);
		}

		#region Constructors

		/// <summary>
		/// Creates new struct with <see cref="DefaultValues"/>
		/// </summary>
		public ChunkVertex()
		{
			Normal = Vector3.UnitY;
			Diffuse = Color.ColorWhite;
			Specular = Color.ColorWhite;
		}

		/// <summary>
		/// Creates a new vertex. Uses <see cref="DefaultValues"/>.
		/// <br/> Applicable for: 
		/// <br/>- <see cref="VertexChunkType.BlankVec4"/>
		/// <br/>- <see cref="VertexChunkType.Blank"/>
		/// </summary>
		/// <param name="position">Vertex position</param>
		public ChunkVertex(Vector3 position) : this()
		{
			Position = position;
		}

		/// <summary>
		/// Creates a new vertex. Uses <see cref="DefaultValues"/>.
		/// <br/> Applicable for: 
		/// <br/>- <see cref="VertexChunkType.UserAttributes"/>
		/// <br/>- <see cref="VertexChunkType.Attributes"/>
		/// </summary>
		/// <param name="position">Vertex position</param>
		/// <param name="attributes">Vertex attributes</param>
		public ChunkVertex(Vector3 position, uint attributes) : this(position)
		{
			Attributes = attributes;
		}

		/// <summary>
		/// Creates a new vertex. Uses <see cref="DefaultValues"/>.
		/// <br/> Applicable for: 
		/// <br/>- <see cref="VertexChunkType.Attributes"/>
		/// </summary>
		/// <param name="position">Vertex position</param>
		/// <param name="index">Weight vertex index</param>
		/// <param name="weight">Vertex weight</param>
		public ChunkVertex(Vector3 position, ushort index, float weight) : this(position)
		{
			Index = index;
			Weight = weight;
		}

		/// <summary>
		/// Creates a new vertex. Uses <see cref="DefaultValues"/>.
		/// <br/> Applicable for: 
		/// <br/>- <see cref="VertexChunkType.Diffuse"/>
		/// </summary>
		/// <param name="position">Vertex position</param>
		/// <param name="diffuse">Diffuse vertex color</param>
		public ChunkVertex(Vector3 position, Color diffuse) : this(position)
		{
			Diffuse = diffuse;
		}

		/// <summary>
		/// Creates a new vertex. Uses <see cref="DefaultValues"/>.
		/// <br/> Applicable for: 
		/// <br/>- <see cref="VertexChunkType.DiffuseSpecular4"/>
		/// <br/>- <see cref="VertexChunkType.DiffuseSpecular5"/>
		/// <br/>- <see cref="VertexChunkType.DiffuseSpecular"/>
		/// </summary>
		/// <param name="position">Vertex position</param>
		/// <param name="diffuse">Diffuse vertex color</param>
		/// <param name="specular">Specular vertex color</param>
		public ChunkVertex(Vector3 position, Color diffuse, Color specular) : this(position, diffuse)
		{
			Specular = specular;
		}

		/// <summary>
		/// Creates a new vertex. Uses <see cref="DefaultValues"/>.
		/// <br/>- <see cref="VertexChunkType.Intensity"/>
		/// </summary>
		/// <param name="position">Vertex position</param>
		/// <param name="diffuse">Diffuse vertex intensity</param>
		/// <param name="specular">Specular vertex intensity</param>
		public ChunkVertex(Vector3 position, float diffuse, float specular) : this(position)
		{
			Diffuse = new(diffuse, diffuse, diffuse);
			Specular = new(specular, specular, specular);
		}

		/// <summary>
		/// Creates a new vertex. Uses <see cref="DefaultValues"/>.
		/// <br/> Applicable for: 
		/// <br/>- <see cref="VertexChunkType.NormalVec4"/>
		/// <br/>- <see cref="VertexChunkType.Normal"/>
		/// <br/>- <see cref="VertexChunkType.Normal32"/>
		/// </summary>
		/// <param name="position">Vertex position</param>
		/// <param name="normal">Vertex normal direction</param>
		public ChunkVertex(Vector3 position, Vector3 normal) : this(position)
		{
			Normal = normal;
		}

		/// <summary>
		/// Creates a new vertex. Uses <see cref="DefaultValues"/>.
		/// <br/> Applicable for: 
		/// <br/>- <see cref="VertexChunkType.NormalUserAttributes"/>
		/// <br/>- <see cref="VertexChunkType.NormalAttributes"/>
		/// </summary>
		/// <param name="position">Vertex position</param>
		/// <param name="normal">Vertex normal direction</param>
		/// <param name="attributes">Vertex attributes</param>
		public ChunkVertex(Vector3 position, Vector3 normal, uint attributes) : this(position, attributes)
		{
			Normal = normal;
		}

		/// <summary>
		/// Creates a new vertex. Uses <see cref="DefaultValues"/>.
		/// <br/> Applicable for: 
		/// <br/>- <see cref="VertexChunkType.NormalAttributes"/>
		/// </summary>
		/// <param name="position">Vertex position</param>
		/// <param name="normal">Vertex normal direction</param>
		/// <param name="index">Weight vertex index</param>
		/// <param name="weight">Vertex weight</param>
		public ChunkVertex(Vector3 position, Vector3 normal, ushort index, float weight) : this(position, index, weight)
		{
			Normal = normal;
		}

		/// <summary>
		/// Creates a new vertex. Uses <see cref="DefaultValues"/>.
		/// <br/> Applicable for: 
		/// <br/>- <see cref="VertexChunkType.NormalDiffuse"/>
		/// </summary>
		/// <param name="position">Vertex position</param>
		/// <param name="normal">Vertex normal direction</param>
		/// <param name="diffuse">Diffuse vertex color</param>
		public ChunkVertex(Vector3 position, Vector3 normal, Color diffuse) : this(position, diffuse)
		{
			Normal = normal;
		}

		/// <summary>
		/// Creates a new vertex
		/// <br/> Applicable for: 
		/// <br/>- <see cref="VertexChunkType.NormalDiffuseSpecular4"/>
		/// <br/>- <see cref="VertexChunkType.NormalDiffuseSpecular5"/>
		/// </summary>
		/// <param name="position">Vertex position</param>
		/// <param name="normal">Vertex normal direction</param>
		/// <param name="diffuse">Diffuse vertex color</param>
		/// <param name="specular">Specular vertex color</param>
		public ChunkVertex(Vector3 position, Vector3 normal, Color diffuse, Color specular) : this(position, diffuse, specular)
		{
			Normal = normal;
		}

		/// <summary>
		/// Creates a new vertex. Uses <see cref="DefaultValues"/>.
		/// <br/>- <see cref="VertexChunkType.NormalIntensity"/>
		/// </summary>
		/// <param name="position">Vertex position</param>
		/// <param name="normal">Vertex normal direction</param>
		/// <param name="diffuse">Diffuse vertex intensity</param>
		/// <param name="specular">Specular vertex intensity</param>
		public ChunkVertex(Vector3 position, Vector3 normal, float diffuse, float specular) : this(position, diffuse, specular)
		{
			Normal = normal;
		}

		/// <summary>
		/// Creates a new vertex. Uses <see cref="DefaultValues"/>.
		/// <br/> Applicable for: 
		/// <br/>- <see cref="VertexChunkType.AttributesDiffuse"/>
		/// </summary>
		/// <param name="position">Vertex position</param>
		/// <param name="attributes">Vertex attributes</param>
		/// <param name="diffuse">Diffuse vertex color</param>
		public ChunkVertex(Vector3 position, uint attributes, Color diffuse) : this(position, attributes)
		{
			Diffuse = diffuse;
		}

		#endregion

		/// <summary>
		/// Returns the appropriate <see cref="ChunkVertex"/> read callback for the given <see cref="VertexChunkType"/>
		/// </summary>
		/// <param name="type">The type to get the read callback for</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static Func<BinaryObjectReader, ChunkVertex> GetReadCallback(VertexChunkType type)
		{
			return type switch
			{
				VertexChunkType.BlankVec4 => r =>
				{
					Vector3 position = r.ReadVector3();
					r.Skip(4); // always 1.0
					return new(position);
				}
				,

				VertexChunkType.NormalVec4 => r =>
				{
					Vector3 position = r.ReadVector3();
					r.Skip(sizeof(float)); // always 1.0
					Vector3 normal = r.ReadVector3();
					r.Skip(sizeof(float)); // always 0.0
					return new(position, normal);
				}
				,

				VertexChunkType.Blank => r => new(
					r.ReadVector3()
				),

				VertexChunkType.Diffuse => r => new(
					r.ReadVector3(),
					r.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_32)
				),

				VertexChunkType.UserAttributes or VertexChunkType.Attributes => r => new(
					r.ReadVector3(),
					r.ReadUInt32()
				),

				VertexChunkType.DiffuseSpecular5 => r => new(
					r.ReadVector3(),
					r.ReadObject<Color, ColorIOType>(ColorIOType.RGB565),
					r.ReadObject<Color, ColorIOType>(ColorIOType.RGB565)
				),

				VertexChunkType.DiffuseSpecular4 => r => new(
						r.ReadVector3(),
						r.ReadObject<Color, ColorIOType>(ColorIOType.ARGB4),
						r.ReadObject<Color, ColorIOType>(ColorIOType.RGB565)
					),
				VertexChunkType.Intensity => r => new(
					r.ReadVector3(),
					r.ReadUInt16() / ((float)ushort.MaxValue),
					r.ReadUInt16() / ((float)ushort.MaxValue)
				),

				VertexChunkType.Normal => r => new(
					r.ReadVector3(),
					r.ReadVector3()
				),

				VertexChunkType.NormalDiffuse => r => new(
					r.ReadVector3(),
					r.ReadVector3(),
					r.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_32)
				),

				VertexChunkType.NormalUserAttributes or VertexChunkType.NormalAttributes => r => new(
					r.ReadVector3(),
					r.ReadVector3(),
					r.ReadUInt32()
				),

				VertexChunkType.NormalDiffuseSpecular5 => r => new(
					r.ReadVector3(),
					r.ReadVector3(),
					r.ReadObject<Color, ColorIOType>(ColorIOType.RGB565),
					r.ReadObject<Color, ColorIOType>(ColorIOType.RGB565)
				),

				VertexChunkType.NormalDiffuseSpecular4 => r => new(
					r.ReadVector3(),
					r.ReadVector3(),
					r.ReadObject<Color, ColorIOType>(ColorIOType.ARGB4),
					r.ReadObject<Color, ColorIOType>(ColorIOType.RGB565)
				),

				VertexChunkType.NormalIntensity => r => new(
					r.ReadVector3(),
					r.ReadVector3(),
					r.ReadUInt16() / ((float)ushort.MaxValue),
					r.ReadUInt16() / ((float)ushort.MaxValue)
				),

				VertexChunkType.Normal32 => r => new(
					r.ReadVector3(),
					DecompressNormal(r.ReadUInt32())
				),

				VertexChunkType.Normal32Diffuse => r => new(
					r.ReadVector3(),
					DecompressNormal(r.ReadUInt32()),
					r.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_32)
				),

				VertexChunkType.Normal32UserAttributes => r => new(
					r.ReadVector3(),
					DecompressNormal(r.ReadUInt32()),
					r.ReadUInt32()
				),

				VertexChunkType.DiffuseSpecular => r => new(
					r.ReadVector3(),
					r.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_32),
					r.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_32)
				),

				VertexChunkType.AttributesDiffuse => r => new(
					r.ReadVector3(),
					r.ReadUInt32(),
					r.ReadObject<Color, ColorIOType>(ColorIOType.ARGB8_32)
				),

				VertexChunkType.Null
				or VertexChunkType.End
				or _ => throw new ArgumentException($"Invalid vertex chunk type {type}"),
			};
		}

		/// <summary>
		/// Return the appropriate <see cref="ChunkVertex"/> write callback for the given <see cref="VertexChunkType"/>
		/// </summary>
		/// <param name="type">The type to get the write callback for</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static Action<BinaryObjectWriter, ChunkVertex> GetWriteCallback(VertexChunkType type)
		{
			return type switch
			{
				VertexChunkType.BlankVec4 => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteSingle(1);
				}
				,

				VertexChunkType.NormalVec4 => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteSingle(1);
					w.WriteVector3(v.Normal);
					w.WriteSingle(0);
				}
				,

				VertexChunkType.Blank => (w, v) => w.WriteVector3(v.Position),

				VertexChunkType.Diffuse => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteObject(v.Diffuse, ColorIOType.ARGB8_32);
				}
				,

				VertexChunkType.UserAttributes or VertexChunkType.Attributes => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteUInt32(v.Attributes);
				}
				,

				VertexChunkType.DiffuseSpecular5 => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteObject(v.Diffuse, ColorIOType.RGB565);
					w.WriteObject(v.Specular, ColorIOType.RGB565);
				}
				,

				VertexChunkType.DiffuseSpecular4 => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteObject(v.Diffuse, ColorIOType.ARGB4);
					w.WriteObject(v.Specular, ColorIOType.RGB565);
				}
				,
				VertexChunkType.Intensity => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteUInt16((ushort)(float.Clamp(v.Diffuse.GetLuminance(), 0, 1) * ushort.MaxValue));
					w.WriteUInt16((ushort)(float.Clamp(v.Specular.GetLuminance(), 0, 1) * ushort.MaxValue));
				}
				,

				VertexChunkType.Normal => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteVector3(v.Normal);
				}
				,

				VertexChunkType.NormalDiffuse => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteVector3(v.Normal);
					w.WriteObject(v.Diffuse, ColorIOType.ARGB8_32);
				}
				,

				VertexChunkType.NormalUserAttributes or VertexChunkType.NormalAttributes => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteVector3(v.Normal);
					w.WriteUInt32(v.Attributes);
				}
				,

				VertexChunkType.NormalDiffuseSpecular5 => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteVector3(v.Normal);
					w.WriteObject(v.Diffuse, ColorIOType.RGB565);
					w.WriteObject(v.Specular, ColorIOType.RGB565);
				}
				,

				VertexChunkType.NormalDiffuseSpecular4 => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteVector3(v.Normal);
					w.WriteObject(v.Diffuse, ColorIOType.ARGB4);
					w.WriteObject(v.Specular, ColorIOType.RGB565);
				}
				,

				VertexChunkType.NormalIntensity => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteVector3(v.Normal);
					w.WriteUInt16((ushort)(float.Clamp(v.Diffuse.GetLuminance(), 0, 1) * ushort.MaxValue));
					w.WriteUInt16((ushort)(float.Clamp(v.Specular.GetLuminance(), 0, 1) * ushort.MaxValue));
				}
				,

				VertexChunkType.Normal32 => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteUInt32(CompressNormal(v.Normal));
				}
				,

				VertexChunkType.Normal32Diffuse => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteUInt32(CompressNormal(v.Normal));
					w.WriteObject(v.Diffuse, ColorIOType.ARGB8_32);
				}
				,

				VertexChunkType.Normal32UserAttributes => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteUInt32(CompressNormal(v.Normal));
					w.WriteUInt32(v.Attributes);
				}
				,

				VertexChunkType.DiffuseSpecular => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteObject(v.Diffuse, ColorIOType.ARGB8_32);
					w.WriteObject(v.Specular, ColorIOType.ARGB8_32);
				}
				,

				VertexChunkType.AttributesDiffuse => (w, v) =>
				{
					w.WriteVector3(v.Position);
					w.WriteUInt32(v.Attributes);
					w.WriteObject(v.Diffuse, ColorIOType.ARGB8_32);
				}
				,

				VertexChunkType.Null
				or VertexChunkType.End
				or _ => throw new ArgumentException($"Invalid vertex chunk type {type}")
			};
		}


		private static Vector3 DecompressNormal(uint value)
		{
			return new(
				DecompressNormalComponent((value >> 20) & 0x3FFu),
				DecompressNormalComponent((value >> 10) & 0x3FFu),
				DecompressNormalComponent(value & 0x3FFu)
			);
		}

		private static float DecompressNormalComponent(uint value)
		{
			uint number = ((value & 0x200) << 22) | (value & 0x1FF);
			int signed = unchecked((int)number);
			return signed / 512f;
		}

		private static uint CompressNormal(Vector3 normal)
		{
			uint x = CompressNormalComponent(normal.X);
			uint y = CompressNormalComponent(normal.Y);
			uint z = CompressNormalComponent(normal.Z);

			return (x << 20) | (y << 10) | z;
		}

		private static uint CompressNormalComponent(float value)
		{
			int number = (int)(float.Clamp(value, -1, 1) * 512);
			uint result = unchecked((uint)number);
			return (result >> 22) | (result & 0x1FF);
		}


		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is ChunkVertex vertex &&
				   Position.Equals(vertex.Position) &&
				   Normal.Equals(vertex.Normal) &&
				   Diffuse.Equals(vertex.Diffuse) &&
				   Specular.Equals(vertex.Specular) &&
				   Attributes == vertex.Attributes;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(Position, Normal, Diffuse, Specular, Attributes);
		}

		/// <inheritdoc/>
		readonly bool IEquatable<ChunkVertex>.Equals(ChunkVertex other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two chunk vertices for equality.
		/// </summary>
		/// <param name="left">Lefthand vertex.</param>
		/// <param name="right">Rigthand vertex.</param>
		/// <returns>Whether the vertices are equal.</returns>
		public static bool operator ==(ChunkVertex left, ChunkVertex right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two chunk vertices for inequality.
		/// </summary>
		/// <param name="left">Lefthand vertex.</param>
		/// <param name="right">Rigthand vertex.</param>
		/// <returns>Whether the vertices are inequal.</returns>
		public static bool operator !=(ChunkVertex left, ChunkVertex right)
		{
			return !(left == right);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"{Position.DebugString()}, {Normal.DebugString()} : {Index}, {Weight:F3}";
		}

	}
}