using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Modeling.Mesh.Gamecube.Enums;
using SA3D.Modeling.Mesh.Gamecube.Parameters;
using SA3D.Modeling.Structs;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Gamecube
{
	/// <summary>
	/// A vertex data set, which can hold various types of data
	/// </summary>
	public class GCVertexSet : ICloneable
	{
		/// <summary>
		/// Null vertex set.
		/// </summary>
		public static readonly GCVertexSet EndVertexSet
			= new(GCVertexType.End, default, default, null);

		/// <summary>
		/// The type of vertex data that is stored.
		/// </summary>
		public GCVertexType Type { get; }

		/// <summary>
		/// The datatype as which the data is stored.
		/// </summary>
		public GCDataType DataType { get; }

		/// <summary>
		/// The structure in which the data is stored.
		/// </summary>
		public GCStructType StructType { get; }

		/// <summary>
		/// The size of a single element in the list in bytes
		/// </summary>
		public uint StructSize => GCEnumExtensions.GetStructSize(StructType, DataType);

		/// <summary>
		/// Raw Data behind the vertex set.
		/// </summary>
		public Array? Data { get; private set; }

		/// <summary>
		/// Number of entries in the data.
		/// </summary>
		public int DataLength
			=> Data?.Length ?? 0;

		/// <summary>
		/// Vector3 data.
		/// </summary>
		public Vector3[] Vector3Data
		{
			get
			{
				if(Data is not Vector3[] v3data)
				{
					throw new InvalidOperationException("VertexSet does not contain Vector3 data!");
				}

				return v3data;
			}
		}

		/// <summary>
		/// Vector2 data.
		/// </summary>
		public Vector2[] Vector2Data
		{
			get
			{
				if(Data is not Vector2[] uvdata)
				{
					throw new InvalidOperationException("VertexSet does not contain Vector2 data!");
				}

				return uvdata;
			}
		}

		/// <summary>
		/// Color data.
		/// </summary>
		public Color[] ColorData
		{
			get
			{
				if(Data is not Color[] coldata)
				{
					throw new InvalidOperationException("VertexSet does not contain Color data!");
				}

				return coldata;
			}
		}


		/// <summary>
		/// Creates a new custom Vertex set.
		/// </summary>
		/// <param name="attribute">The type of vertex data that is stored.</param>
		/// <param name="dataType">The datatype as which the data is stored.</param>
		/// <param name="structType">The structure in which the data is stored.</param>
		/// <param name="data">Raw Data behind the vertex set.</param>
		public GCVertexSet(GCVertexType attribute, GCDataType dataType, GCStructType structType, Array? data)
		{
			Type = attribute;
			DataType = dataType;
			StructType = structType;
			Data = data;

			if(data is not (null or Vector3[] or Vector2[] or Color[]))
			{
				throw new ArgumentException("Data array has to hold either Vector3, Vector2 or Color!");
			}
		}


		/// <summary>
		/// Creates a new vertex set for position data.
		/// </summary>
		/// <param name="positions">The position data.</param>
		/// <returns>The created vertex set.</returns>
		public static GCVertexSet CreatePositionSet(Vector3[] positions)
		{
			return new GCVertexSet(GCVertexType.Position, GCDataType.Float32, GCStructType.PositionXYZ, positions);
		}

		/// <summary>
		/// Creates a new vertex set for normal data.
		/// </summary>
		/// <param name="normals">The normal data.</param>
		/// <returns>The created vertex set.</returns>
		public static GCVertexSet CreateNormalSet(Vector3[] normals)
		{
			return new GCVertexSet(GCVertexType.Normal, GCDataType.Float32, GCStructType.NormalXYZ, normals);
		}

		/// <summary>
		/// Creates a new vertex set for texcoord0 data.
		/// </summary>
		/// <param name="texcoords">The texcoord data.</param>
		/// <returns>The created vertex set.</returns>
		public static GCVertexSet CreateTexcoord0Set(Vector2[] texcoords)
		{
			return new GCVertexSet(GCVertexType.TexCoord0, GCDataType.Signed16, GCStructType.TexCoordUV, texcoords);
		}

		/// <summary>
		/// Creates a new vertex set for color0 data.
		/// </summary>
		/// <param name="colors">The color data.</param>
		/// <returns>The created vertex set.</returns>
		public static GCVertexSet CreateColor0Set(Color[] colors)
		{
			return new GCVertexSet(GCVertexType.Color0, GCDataType.RGBA8, GCStructType.ColorRGBA, colors);
		}

		/// <summary>
		/// Removes double values from the data. Can correct affected indices in mesh polygons.
		/// </summary>
		/// <param name="meshes">The meshes to correct the polygons of.</param>
		/// <exception cref="NotSupportedException"></exception>
		public unsafe void Optimize(IEnumerable<GCMesh>? meshes)
		{
			if(Data == null)
			{
				return;
			}

			int prevLength = Data.Length;

			int[]? OptimizeData<T>(T[] data) where T : IEquatable<T>
			{
				if(data.TryCreateDistinctMap(out DistinctMap<T> mapping))
				{
					Data = mapping.ValueArray;
				}

				return mapping.Map;
			}

			int[]? map = Data switch
			{
				Vector3[] vector3Data => OptimizeData(vector3Data),
				Vector2[] vector2Data => OptimizeData(vector2Data),
				Color[] colorData => OptimizeData(colorData),
				_ => throw new NotSupportedException(),
			};

			if(map is null || meshes == null)
			{
				return;
			}

			int fieldOffset = (int)Type;


			foreach(GCMesh mesh in meshes)
			{
				for(int i = 0; i < mesh.Polygons.Length; i++)
				{
					int count = mesh.Polygons[i].Corners.Length;
					fixed(GCCorner* corner = &mesh.Polygons[i].Corners[0])
					{
						// Offsetting it so that its on the correct field for every element.
						// We just keep it as a GCCorner* so that ++ moves forward a full element.
						GCCorner* current = (GCCorner*)(((ushort*)corner) + fieldOffset);

						for(int j = 0; j < count; j++, current++)
						{
							*(ushort*)current = (ushort)map[*(ushort*)current];
						}
					}
				}
			}

			if(prevLength > 256 && Data.Length <= 256)
			{
				GCIndexFormat useLargeIndex = (GCIndexFormat)(1 << (fieldOffset * 2));
				GCIndexFormat hasData = (GCIndexFormat)((uint)useLargeIndex << 1);
				useLargeIndex = ~useLargeIndex;

				foreach(GCMesh mesh in meshes)
				{
					for(int i = 0; i < mesh.Parameters.Length; i++)
					{
						if(mesh.Parameters[i] is not GCIndexFormatParameter param
							|| !param.IndexFormat.HasFlag(hasData))
						{
							continue;
						}

						param.IndexFormat &= useLargeIndex;
						mesh.Parameters[i] = param;
					}
				}
			}
		}


		/// <summary>
		/// Writes the data behind the set to the endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <returns>Address at whicht the data was written.</returns>
		/// <exception cref="FormatException"></exception>
		public uint WriteData(EndianStackWriter writer)
		{
			uint result = writer.PointerPosition;

			switch(Type)
			{
				case GCVertexType.Position:
				case GCVertexType.Normal:
					foreach(Vector3 vec in Vector3Data)
					{
						writer.WriteVector3(vec);
					}

					break;
				case GCVertexType.Color0:
				case GCVertexType.Color1:
					foreach(Color col in ColorData)
					{
						writer.WriteColor(col, ColorIOType.RGBA8);
					}

					break;
				case GCVertexType.TexCoord0:
				case GCVertexType.TexCoord1:
				case GCVertexType.TexCoord2:
				case GCVertexType.TexCoord3:
				case GCVertexType.TexCoord4:
				case GCVertexType.TexCoord5:
				case GCVertexType.TexCoord6:
				case GCVertexType.TexCoord7:
					foreach(Vector2 uv in Vector2Data)
					{
						writer.WriteVector2(uv * 255f, FloatIOType.Short);
					}

					break;
				case GCVertexType.PositionMatrixID:
				case GCVertexType.End:
				default:
					throw new FormatException($"Vertex type invalid: {Type}");
			}

			return result;
		}

		/// <summary>
		/// Writes the sets structure/header to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="dataAddress">The address at which the sets data is located.</param>
		public void WriteHeader(EndianStackWriter writer, uint dataAddress)
		{
			writer.WriteByte((byte)Type);
			writer.WriteByte((byte)StructSize);
			writer.WriteUShort((ushort)DataLength);

			uint structure = (uint)StructType;
			structure |= (uint)((byte)DataType << 4);
			writer.WriteUInt(structure);

			writer.WriteUInt(dataAddress);
			writer.WriteUInt((uint)(DataLength * StructSize));
		}

		/// <summary>
		/// Writes an array of vertex sets to an endian stack writer. Includes end marker.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="vertexSets">The vertex sets to write.</param>
		/// <returns>Address at which the vertex set headers were written.</returns>
		public static uint WriteArray(EndianStackWriter writer, GCVertexSet[] vertexSets)
		{
			uint[] dataAddresses = new uint[vertexSets.Length];
			for(int i = 0; i < vertexSets.Length; i++)
			{
				dataAddresses[i] = vertexSets[i].WriteData(writer);
			}

			uint result = writer.PointerPosition;

			for(int i = 0; i < vertexSets.Length; i++)
			{
				vertexSets[i].WriteHeader(writer, dataAddresses[i]);
			}

			// End marker
			writer.WriteByte(0xFF);
			writer.WriteEmpty(15);

			return result;
		}

		/// <summary>
		/// Read a vertex set off an endian data reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <returns>The vertex set that was read.</returns>
		public static GCVertexSet Read(EndianStackReader reader, uint address)
		{
			GCVertexType attribute = (GCVertexType)reader[address];
			if(attribute == GCVertexType.End)
			{
				return EndVertexSet;
			}

			uint structure = reader.ReadUInt(address + 4);
			GCStructType structType = (GCStructType)(structure & 0x0F);
			GCDataType dataType = (GCDataType)((structure >> 4) & 0x0F);
			uint structSize = GCEnumExtensions.GetStructSize(structType, dataType);

			if(reader[address + 1] != structSize)
			{
				throw new Exception($"Read structure size doesnt match calculated structure size: {reader[address + 1]} != {structSize}");
			}

			// reading the data
			int count = reader.ReadUShort(address + 2);
			uint dataAddr = reader.ReadPointer(address + 8);

			Array setdata;

			switch(attribute)
			{
				case GCVertexType.Position:
				case GCVertexType.Normal:
					Vector3[] vector3Data = new Vector3[count];
					for(int i = 0; i < count; i++)
					{
						vector3Data[i] = reader.ReadVector3(ref dataAddr);
					}

					setdata = vector3Data;
					break;
				case GCVertexType.Color0:
				case GCVertexType.Color1:
					Color[] colorData = new Color[count];
					for(int i = 0; i < count; i++)
					{
						colorData[i] = reader.ReadColor(ref dataAddr, ColorIOType.RGBA8);
					}

					setdata = colorData;
					break;
				case GCVertexType.TexCoord0:
				case GCVertexType.TexCoord1:
				case GCVertexType.TexCoord2:
				case GCVertexType.TexCoord3:
				case GCVertexType.TexCoord4:
				case GCVertexType.TexCoord5:
				case GCVertexType.TexCoord6:
				case GCVertexType.TexCoord7:
					Vector2[] uvData = new Vector2[count];
					for(int i = 0; i < count; i++)
					{
						uvData[i] = reader.ReadVector2(ref dataAddr, FloatIOType.Short) / 255f;
					}

					setdata = uvData;
					break;
				case GCVertexType.PositionMatrixID:
					throw new NotSupportedException();
				case GCVertexType.End:
				default:
					throw new ArgumentException($"Vertex type invalid: {attribute}");
			}

			return new GCVertexSet(attribute, dataType, structType, setdata);
		}

		/// <summary>
		/// Reads an array of vertex sets off an endian stack reader. Stops reading when an end marker set is encountered.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <returns>The vertex sets that were read.</returns>
		public static GCVertexSet[] ReadArray(EndianStackReader reader, uint address)
		{
			List<GCVertexSet> result = [];
			GCVertexSet vertexSet = Read(reader, address);
			while(vertexSet.Type != GCVertexType.End)
			{
				result.Add(vertexSet);
				address += 16;
				vertexSet = Read(reader, address);
			}

			return result.ToArray();
		}


		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the vertex set.
		/// </summary>
		/// <returns></returns>
		public GCVertexSet Clone()
		{
			return new(Type, DataType, StructType, (Array?)Data?.Clone() ?? null);
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Type}: {DataLength}";
		}
	}
}