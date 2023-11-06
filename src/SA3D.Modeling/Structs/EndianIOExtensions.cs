using SA3D.Common.IO;
using SA3D.Common.Lookup;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// Extension methods for <see cref="EndianStackReader"/> and <see cref="EndianStackWriter"/>.
	/// </summary>
	public static class EndianIOExtensions
	{
		#region Writing

		/// <summary>
		/// Writes a color to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="color">The color to write.</param>
		/// <param name="type">The data type by which to write the color.</param>
		/// <exception cref="ArgumentException"/>
		public static void WriteColor(this EndianStackWriter writer, Color color, ColorIOType type)
		{
			switch(type)
			{
				case ColorIOType.RGBA8:
					writer.WriteUInt(color.RGBA);
					break;
				case ColorIOType.ARGB8_32:
					writer.WriteUInt(color.ARGB);
					break;
				case ColorIOType.ARGB8_16:
					uint val = color.ARGB;
					writer.WriteUShort((ushort)val);
					writer.WriteUShort((ushort)(val >> 16));
					break;
				case ColorIOType.ARGB4:
					writer.WriteUShort(color.ARGB4);
					break;
				case ColorIOType.RGB565:
					writer.WriteUShort(color.RGB565);
					break;
				default:
					throw new ArgumentException($"Invalid type.", nameof(type));
			}
		}

		/// <summary>
		/// Writes a 2 component vector to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="vector2">The vector to write.</param>
		/// <param name="type">The data type by which to write the vector.</param>
		public static void WriteVector2(this EndianStackWriter writer, Vector2 vector2, FloatIOType type = FloatIOType.Float)
		{
			Action<EndianStackWriter, float> floatWriter = type.GetWriter();

			floatWriter(writer, vector2.X);
			floatWriter(writer, vector2.Y);
		}

		/// <summary>
		/// Writes a 3 component vector to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="vector3">The vector to write.</param>
		/// <param name="type">The data type by which to write the vector.</param>
		public static void WriteVector3(this EndianStackWriter writer, Vector3 vector3, FloatIOType type = FloatIOType.Float)
		{
			Action<EndianStackWriter, float> floatWriter = type.GetWriter();

			floatWriter(writer, vector3.X);
			floatWriter(writer, vector3.Y);
			floatWriter(writer, vector3.Z);
		}

		/// <summary>
		/// Writes a quaternion to an endian stack writer (WXYZ).
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="quaternion">The quaternion to write.</param>
		public static void WriteQuaternion(this EndianStackWriter writer, Quaternion quaternion)
		{
			writer.WriteFloat(quaternion.W);
			writer.WriteFloat(quaternion.X);
			writer.WriteFloat(quaternion.Y);
			writer.WriteFloat(quaternion.Z);
		}


		/// <summary>
		/// Delegate for writing values to an endian stack writer.
		/// </summary>
		/// <typeparam name="T">The type of value to write.</typeparam>
		/// <param name="writer">The write to write to.</param>
		/// <param name="value">The value to write..</param>
		public delegate void WriteValueDelegate<T>(EndianStackWriter writer, T value);

		/// <summary>
		/// Writes a collection to a writer and returns the pointer position at which the collection has started writing.
		/// </summary>
		/// <typeparam name="T">Type of value the collection stores.</typeparam>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="values">The values to write.</param>
		/// <param name="write">Function responsible for writing.</param>
		/// <param name="preWrite">If specified, this will be executed for every value before the actual collection gets written.</param>
		/// <returns>The address at which the collection was written.</returns>
		public static uint WriteCollection<T>(
			this EndianStackWriter writer,
			ICollection<T> values,
			WriteValueDelegate<T> write,
			WriteValueDelegate<T>? preWrite)
		{
			if(preWrite != null)
			{
				foreach(T value in values)
				{
					preWrite(writer, value);
				}
			}

			uint result = writer.PointerPosition;

			foreach(T value in values)
			{
				write(writer, value);
			}

			return result;
		}

		/// <summary>
		/// Writes a collection to a writer and returns the pointer position at which the collection has started writing.
		/// </summary>
		/// <typeparam name="T">Type of value the collection stores.</typeparam>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="values">The values to write.</param>
		/// <param name="write">Function responsible for writing.</param>
		/// <returns>The address at which the collection was written.</returns>
		public static uint WriteCollection<T>(
			this EndianStackWriter writer,
			ICollection<T> values,
			WriteValueDelegate<T> write)
		{
			return writer.WriteCollection(values, write, null);
		}

		/// <summary>
		/// Writes a collection to a writer and returns the pointer position at which the collection has started writing.
		/// <br/> Checks if the collection is already in the LUT, and does not write the collection if that is the case.
		/// </summary>
		/// <typeparam name="T">Type of value the collection stores.</typeparam>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="values">The values to write.</param>
		/// <param name="write">Function responsible for writing.</param>
		/// <param name="preWrite">If specified, this will be executed for every value before the actual collection gets written.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The address at which the collection was written.</returns>
		public static uint WriteCollectionWithLUT<T>(
			this EndianStackWriter writer,
			ICollection<T>? values,
			WriteValueDelegate<T> write,
			WriteValueDelegate<T>? preWrite,
			BaseLUT lut)
		{
			return lut.GetAddAddress(values, () => writer.WriteCollection(values!, write, preWrite));
		}

		/// <summary>
		/// Writes a collection to a writer and returns the pointer position at which the collection has started writing.
		/// <br/> Checks if the collection is already in the LUT, and does not write the collection if that is the case.
		/// </summary>
		/// <typeparam name="T">Type of value the collection stores.</typeparam>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="values">The values to write.</param>
		/// <param name="write">Function responsible for writing.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The address at which the collection was written.</returns>
		public static uint WriteCollectionWithLUT<T>(
			this EndianStackWriter writer,
			ICollection<T>? values,
			WriteValueDelegate<T> write,
			BaseLUT lut)
		{
			return writer.WriteCollectionWithLUT(values, write, null, lut);
		}

		#endregion

		#region Reading

		/// <summary>
		/// Reads a color off an endian stack reader. Advances the address by the number of bytes read.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <param name="type">The type by which to read the color.</param>
		/// <returns>The read color.</returns>
		/// <exception cref="ArgumentException"/>
		public static Color ReadColor(this EndianStackReader reader, ref uint address, ColorIOType type)
		{
			Color col = default;
			switch(type)
			{
				case ColorIOType.RGBA8:
					col.RGBA = reader.ReadUInt(address);
					address += 4;
					break;
				case ColorIOType.ARGB8_32:
					col.ARGB = reader.ReadUInt(address);
					address += 4;
					break;
				case ColorIOType.ARGB8_16:
					ushort GB = reader.ReadUShort(address);
					ushort AR = reader.ReadUShort(address + 2);
					col.ARGB = (uint)(GB | (AR << 16));
					address += 4;
					break;
				case ColorIOType.ARGB4:
					col.ARGB4 = reader.ReadUShort(address);
					address += 2;
					break;
				case ColorIOType.RGB565:
					col.RGB565 = reader.ReadUShort(address);
					address += 2;
					break;
				default:
					throw new ArgumentException($"Invalid type.", nameof(type));
			}

			return col;
		}

		/// <summary>
		/// Reads a color off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <param name="type">The type by which to read the color.</param>
		/// <returns>The read color.</returns>
		/// <exception cref="ArgumentException"/>
		public static Color ReadColor(this EndianStackReader reader, uint address, ColorIOType type)
		{
			return reader.ReadColor(ref address, type);
		}

		/// <summary>
		/// Reads a 2 component vector off an endian stack reader. Advances the address by the number of bytes read.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <param name="type">The type by which to read the vector.</param>
		/// <returns>The read vector.</returns>
		public static Vector2 ReadVector2(this EndianStackReader reader, ref uint address, FloatIOType type = FloatIOType.Float)
		{
			Func<EndianStackReader, uint, float> readFloat = type.GetReader();
			uint fieldSize = (uint)type.GetByteSize();

			Vector2 result = new(
				readFloat(reader, address),
				readFloat(reader, address += fieldSize)
				);

			address += fieldSize;
			return result;
		}

		/// <summary>
		/// Reads a 2 component vector off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <param name="type">The type by which to read the vector.</param>
		/// <returns>The read vector.</returns>
		public static Vector2 ReadVector2(this EndianStackReader reader, uint address, FloatIOType type = FloatIOType.Float)
		{
			return reader.ReadVector2(ref address, type);
		}

		/// <summary>
		/// Reads a 3 component vector off an endian stack reader. Advances the address by the number of bytes read.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <param name="type">The type by which to read the vector.</param>
		/// <returns>The read vector.</returns>
		public static Vector3 ReadVector3(this EndianStackReader reader, ref uint address, FloatIOType type = FloatIOType.Float)
		{
			Func<EndianStackReader, uint, float> readFloat = type.GetReader();
			uint fieldSize = (uint)type.GetByteSize();

			Vector3 result = new(
				readFloat(reader, address),
				readFloat(reader, address += fieldSize),
				readFloat(reader, address += fieldSize)
				);

			address += fieldSize;
			return result;
		}

		/// <summary>
		/// Reads a 3 component vector off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <param name="type">The type by which to read the vector.</param>
		/// <returns>The read vector.</returns>
		public static Vector3 ReadVector3(this EndianStackReader reader, uint address, FloatIOType type = FloatIOType.Float)
		{
			return reader.ReadVector3(ref address, type);
		}

		/// <summary>
		/// Reads a quaternion off an endian stack reader. Advances the address by the number of bytes read.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <returns>The read quaternion.</returns>
		public static Quaternion ReadQuaternion(this EndianStackReader reader, ref uint address)
		{
			Quaternion result = new()
			{
				W = reader.ReadFloat(address),
				X = reader.ReadFloat(address + 4),
				Y = reader.ReadFloat(address + 8),
				Z = reader.ReadFloat(address + 12)
			};

			address += 16;
			return result;
		}

		/// <summary>
		/// Reads a quaternion off an endian stack reader. Advances the address by the number of bytes read.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <returns>The read quaternion.</returns>
		public static Quaternion ReadQuaternion(this EndianStackReader reader, uint address)
		{
			return reader.ReadQuaternion(ref address);
		}


		/// <summary>
		/// Delegate for reading values from an endian stack reader. Used for reading and advancing the address by the number of bytes read.
		/// </summary>
		/// <typeparam name="T">The type of value to read.</typeparam>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <returns>The read value</returns>
		public delegate T ReadValueAdvanceDelegate<T>(EndianStackReader reader, ref uint address);

		/// <summary>
		/// Delegate for reading values from an endian stack reader.
		/// </summary>
		/// <typeparam name="T">The type of value to read.</typeparam>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <returns>The read value</returns>
		public delegate T ReadValueDelegate<T>(EndianStackReader reader, uint address);

		/// <summary>
		/// Reads an array of values off an endian stack reader.
		/// </summary>
		/// <typeparam name="T">Type of value to read.</typeparam>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <param name="count">The number of elements to read.</param>
		/// <param name="read">The read function.</param>
		/// <returns>The read array.</returns>
		public static T[] ReadArray<T>(
			this EndianStackReader reader,
			uint address,
			uint count,
			ReadValueAdvanceDelegate<T> read)
		{
			T[] result = new T[count];

			for(int i = 0; i < count; i++)
			{
				result[i] = read(reader, ref address);
			}

			return result;
		}

		/// <summary>
		/// Reads an array of values off an endian stack reader.
		/// </summary>
		/// <typeparam name="T">Type of value to read.</typeparam>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <param name="count">The number of elements to read.</param>
		/// <param name="elementByteSize">Number of bytes to advance the address after reading an element.</param>
		/// <param name="read">The read function.</param>
		/// <returns>The read array.</returns>
		public static T[] ReadArray<T>(
			this EndianStackReader reader,
			uint address,
			uint count,
			uint elementByteSize,
			ReadValueDelegate<T> read)
		{
			T[] result = new T[count];

			for(int i = 0; i < count; i++)
			{
				result[i] = read(reader, address);
				address += elementByteSize;
			}

			return result;
		}

		/// <summary>
		/// Reads an array of values off an endian stack reader.
		/// <br/> Checks if the collection is already in the LUT, and returns it if that is the case.
		/// </summary>
		/// <typeparam name="T">Type of value to read.</typeparam>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <param name="count">The number of elements to read.</param>
		/// <param name="read">The read function.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The read array.</returns>
		public static T[] ReadArrayWithLUT<T>(
			this EndianStackReader reader,
			uint address,
			uint count,
			ReadValueAdvanceDelegate<T> read,
			BaseLUT lut)
		{
			return lut.GetAddValue(address, () => ReadArray(reader, address, count, read));
		}


		/// <summary>
		/// Reads an array of values off an endian stack reader.
		/// <br/> Checks if the collection is already in the LUT, and returns it if that is the case.
		/// </summary>
		/// <typeparam name="T">Type of value to read.</typeparam>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <param name="count">The number of elements to read.</param>
		/// <param name="elementByteSize">Number of bytes to advance the address after reading an element.</param>
		/// <param name="read">The read function.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The read array.</returns>
		public static T[] ReadArrayWithLUT<T>(
			this EndianStackReader reader,
			uint address,
			uint count,
			uint elementByteSize,
			ReadValueDelegate<T> read,
			BaseLUT lut)
		{
			return lut.GetAddValue(address, () => ReadArray(reader, address, count, elementByteSize, read));
		}

		/// <summary>
		/// Reads a labeled array of values off an endian stack reader.
		/// <br/> Checks if the collection is already in the LUT, and returns it if that is the case.
		/// </summary>
		/// <typeparam name="T">Type of value to read.</typeparam>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <param name="count">The number of elements to read.</param>
		/// <param name="read">The read function.</param>
		/// <param name="genPrefix">Label prefix to use when generating the arrays label, when none is found in the label dictionary.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The read array.</returns>
		public static LabeledArray<T> ReadLabeledArray<T>(
			this EndianStackReader reader,
			uint address,
			uint count,
			ReadValueAdvanceDelegate<T> read,
			string genPrefix,
			BaseLUT lut)
		{
			return lut.GetAddLabeledValue(address, genPrefix, () => new LabeledArray<T>(ReadArray(reader, address, count, read)));
		}

		/// <summary>
		/// Reads a labeled array of values off an endian stack reader.
		/// <br/> Checks if the collection is already in the LUT, and returns it if that is the case.
		/// </summary>
		/// <typeparam name="T">Type of value to read.</typeparam>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <param name="count">The number of elements to read.</param>
		/// <param name="elementByteSize">Number of bytes to advance the address after reading an element.</param>
		/// <param name="read">The read function.</param>
		/// <param name="genPrefix">Label prefix to use when generating the arrays label, when none is found in the label dictionary.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The read array.</returns>
		public static LabeledArray<T> ReadLabeledArray<T>(
			this EndianStackReader reader,
			uint address,
			uint count,
			uint elementByteSize,
			ReadValueDelegate<T> read,
			string genPrefix,
			BaseLUT lut)
		{
			return lut.GetAddLabeledValue(address, genPrefix, () => new LabeledArray<T>(ReadArray(reader, address, count, elementByteSize, read)));
		}

		/// <summary>
		/// Reads a labeled read only array of values off an endian stack reader.
		/// <br/> Checks if the collection is already in the LUT, and returns it if that is the case.
		/// </summary>
		/// <typeparam name="T">Type of value to read.</typeparam>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <param name="count">The number of elements to read.</param>
		/// <param name="read">The read function.</param>
		/// <param name="genPrefix">Label prefix to use when generating the arrays label, when none is found in the label dictionary.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The read array.</returns>
		public static LabeledReadOnlyArray<T> ReadLabeledReadOnlyArray<T>(
			this EndianStackReader reader,
			uint address,
			uint count,
			ReadValueAdvanceDelegate<T> read,
			string genPrefix,
			BaseLUT lut)
		{
			return lut.GetAddLabeledValue(address, genPrefix, () => new LabeledReadOnlyArray<T>(ReadArray(reader, address, count, read)));
		}

		/// <summary>
		/// Reads a labeled read only array of values off an endian stack reader.
		/// <br/> Checks if the collection is already in the LUT, and returns it if that is the case.
		/// </summary>
		/// <typeparam name="T">Type of value to read.</typeparam>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">The address at which to read.</param>
		/// <param name="count">The number of elements to read.</param>
		/// <param name="elementByteSize">Number of bytes to advance the address after reading an element.</param>
		/// <param name="read">The read function.</param>
		/// <param name="genPrefix">Label prefix to use when generating the arrays label, when none is found in the label dictionary.</param>
		/// <param name="lut">Pointer references to utilize.</param>
		/// <returns>The read array.</returns>
		public static LabeledReadOnlyArray<T> ReadLabeledReadOnlyArray<T>(
			this EndianStackReader reader,
			uint address,
			uint count,
			uint elementByteSize,
			ReadValueDelegate<T> read,
			string genPrefix,
			BaseLUT lut)
		{
			return lut.GetAddLabeledValue(address, genPrefix, () => new LabeledReadOnlyArray<T>(ReadArray(reader, address, count, elementByteSize, read)));
		}

		#endregion
	}
}
