using Amicitia.IO.Binary;
using SA3D.Common;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Ginja.Enums;
using SA3D.Modeling.Structs;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Ginja
{
	/// <summary>
	/// A vertex data set, which can hold various types of data
	/// </summary>
	public class GinjaVertexSet : ICloneable, IBinarySerializable<IOContext>
	{
		private GinjaDataType _dataType;

		/// <summary>
		/// Null vertex set.
		/// </summary>
		public static readonly GinjaVertexSet EndVertexSet
			= new() { Type = GinjaVertexType.End };

		/// <summary>
		/// The type of vertex data that is stored.
		/// </summary>
		public GinjaVertexType Type { get; set; }

		/// <summary>
		/// The datatype as which the data is stored.
		/// </summary>
		public GinjaDataType DataType
		{
			get => _dataType;
			init => _dataType = value;
		}

		/// <summary>
		/// The structure in which the data is stored.
		/// </summary>
		public GinjaStructType StructType { get; set; }

		/// <summary>
		/// Raw Data behind the vertex set. Always a <see cref="LabeledArray{T}"/>
		/// </summary>
		public object? Data { get; private set; }

		/// <summary>
		/// Number of items in the data.
		/// </summary>
		public int DataLength
			=> ((IList?)Data)?.Count ?? 0;


		/// <summary>
		/// <see cref="GinjaDataType.Signed8"/> data.
		/// </summary>
		public LabeledArray<byte>? Unsigned8Data
		{
			get => GetData<byte>();
			set => SetData(value);
		}

		/// <summary>
		/// <see cref="GinjaDataType.Signed8"/> data.
		/// </summary>
		public LabeledArray<sbyte>? Signed8Data
		{
			get => GetData<sbyte>();
			set => SetData(value);
		}

		/// <summary>
		/// <see cref="GinjaDataType.Unsigned16"/> data.
		/// </summary>
		public LabeledArray<ushort>? Unsigned16Data
		{
			get => GetData<ushort>();
			set => SetData(value);
		}

		/// <summary>
		/// <see cref="GinjaDataType.Signed16"/> data.
		/// </summary>
		public LabeledArray<short>? Signed16Data
		{
			get => GetData<short>();
			set => SetData(value);
		}

		/// <summary>
		/// <see cref="GinjaDataType.Float32"/> data.
		/// </summary>
		public LabeledArray<float>? Float32Data
		{
			get => GetData<float>();
			set => SetData(value);
		}

		/// <summary>
		/// <see cref="GinjaDataType.RGB565"/>, <see cref="GinjaDataType.RGB8"/>, <see cref="GinjaDataType.RGBX8"/>, <see cref="GinjaDataType.RGBA4"/>, <see cref="GinjaDataType.RGBA6"/> or <see cref="GinjaDataType.RGBA8"/> data.
		/// </summary>
		public LabeledArray<Color>? ColorData
		{
			get => GetData<Color>();
			set => SetData(value);
		}


		private void SetData<T>(LabeledArray<T>? data) where T : struct
		{
			if(data == null)
			{
				Data = null;
				return;
			}

			Type dataType = DataType.GetDataReflectionType();
			if(typeof(T) != dataType)
			{
				throw new ArgumentException($"Invalid data type \"{typeof(T)}\" for vertex type \"{Type}\"! Expected data type \"{dataType}\"!");
			}

			Data = data;
		}

		private LabeledArray<T>? GetData<T>() where T : struct
		{
			if(Data == null)
			{
				return null;
			}

			if(Data is not LabeledArray<T> data)
			{
				throw new InvalidOperationException($"VertexSet does not contain {typeof(T)} data!");
			}

			return data;
		}


		/// <summary>
		/// Creates a new <see cref="GinjaVertexType.End"/> vertex set (implemented for binary serialization)
		/// </summary>
		public GinjaVertexSet()
		{
			Type = GinjaVertexType.End;
		}


		/// <summary>
		/// Changes the data type and data, which is expected to be a <see cref="LabeledArray{T}"/>
		/// </summary>
		/// <param name="dataType">The new data type</param>
		/// <param name="data">The new data. Must be a <see cref="LabeledArray{T}"/></param>
		public void ChangeDataType(GinjaDataType dataType, object? data)
		{
			if(data != null)
			{
				Type expectedType = dataType switch
				{
					GinjaDataType.Unsigned8 => typeof(LabeledArray<byte>),
					GinjaDataType.Signed8 => typeof(LabeledArray<sbyte>),
					GinjaDataType.Unsigned16 => typeof(LabeledArray<ushort>),
					GinjaDataType.Signed16 => typeof(LabeledArray<short>),
					GinjaDataType.Float32 => typeof(LabeledArray<float>),

					GinjaDataType.RGB565
					or GinjaDataType.RGB8
					or GinjaDataType.RGBX8
					or GinjaDataType.RGBA4
					or GinjaDataType.RGBA6
					or GinjaDataType.RGBA8 => typeof(LabeledArray<Color>),

					_ => throw new InvalidEnumArgumentException(nameof(dataType), (int)dataType, typeof(GinjaDataType)),
				};
				if(data.GetType() != expectedType)
				{
					throw new ArgumentException($"Invalid data type \"{data.GetType()}\"! Expected \"{expectedType}\"!");
				}
			}

			_dataType = dataType;
			Data = data;
		}


		/// <summary>
		/// Returns data as a float array (not applicable to color types)
		/// </summary>
		/// <param name="fractionalBitCount">Fractional bit count to use for when converting fixed-pointer number to float</param>
		public float[]? GetDataAsFloat(int fractionalBitCount)
		{
			if(DataType is >= GinjaDataType.RGB565)
			{
				throw new InvalidOperationException("Vertex set contains colors and is thus not convertable to float!");
			}

			if(Data == null)
			{
				return null;
			}

			float[] result = DataType switch
			{
				GinjaDataType.Unsigned8 => [.. Unsigned8Data!.Select(x => (float)x)],
				GinjaDataType.Signed8 => [.. Signed8Data!.Select(x => (float)x)],
				GinjaDataType.Unsigned16 => [.. Unsigned16Data!.Select(x => (float)x)],
				GinjaDataType.Signed16 => [.. Signed16Data!.Select(x => (float)x)],
				GinjaDataType.Float32 => Float32Data!.ToArray(),

				_ => throw new InvalidOperationException($"Vertex set has invalid data type \"{DataType}\""!),
			};

			if(DataType != GinjaDataType.Float32 && fractionalBitCount > 0)
			{
				byte maxFractionalBitCount = (byte)(DataType.GetDataByteSize() * 8);
				if(fractionalBitCount > maxFractionalBitCount)
				{
					throw new ArgumentOutOfRangeException(nameof(fractionalBitCount), fractionalBitCount, $"Fractional bit count too large! With a datatype of \"{DataType}\", it can at most be {maxFractionalBitCount}!");
				}

				float fractionalFactor = 1 / (float)(1 << fractionalBitCount);
				for(int i = 0; i < result.Length; i++)
				{
					result[i] *= fractionalFactor;
				}
			}

			return result;
		}

		/// <summary>
		/// Returns data as a vector2 array (not applicable to color types)
		/// </summary>
		/// <param name="fractionalBitCount">Fractional bit count to use for when converting fixed-pointer number to float</param>
		public Vector2[]? GetDataAsVector2(int fractionalBitCount)
		{
			if(Data == null)
			{
				return null;
			}

			if(DataLength % 2 != 0)
			{
				throw new InvalidOperationException($"Vertex set data length is {DataLength}, which is not a multiple of 2!");
			}

			float[] values = GetDataAsFloat(fractionalBitCount)!;

			Vector2[] result = new Vector2[values.Length / 2];
			for(int i = 0; i < result.Length; i++)
			{
				result[i] = new(
					values[i * 2],
					values[(i * 2) + 1]
				);
			}

			return result;
		}

		/// <summary>
		/// Returns data as a vector3 array (not applicable to color types)
		/// </summary>
		/// <param name="fractionalBitCount">Fractional bit count to use for when converting fixed-pointer number to float</param>
		public Vector3[]? GetDataAsVector3(int fractionalBitCount)
		{
			if(Data == null)
			{
				return null;
			}

			if(DataLength % 3 != 0)
			{
				throw new InvalidOperationException($"Vertex set data length is {DataLength}, which is not a multiple of 3!");
			}

			float[] values = GetDataAsFloat(fractionalBitCount)!;

			Vector3[] result = new Vector3[values.Length / 3];
			for(int i = 0; i < result.Length; i++)
			{
				result[i] = new(
					values[i * 3],
					values[(i * 3) + 1],
					values[(i * 3) + 2]
				);
			}

			return result;
		}


		/// <summary>
		/// Replace data with float-based data
		/// </summary>
		/// <param name="data">The float data to insert</param>
		/// <param name="fractionalBitCount">Fractional bit count to use when converting to fixed-pointer numbers</param>
		public void SetFloatData(float[]? data, int fractionalBitCount)
		{
			if(DataType is >= GinjaDataType.RGB565)
			{
				throw new InvalidOperationException("Vertex set contains colors and is thus not convertable from float!");
			}

			if(data == null)
			{
				Data = null;
				return;
			}

			float[] processedData = data;

			if(DataType != GinjaDataType.Float32 && fractionalBitCount > 0)
			{
				byte maxFractionalBitCount = (byte)(DataType.GetDataByteSize() * 8);
				if(fractionalBitCount > maxFractionalBitCount)
				{
					throw new ArgumentOutOfRangeException(nameof(fractionalBitCount), fractionalBitCount, $"Fractional bit count too large! With a datatype of \"{DataType}\", it can at most be {maxFractionalBitCount}!");
				}

				float fractionalFactor = 1 << fractionalBitCount;
				processedData = [.. data.Select(x => x * fractionalFactor)];
			}

			string label = ((ILabel?)Data)?.Label ?? (Type.ToString().ToLower() + "_").GenerateIdentifier();

			switch(DataType)
			{
				case GinjaDataType.Unsigned8:
					Unsigned8Data = new(label, [.. processedData.Select(x => (byte)x)]);
					break;
				case GinjaDataType.Signed8:
					Signed8Data = new(label, [.. processedData.Select(x => (sbyte)x)]);
					break;
				case GinjaDataType.Unsigned16:
					Unsigned16Data = new(label, [.. processedData.Select(x => (byte)x)]);
					break;
				case GinjaDataType.Signed16:
					Signed16Data = new(label, [.. processedData.Select(x => (byte)x)]);
					break;
				case GinjaDataType.Float32:
					Float32Data = new(label, processedData);
					break;
			}
		}

		/// <summary>
		/// Replace data with float-based data from vectors
		/// </summary>
		/// <param name="data">The float data to insert</param>
		/// <param name="fractionalBitCount">Fractional bit count to use when converting to fixed-pointer numbers</param>
		public void SetFloatData(Vector2[]? data, int fractionalBitCount)
		{
			SetFloatData(
				data?.SelectMany<Vector2, float>(x => [x.X, x.Y]).ToArray(),
				fractionalBitCount
			);
		}

		/// <summary>
		/// Replace data with float-based data from vectors
		/// </summary>
		/// <param name="data">The float data to insert</param>
		/// <param name="fractionalBitCount">Fractional bit count to use when converting to fixed-pointer numbers</param>
		public void SetFloatData(Vector3[]? data, int fractionalBitCount)
		{
			SetFloatData(
				data?.SelectMany<Vector3, float>(x => [x.X, x.Y, x.Z]).ToArray(),
				fractionalBitCount
			);
		}


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, IOContext context)
		{
			Type = (GinjaVertexType)reader.ReadByte();
			int readStructSize = reader.ReadByte();
			int count = reader.ReadUInt16();

			uint structure = reader.ReadUInt32();
			StructType = (GinjaStructType)(structure & 0x0F);
			_dataType = (GinjaDataType)((structure >> 4) & 0x0F);

			if(Type != GinjaVertexType.End)
			{
				// verify the struct size
				int structSize = StructType.GetStructComponentCount() * DataType.GetDataByteSize();
				if(readStructSize != structSize)
				{
					throw new Exception($"Read structure size doesnt match calculated structure size: {readStructSize} != {structSize}");
				}
			}

			long dataOffset = reader.ReadOffsetValue();
			reader.Skip(sizeof(uint)); // Skipping data byte size

			if(Type == GinjaVertexType.End)
			{
				return;
			}

			int arraySize = StructType.GetStructComponentCount() * count;
			string labelPrefix = Type.ToString() + "_";

			Data = DataType switch
			{
				GinjaDataType.Unsigned8 => reader.ReadLabeledArrayAtOffset<byte>(dataOffset, arraySize, labelPrefix, context.PointerLUT),
				GinjaDataType.Signed8 => reader.ReadLabeledArrayAtOffset<sbyte>(dataOffset, arraySize, labelPrefix, context.PointerLUT),
				GinjaDataType.Unsigned16 => reader.ReadLabeledArrayAtOffset<ushort>(dataOffset, arraySize, labelPrefix, context.PointerLUT),
				GinjaDataType.Signed16 => reader.ReadLabeledArrayAtOffset<short>(dataOffset, arraySize, labelPrefix, context.PointerLUT),
				GinjaDataType.Float32 => reader.ReadLabeledArrayAtOffset<float>(dataOffset, arraySize, labelPrefix, context.PointerLUT),
				GinjaDataType.RGB565 => reader.ReadLabeledObjectArrayAtOffset<Color, ColorIOType>(dataOffset, arraySize, labelPrefix, ColorIOType.RGB565, context.PointerLUT),
				GinjaDataType.RGBA8 => reader.ReadLabeledObjectArrayAtOffset<Color, ColorIOType>(dataOffset, arraySize, labelPrefix, ColorIOType.RGBA8, context.PointerLUT),
				GinjaDataType.RGB8 or GinjaDataType.RGBX8 or GinjaDataType.RGBA4 or GinjaDataType.RGBA6 => throw new NotImplementedException($"Data type \"{DataType}\" is not implemented"),
				_ => throw new InvalidDataException($"Invalid data type \"{DataType}\"!"),
			};
		}

		internal static LabeledArray<GinjaVertexSet> ReadArray(BinaryObjectReader reader, IOContext context)
		{
			List<GinjaVertexSet> result = [];

			while(reader.ReadObject<GinjaVertexSet, IOContext>(context) is GinjaVertexSet vertexSet && vertexSet.Type != GinjaVertexType.End)
			{
				result.Add(vertexSet);
			}

			return new([.. result]);
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, IOContext context)
		{
			int structComponentCount = StructType.GetStructComponentCount();
			if(DataLength % structComponentCount != 0)
			{
				throw new InvalidOperationException($"Length of data with struct type \"{StructType}\" must be a multiple of {structComponentCount}; Is {DataLength} (off by {DataLength % structComponentCount})");
			}

			byte structSize = (byte)(structComponentCount * DataType.GetDataByteSize());
			ushort dataLength = (ushort)(DataLength / structComponentCount);

			writer.WriteByte((byte)Type);
			writer.WriteByte(structSize);
			writer.WriteUInt16(dataLength);

			uint structure = (uint)StructType;
			structure |= (uint)((byte)DataType << 4);
			writer.WriteUInt32(structure);

			switch(DataType)
			{
				case GinjaDataType.Unsigned8:
					writer.WriteArrayOffset(Unsigned8Data, context.PointerLUT, 4);
					break;
				case GinjaDataType.Signed8:
					writer.WriteArrayOffset(Signed8Data, context.PointerLUT, 4);
					break;
				case GinjaDataType.Unsigned16:
					writer.WriteArrayOffset(Unsigned16Data, context.PointerLUT, 4);
					break;
				case GinjaDataType.Signed16:
					writer.WriteArrayOffset(Signed16Data, context.PointerLUT, 4);
					break;
				case GinjaDataType.Float32:
					writer.WriteArrayOffset(Float32Data, context.PointerLUT);
					break;
				case GinjaDataType.RGB565:
					writer.WriteObjectArrayOffset(ColorData, ColorIOType.RGB565, context.PointerLUT, 4);
					break;
				case GinjaDataType.RGBA8:
					writer.WriteObjectArrayOffset(ColorData, ColorIOType.RGBA8, context.PointerLUT);
					break;
				case GinjaDataType.RGB8:
				case GinjaDataType.RGBX8:
				case GinjaDataType.RGBA4:
				case GinjaDataType.RGBA6:
					throw new NotImplementedException($"Data type \"{DataType}\" is not implemented");
				default:
					throw new InvalidDataException($"Invalid data type \"{DataType}\"!");
			}

			writer.WriteUInt32((uint)(dataLength * structSize));
		}

		internal static void WriteArray(BinaryObjectWriter writer, IEnumerable<GinjaVertexSet> vertexSets, IOContext context)
		{
			foreach(GinjaVertexSet vertexSet in vertexSets)
			{
				writer.WriteObject(vertexSet, context);
			}

			writer.WriteObject(EndVertexSet);
		}


		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the vertex set.
		/// </summary>
		/// <returns></returns>
		public GinjaVertexSet Clone()
		{
			return new()
			{
				Type = Type,
				DataType = DataType,
				StructType = StructType,
				Data = ((ICloneable?)Data)?.Clone(),
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Type}: {DataLength}";
		}

	}
}