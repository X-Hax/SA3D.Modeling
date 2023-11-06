using SA3D.Common;
using SA3D.Common.IO;
using System;
using System.Globalization;

namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// Write/Read mode for float values.
	/// </summary>
	public enum FloatIOType
	{
		/// <summary>
		/// Interpret as is.
		/// </summary>
		Float,

		/// <summary>
		/// Interpret float as a short.
		/// </summary>
		Short,

		/// <summary>
		/// Interpret float as an integer.
		/// </summary>
		Integer,

		/// <summary>
		/// Interpret (Radians) as a 16 Bit BAMS angle.
		/// </summary>
		BAMS16,

		/// <summary>
		/// Interpret (Radians) as 32 Bit BAMS angle.
		/// </summary>
		BAMS32
	}

	/// <summary>
	/// Extension methods for <see cref="FloatIOType"/>
	/// </summary>
	public static class FloatIOTypeExtensions
	{
		/// <summary>
		/// Returns the number of bytes the given type takes up.
		/// </summary>
		/// <param name="type">Type to get the size of.</param>
		/// <returns>The value size.</returns>
		public static int GetByteSize(this FloatIOType type)
		{
			return type switch
			{
				FloatIOType.Float => 4,
				FloatIOType.Short => 2,
				FloatIOType.Integer => 4,
				FloatIOType.BAMS16 => 2,
				FloatIOType.BAMS32 => 4,
				_ => throw new ArgumentException("Type invalid.", nameof(type)),
			};
		}

		/// <summary>
		/// Returns a function for converting a <see cref="float"/> value to a string in the given IO type.
		/// </summary>
		/// <param name="type">The type to print out as.</param>
		/// <returns>The function for converting float values.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static Func<float, string> GetPrinter(this FloatIOType type)
		{
			static string GetText(float value)
			{
				return value.ToString("F5", CultureInfo.InvariantCulture) + "f";
			}

			static string GetShortText(float value)
			{
				return ((short)MathF.Round(value)).ToString();
			}

			static string GetIntegerText(float value)
			{
				return ((int)MathF.Round(value)).ToString();
			}

			static string GetBAMS16Text(float value)
			{
				return ((ushort)MathHelper.RadToBAMS(value)).ToCHex();
			}

			static string GetBAMS32Text(float value)
			{
				return ((uint)MathHelper.RadToBAMS(value)).ToCHex();
			}

			return type switch
			{
				FloatIOType.Short => GetShortText,
				FloatIOType.Float => GetText,
				FloatIOType.Integer => GetIntegerText,
				FloatIOType.BAMS16 => GetBAMS16Text,
				FloatIOType.BAMS32 => GetBAMS32Text,
				_ => throw new ArgumentException("Type invalid", nameof(type)),
			};
		}

		/// <summary>
		/// Returns a function for writing a <see cref="float"/> value to an endian stack writer in the given IO type.
		/// </summary>
		/// <param name="type">The type to write out as.</param>
		/// <returns>The function for writing a float value to a writer.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static Action<EndianStackWriter, float> GetWriter(this FloatIOType type)
		{
			static void WriteFloat(EndianStackWriter writer, float value)
			{
				writer.WriteFloat(value);
			}

			static void WriteShort(EndianStackWriter writer, float value)
			{
				writer.WriteShort((short)MathF.Round(value));
			}

			static void WriteInteger(EndianStackWriter writer, float value)
			{
				writer.WriteInt((int)MathF.Round(value));
			}

			static void WriteBAMS16(EndianStackWriter writer, float value)
			{
				writer.WriteShort((short)MathHelper.RadToBAMS(value));
			}

			static void WriteBAMS32(EndianStackWriter writer, float value)
			{
				writer.WriteInt(MathHelper.RadToBAMS(value));
			}

			return type switch
			{
				FloatIOType.Float => WriteFloat,
				FloatIOType.Short => WriteShort,
				FloatIOType.Integer => WriteInteger,
				FloatIOType.BAMS16 => WriteBAMS16,
				FloatIOType.BAMS32 => WriteBAMS32,
				_ => throw new ArgumentException("Type invalid", nameof(type)),
			};
		}

		/// <summary>
		/// Returns a function for reading a <see cref="float"/> value off an endian stack reader in the given IO type.
		/// </summary>
		/// <param name="type">The type to read as.</param>
		/// <returns>The function for reading a float value off a reader.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static Func<EndianStackReader, uint, float> GetReader(this FloatIOType type)
		{
			static float ReadFloat(EndianStackReader reader, uint address)
			{
				return reader.ReadFloat(address);
			}

			static float ReadShort(EndianStackReader reader, uint address)
			{
				return reader.ReadShort(address);
			}

			static float ReadInteger(EndianStackReader reader, uint address)
			{
				return reader.ReadInt(address);
			}

			static float ReadBAMS16(EndianStackReader reader, uint address)
			{
				return MathHelper.BAMSToRad(reader.ReadShort(address));
			}

			static float ReadBAMS32(EndianStackReader reader, uint address)
			{
				return MathHelper.BAMSToRad(reader.ReadInt(address));
			}

			return type switch
			{
				FloatIOType.Float => ReadFloat,
				FloatIOType.Short => ReadShort,
				FloatIOType.Integer => ReadInteger,
				FloatIOType.BAMS16 => ReadBAMS16,
				FloatIOType.BAMS32 => ReadBAMS32,
				_ => throw new ArgumentException("Type invalid", nameof(type)),
			};
		}
	}
}
