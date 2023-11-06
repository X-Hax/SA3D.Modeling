using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Modeling.Mesh.Gamecube.Enums;
using SA3D.Modeling.Mesh.Gamecube.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Modeling.Mesh.Gamecube
{
	/// <summary>
	/// A single mesh, with its own parameter and primitive data <br/>
	/// </summary>
	public class GCMesh : ICloneable
	{
		/// <summary>
		/// The data parameters.
		/// </summary>
		public IGCParameter[] Parameters { get; set; }

		/// <summary>
		/// The polygon data.
		/// </summary>
		public GCPolygon[] Polygons { get; set; }

		/// <summary>
		/// The index attributes of this mesh. If it has no <see cref="GCIndexFormatParameter"/>, it will return null.
		/// </summary>
		public GCIndexFormat? IndexFormat
		{
			get
			{
				if(Parameters.FirstOrDefault(x => x.Type == GCParameterType.IndexFormat) is GCIndexFormatParameter p)
				{
					return p.IndexFormat;
				}

				return null;
			}
		}


		/// <summary>
		/// Create a new mesh from existing polygons and parameters
		/// </summary>
		/// <param name="parameters">Polygon data.</param>
		/// <param name="polygons">Data parameters.</param>
		public GCMesh(IGCParameter[] parameters, GCPolygon[] polygons)
		{
			Parameters = parameters;
			Polygons = polygons;
		}


		/// <summary>
		/// Optimizes the polygon data in the mesh by strippifying.
		/// </summary>
		public void OptimizePolygons()
		{
			// getting the current triangles
			List<GCCorner> triangles = new();
			foreach(GCPolygon p in Polygons)
			{
				if(p.Type == GCPolyType.Triangles)
				{
					triangles.AddRange(p.Corners);
				}
				else if(p.Type == GCPolyType.TriangleStrip)
				{
					bool rev = p.Corners[0].PositionIndex == p.Corners[1].PositionIndex;
					for(int i = rev ? 3 : 2; i < p.Corners.Length; i++)
					{
						if(rev)
						{
							triangles.Add(p.Corners[i - 1]);
							triangles.Add(p.Corners[i - 2]);
						}
						else
						{
							triangles.Add(p.Corners[i - 2]);
							triangles.Add(p.Corners[i - 1]);
						}

						triangles.Add(p.Corners[i]);
						rev = !rev;
					}
				}
			}

			GCCorner[][] strips = Strippify.TriangleStrippifier.Global.Strippify(triangles.ToArray());

			// putting them all together
			List<GCPolygon> polygons = new();
			List<GCCorner> singleTris = new();

			for(int i = 0; i < strips.Length; i++)
			{
				GCCorner[] strip = strips[i];
				if(strip.Length == 3)
				{
					singleTris.AddRange(strip);
				}
				else
				{
					polygons.Add(new(GCPolyType.TriangleStrip, strip));
				}
			}

			if(singleTris.Count > 0)
			{
				polygons.Add(new(GCPolyType.Triangles, singleTris.ToArray()));
			}

			Polygons = polygons.ToArray();
		}


		/// <summary>
		/// Writes the parameters to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <returns>The address at which the parameters were written.</returns>
		public uint WriteParameters(EndianStackWriter writer)
		{
			uint result = writer.PointerPosition;
			foreach(IGCParameter p in Parameters)
			{
				p.Write(writer);
			}

			return result;
		}

		/// <summary>
		/// Writes the polygons to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="indexFormat">Index format reference to use for writing polygon indices. If the mesh contains their own index format, it will be replaced.</param>
		/// <returns>The address at which the polygons were written.</returns>
		public uint WritePolygons(EndianStackWriter writer, ref GCIndexFormat indexFormat)
		{
			GCIndexFormat? t = IndexFormat;
			if(t.HasValue)
			{
				indexFormat = t.Value;
			}

			uint result = writer.PointerPosition;

			foreach(GCPolygon p in Polygons)
			{
				p.Write(writer, indexFormat);
			}

			writer.Align(0x20);

			return result;
		}

		/// <summary>
		/// Writes the parameters and polygons of a mesh array to an endian stack writer and returns the header array data.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="meshes">The meshes to write.</param>
		/// <returns></returns>
		public static byte[] WriteArrayContents(EndianStackWriter writer, GCMesh[] meshes)
		{
			uint[] headerData = new uint[meshes.Length * 4];

			for(int i = 0, h = 0; i < meshes.Length; i++, h += 4)
			{
				headerData[h] = meshes[i].WriteParameters(writer);
				headerData[h + 1] = (uint)meshes[i].Parameters.Length;
			}

			writer.Align(0x20);

			GCIndexFormat indexFormat = GCIndexFormat.HasPosition | GCIndexFormat.PositionLargeIndex;

			for(int i = 0, h = 2; i < meshes.Length; i++, h += 4)
			{
				uint polyAddress = meshes[i].WritePolygons(writer, ref indexFormat);
				headerData[h] = polyAddress;
				headerData[h + 1] = writer.PointerPosition - polyAddress;
			}

			byte[] result = new byte[headerData.Length * sizeof(uint)];
			System.Buffer.BlockCopy(headerData, 0, result, 0, result.Length);

			return result;
		}

		/// <summary>
		/// Reads gamecube mesh data off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="indexFormat">Index format to use.</param>
		/// <returns>The read GC mesh.</returns>
		public static GCMesh Read(EndianStackReader reader, uint address, ref GCIndexFormat indexFormat)
		{
			uint parameters_addr = reader.ReadPointer(address);
			int parameters_count = reader.ReadInt(address + 4);

			uint primitives_addr = reader.ReadPointer(address + 8);
			int primitives_size = reader.ReadInt(address + 12);

			List<IGCParameter> parameters = new();
			for(int i = 0; i < parameters_count; i++)
			{
				parameters.Add(IGCParameter.Read(reader, parameters_addr));
				parameters_addr += 8;
			}

			if(parameters.FirstOrDefault(x => x.Type == GCParameterType.IndexFormat) is GCIndexFormatParameter p)
			{
				indexFormat = p.IndexFormat;
			}

			List<GCPolygon> primitives = new();
			uint end_pos = (uint)(primitives_addr + primitives_size);

			while(primitives_addr < end_pos)
			{
				// if the primitive isnt valid
				if(reader[primitives_addr] == 0)
				{
					break;
				}

				primitives.Add(GCPolygon.Read(reader, ref primitives_addr, indexFormat));
			}

			return new GCMesh(parameters.ToArray(), primitives.ToArray());
		}

		/// <summary>
		/// Reads an array of meshes off an endian stack readaer.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="count">Number of meshes in the array to read.</param>
		/// <returns>The mesh array that was read.</returns>
		public static GCMesh[] ReadArray(EndianStackReader reader, uint address, int count)
		{
			GCIndexFormat indexAttribs = GCIndexFormat.HasPosition;

			GCMesh[] result = new GCMesh[count];
			for(int i = 0; i < count; i++, address += 16)
			{
				result[i] = Read(reader, address, ref indexAttribs);
			}

			return result;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a clone of the mesh.
		/// </summary>
		/// <returns>The cloned mesh.</returns>
		public GCMesh Clone()
		{
			return new((IGCParameter[])Parameters.Clone(), Polygons.ContentClone());
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return (IndexFormat.HasValue ? ((uint)IndexFormat.Value).ToString("X8") : "NULL") + $" - {Parameters.Length} - {Polygons.Length}";
		}
	}
}
