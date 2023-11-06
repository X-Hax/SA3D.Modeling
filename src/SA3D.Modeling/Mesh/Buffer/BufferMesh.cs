using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Modeling.Strippify;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Buffer
{
	/// <summary>
	/// Data set for a renderable mesh <br/>
	/// Can also consist of only vertices.
	/// </summary>
	public class BufferMesh : ICloneable
	{
		/// <summary>
		/// Default normal direction for buffer mesh vertices.
		/// </summary>
		public static readonly Vector3 DefaultNormal = Vector3.UnitY;

		/// <summary>
		/// Default color for buffer mesh corners.
		/// </summary>
		public static readonly Color DefaultColor = Color.ColorWhite;


		/// <summary>
		/// Mesh vertices.
		/// </summary>
		public BufferVertex[]? Vertices { get; }

		/// <summary>
		/// Polygon rendering information.
		/// </summary>
		public BufferMaterial Material { get; private set; }

		/// <summary>
		/// Polygon corners.
		/// </summary>
		public BufferCorner[]? Corners { get; private set; }

		/// <summary>
		/// Index list combining polygon corners into triangles.
		/// <br/> If null, use the corners in order.
		/// </summary>
		public uint[]? IndexList { get; private set; }

		/// <summary>
		/// When set, <see cref="IndexList"/> / <see cref="Corners"/> is made up of one big triangle strip, instead of individual triangles.
		/// </summary>
		public bool Strippified { get; private set; }

		/// <summary>
		/// If true, the vertices will be added onto the existing buffered vertices.
		/// </summary>
		public bool ContinueWeight { get; }

		/// <summary>
		/// Whether the model uses vertex normals.
		/// </summary>
		public bool HasNormals { get; }

		/// <summary>
		/// Whether the model uses polygon colors.
		/// </summary>
		public bool HasColors { get; private set; }

		/// <summary>
		/// Index offset for when writing vertices into the buffer array.
		/// </summary>
		public ushort VertexWriteOffset { get; internal set; }

		/// <summary>
		/// Index offset for when reading vertices from the buffer array for rendering.
		/// </summary>
		public ushort VertexReadOffset { get; internal set; }


		/// <summary>
		/// Creates a new buffer mesh.
		/// </summary>
		/// <param name="vertices">Buffer vertices.</param>
		/// <param name="material">Polygon rendering information.</param>
		/// <param name="corners">Polygon corners.</param>
		/// <param name="indexList">Index list combining polygon corners into triangles</param>
		/// <param name="strippified">When set, <paramref name="indexList"/> is made up of one big triangle strip, instead of individual triangles.</param>
		/// <param name="continueWeight">If true, the vertices will be added onto the existing buffered vertices.</param>
		/// <param name="hasNormals">Whether the model uses vertex normals.</param>
		/// <param name="hasColors">Whether the model uses polygon colors.</param>
		/// <param name="vertexWriteOffset">Index offset for when writing vertices into the buffer array.</param>
		/// <param name="vertexReadOffset">Index offset for when reading vertices from the buffer array for rendering.</param>
		/// <exception cref="ArgumentException"/>
		public BufferMesh(
			BufferVertex[]? vertices,
			BufferMaterial material,
			BufferCorner[]? corners,
			uint[]? indexList,
			bool strippified,
			bool continueWeight,
			bool hasNormals,
			bool hasColors,
			ushort vertexWriteOffset,
			ushort vertexReadOffset)
		{
			Vertices = vertices;
			Material = material;
			Corners = corners;
			IndexList = indexList;
			Strippified = strippified;
			ContinueWeight = continueWeight;
			HasNormals = hasNormals;
			HasColors = hasColors;
			VertexWriteOffset = vertexWriteOffset;
			VertexReadOffset = vertexReadOffset;

			VerifyVertexData();
			VerifyPolygonData();
		}

		/// <summary>
		/// Creates a new buffer mesh from only vertex data.
		/// </summary>
		/// <param name="vertices">Buffer vertices.</param>
		/// <param name="continueWeight">If true, the vertices will be added onto the existing buffered vertices.</param>
		/// <param name="hasNormals">Whether the model uses vertex normals.</param>
		/// <param name="vertexWriteOffset">Index offset for when writing vertices into the buffer array.</param>
		/// <exception cref="ArgumentException"/>
		public BufferMesh(BufferVertex[] vertices, bool continueWeight, bool hasNormals, ushort vertexWriteOffset)
		{
			Vertices = vertices;
			ContinueWeight = continueWeight;
			VertexWriteOffset = vertexWriteOffset;
			HasNormals = hasNormals;

			VerifyVertexData();
		}

		/// <summary>
		/// Creates a new buffer mesh with only polygon data.
		/// </summary>
		/// <param name="material">Polygon rendering information.</param>
		/// <param name="corners">Polygon corners.</param>
		/// <param name="indexList">Index list combining polygon corners into triangles</param>
		/// <param name="strippified">When set, <paramref name="indexList"/> is made up of one big triangle strip, instead of individual triangles.</param>
		/// <param name="hasColors">Whether the model uses polygon colors.</param>
		/// <param name="vertexReadOffset">Index offset for when reading vertices from the buffer array for rendering.</param>
		/// <exception cref="ArgumentException"/>
		public BufferMesh(BufferMaterial material, BufferCorner[] corners, uint[]? indexList, bool strippified, bool hasColors, ushort vertexReadOffset)
		{
			Material = material;
			Corners = corners;
			IndexList = indexList;
			Strippified = strippified;
			HasColors = hasColors;
			VertexReadOffset = vertexReadOffset;

			VerifyPolygonData();
		}


		private void VerifyVertexData()
		{
			if(Vertices == null || Vertices.Length == 0)
			{
				throw new ArgumentException("Vertices can't be empty", "vertices");
			}
		}

		private void VerifyPolygonData()
		{
			if(Corners == null || Corners.Length == 0)
			{
				throw new ArgumentException("Corners can't be empty", "corners");
			}

			if(IndexList != null && IndexList.Length == 0)
			{
				throw new ArgumentException("Triangle list cant be empty", "triangleList");
			}
		}


		/// <summary>
		/// Compiles the triangle list of indices to <see cref="Corners"/>.
		/// </summary>
		/// <returns>The index triangle list.</returns>
		/// <exception cref="InvalidOperationException"></exception>
		public uint[] GetIndexTriangleList()
		{
			if(Corners == null)
			{
				throw new InvalidOperationException("The mesh contains no polygon information.");
			}

			uint[] result;

			if(IndexList == null)
			{
				if(Strippified)
				{
					List<uint> triangles = new();

					bool rev = false;

					for(uint i = 2; i < Corners.Length; i++, rev = !rev)
					{
						uint i1 = i - 2;
						uint i2 = i - 1;
						uint i3 = i;

						BufferCorner c1 = Corners[i1];
						BufferCorner c2 = Corners[i2];
						BufferCorner c3 = Corners[i3];

						if(c1.VertexIndex == c2.VertexIndex
						   || c2.VertexIndex == c3.VertexIndex
						   || c3.VertexIndex == c1.VertexIndex)
						{
							continue;
						}

						if(rev)
						{
							triangles.Add(i2);
							triangles.Add(i1);
							triangles.Add(i3);
						}
						else
						{
							triangles.Add(i1);
							triangles.Add(i2);
							triangles.Add(i3);
						}
					}

					result = triangles.ToArray();
				}
				else
				{
					result = new uint[Corners.Length];
					for(uint i = 0; i < result.Length; i++)
					{
						result[i] = i;
					}
				}
			}
			else
			{
				if(Strippified)
				{
					List<uint> triangles = new();
					bool rev = false;

					for(int i = 2; i < IndexList.Length; i++, rev = !rev)
					{
						uint i1 = IndexList[i - 2];
						uint i2 = IndexList[i - 1];
						uint i3 = IndexList[i];

						if(i1 == i2 || i2 == i3 || i3 == i1)
						{
							continue;
						}

						if(rev)
						{
							triangles.Add(i2);
							triangles.Add(i1);
							triangles.Add(i3);
						}
						else
						{
							triangles.Add(i1);
							triangles.Add(i2);
							triangles.Add(i3);
						}
					}

					result = triangles.ToArray();

				}
				else
				{
					// fast copy
					result = IndexList.ToArray();
				}

			}

			return result;
		}

		/// <summary>
		/// Compiles the triangle list of <see cref="Corners"/>.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public BufferCorner[] GetCornerTriangleList()
		{
			if(Corners == null)
			{
				throw new InvalidOperationException("The mesh contains no polygon information.");
			}

			BufferCorner[] result;

			if(IndexList == null)
			{
				if(Strippified)
				{
					List<BufferCorner> triangles = new();
					bool rev = false;

					for(uint i = 2; i < Corners.Length; i++, rev = !rev)
					{
						BufferCorner c1 = Corners[i - 2];
						BufferCorner c2 = Corners[i - 1];
						BufferCorner c3 = Corners[i];

						if(c1.VertexIndex == c2.VertexIndex
						   || c2.VertexIndex == c3.VertexIndex
						   || c3.VertexIndex == c1.VertexIndex)
						{
							continue;
						}

						if(rev)
						{
							triangles.Add(c2);
							triangles.Add(c1);
							triangles.Add(c3);
						}
						else
						{
							triangles.Add(c1);
							triangles.Add(c2);
							triangles.Add(c3);
						}
					}

					result = triangles.ToArray();
				}
				else
				{
					// fast copy
					result = Corners.ToArray();
				}
			}
			else
			{
				if(Strippified)
				{
					List<BufferCorner> triangles = new();
					bool rev = false;

					for(uint i = 2; i < IndexList.Length; i++, rev = !rev)
					{
						uint i1 = IndexList[i - 2];
						uint i2 = IndexList[i - 1];
						uint i3 = IndexList[i];

						if(i1 == i2 || i2 == i3 || i3 == i1)
						{
							continue;
						}

						if(rev)
						{
							triangles.Add(Corners[i2]);
							triangles.Add(Corners[i1]);
							triangles.Add(Corners[i3]);
						}
						else
						{
							triangles.Add(Corners[i1]);
							triangles.Add(Corners[i2]);
							triangles.Add(Corners[i3]);
						}
					}

					result = triangles.ToArray();
				}
				else
				{
					result = new BufferCorner[IndexList.Length];
					for(int i = 0; i < result.Length; i++)
					{
						result[i] = Corners[IndexList[i]];
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Attempts to optimize the polygons by strippifying and/or generating index lists.
		/// </summary>
		public void OptimizePolygons()
		{
			if(Corners == null)
			{
				return;
			}

			BufferCorner[] corners = GetCornerTriangleList();

			// filter degenerate triangles
			int newArraySize = corners.Length;
			for(int i = 0; i < newArraySize; i += 3)
			{
				ushort index1 = corners[i].VertexIndex;
				ushort index2 = corners[i + 1].VertexIndex;
				ushort index3 = corners[i + 2].VertexIndex;

				if(index1 == index2 || index2 == index3 || index3 == index1)
				{
					corners[i] = corners[newArraySize - 3];
					corners[i + 1] = corners[newArraySize - 2];
					corners[i + 2] = corners[newArraySize - 1];
					i -= 3;
					newArraySize -= 3;
				}
			}

			if(newArraySize == 0)
			{
				Corners = null;
				IndexList = null;
				Material = default;
				HasColors = false;
				VertexReadOffset = 0;
				return;
			}

			Array.Resize(ref corners, newArraySize);

			if(!corners.TryCreateDistinctMap(out DistinctMap<BufferCorner> distinctMap))
			{
				Corners = corners;
				return;
			}

			int[][] strips = TriangleStrippifier.Global.Strippify(distinctMap.Map!);

			uint stripLength = (uint)TriangleStrippifier.JoinedStripEnumerator(strips, null).Count();
			uint structSize = HasColors ? BufferCorner.StructSize : BufferCorner.StructSizeNoColor;

			uint sizeTriList = structSize * (uint)corners.Length;
			uint sizeTriListIndexed = (uint)((structSize * distinctMap.Values.Count) + (corners.Length * 2));
			uint sizeStrips = structSize * stripLength;
			uint sizeStripsIndexed = (uint)((structSize * distinctMap.Values.Count) + (stripLength * 2));

			uint smallestSize = uint.Min(
				uint.Min(sizeTriList, sizeTriListIndexed),
				uint.Min(sizeStrips, sizeStripsIndexed));

			if(sizeStripsIndexed == smallestSize)
			{
				Corners = distinctMap.ValueArray;
				IndexList = (uint[])(object)TriangleStrippifier.JoinStrips(strips, null);
				Strippified = true;
			}
			else if(sizeStrips == smallestSize)
			{
				Corners = TriangleStrippifier.JoinedStripEnumerator(strips, null)
					.Select(x => distinctMap.Values[x]).ToArray();
				IndexList = null;
				Strippified = true;
			}
			else if(sizeTriListIndexed == smallestSize)
			{
				Corners = distinctMap.ValueArray;
				IndexList = (uint[])(object)distinctMap.Map!;
				Strippified = false;
			}
			else
			{
				Corners = corners;
				IndexList = null;
				Strippified = false;
			}


		}

		/// <summary>
		/// Optimizes a collection of buffer meshes by optimizing the polygons and combining vertex and poly data between meshes.
		/// <br/> Reuses arrays and buffermeshes.
		/// </summary>
		/// <param name="input">The collection to optimize</param>
		/// <returns>The optimized buffermeshes.</returns>
		public static BufferMesh[] Optimize(IList<BufferMesh> input)
		{
			List<BufferMesh> result = new();

			foreach(BufferMesh mesh in input)
			{
				mesh.OptimizePolygons();

				if(mesh.Vertices != null)
				{
					result.Add(mesh);
					continue;
				}

				if(result.Count > 0 && result[^1].Corners == null)
				{
					BufferMesh prev = result[^1];
					prev.Corners = mesh.Corners!;
					prev.IndexList = mesh.IndexList;
					prev.Material = mesh.Material;
					prev.HasColors = mesh.HasColors;
					prev.VertexReadOffset = mesh.VertexReadOffset;
				}
				else
				{
					result.Add(mesh);
				}
			}

			return result.ToArray();
		}


		/// <summary>
		/// Writes the buffer mesh to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		public uint Write(EndianStackWriter writer)
		{
			uint vtxAddr = 0;
			if(Vertices != null)
			{
				vtxAddr = writer.PointerPosition;
				foreach(BufferVertex vtx in Vertices)
				{
					vtx.Write(writer, HasNormals);
				}
			}

			uint cornerAddr = 0;
			if(Corners != null)
			{
				cornerAddr = writer.PointerPosition;
				foreach(BufferCorner c in Corners)
				{
					c.Write(writer, HasColors);
				}
			}

			uint triangleAddr = 0;
			if(IndexList != null)
			{
				triangleAddr = writer.PointerPosition;
				foreach(uint t in IndexList)
				{
					writer.WriteUInt(t);
				}
			}

			uint address = writer.PointerPosition;

			ushort flags = 0;
			if(ContinueWeight)
			{
				flags |= 1;
			}

			if(Strippified)
			{
				flags |= 2;
			}

			if(HasNormals)
			{
				flags |= 4;
			}

			if(HasColors)
			{
				flags |= 8;
			}


			writer.WriteUShort(VertexReadOffset);
			writer.WriteUShort(VertexWriteOffset);
			writer.WriteUShort(flags);

			writer.WriteUShort((ushort)(Vertices?.Length ?? 0));
			writer.WriteUInt(vtxAddr);

			writer.WriteUInt((uint)(Corners?.Length ?? 0));
			writer.WriteUInt(cornerAddr);

			writer.WriteUInt((uint)(IndexList?.Length ?? 0));
			writer.WriteUInt(triangleAddr);

			Material.Write(writer);

			return address;
		}

		/// <summary>
		/// Reads a buffer mesh from a byte array
		/// </summary>
		/// <param name="reader">Byte source</param>
		/// <param name="address">Address at which the buffermesh is located</param>
		public static BufferMesh Read(EndianStackReader reader, uint address)
		{
			ushort vertexReadOffset = reader.ReadUShort(address + 0);
			ushort vertexWriteOffset = reader.ReadUShort(address + 2);

			ushort flags = reader.ReadUShort(address + 4);
			bool continueWeight = (flags & 1) != 0;
			bool strippified = (flags & 2) != 0;
			bool hasNormals = (flags & 4) != 0;
			bool hasColors = (flags & 8) != 0;


			BufferVertex[]? vertices = null;
			if(reader.TryReadPointer(address + 8, out uint vtxAddr))
			{
				vertices = new BufferVertex[reader.ReadUShort(address + 6)];
				for(int i = 0; i < vertices.Length; i++)
				{
					vertices[i] = BufferVertex.Read(reader, ref vtxAddr, hasNormals);
				}
			}

			BufferCorner[]? corners = null;
			if(reader.TryReadPointer(address + 0x10, out uint cornerAddr))
			{
				corners = new BufferCorner[reader.ReadUInt(address + 0xC)];
				for(int i = 0; i < corners.Length; i++)
				{
					corners[i] = BufferCorner.Read(reader, ref cornerAddr, hasColors);
				}
			}

			uint[]? triangles = null;
			if(reader.TryReadPointer(address + 0x18, out uint triangleAddr))
			{
				triangles = new uint[reader.ReadUInt(address + 0x14)];
				for(int i = 0; i < triangles.Length; i++)
				{
					triangles[i] = reader.ReadUInt(triangleAddr);
					triangleAddr += 4;
				}
			}

			BufferMaterial material = BufferMaterial.Read(reader, address + 0x1C);

			return new(
				vertices,
				material,
				corners,
				triangles,
				strippified,
				continueWeight,
				hasNormals,
				hasColors,
				vertexWriteOffset,
				vertexReadOffset
				);
		}


		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the mesh.
		/// </summary>
		/// <returns>The clone of the mesh.</returns>
		public BufferMesh Clone()
		{
			return new(
				Vertices?.ToArray(),
				Material,
				Corners?.ToArray(),
				IndexList?.ToArray(),
				Strippified,
				ContinueWeight,
				HasNormals,
				HasColors,
				VertexWriteOffset,
				VertexReadOffset
			);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Vertices?.Length} - {Corners?.Length} - {IndexList?.Length}";
		}
	}
}
