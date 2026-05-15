using Amicitia.IO.Binary;
using SA3D.Common;
using SA3D.Common.IO;
using System;
using System.Globalization;
using System.Numerics;

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
		/// Interpret (Radians) as a 16 Bit BAMS angle with 360° = 0x10000.
		/// </summary>
		BAMS16,

		/// <summary>
		/// Interpret (Radians) as 32 Bit BAMS angle with 360° = 0x10000.
		/// </summary>
		BAMS32,

		/// <summary>
		/// Interpret (Radians) as a 16 Bit BAMS angle with 360° = 0xFFFF.
		/// </summary>
		BAMSF16,

		/// <summary>
		/// Interpret (Radians) as 32 Bit BAMS angle with 360° = 0xFFFF.
		/// </summary>
		BAMSF32,

		/// <summary>
		/// Interpret as short and normalize( -32767~32767 -> -1~1 )
		/// </summary>
		NormalizedShort
	}

	/// <summary>
	/// Extension methods for <see cref="FloatIOType"/>
	/// </summary>
	public static class FloatIOTypeExtensions
	{
		private const float _normalizedShortRange = 32767;

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
				FloatIOType.BAMSF16 => 2,
				FloatIOType.BAMSF32 => 4,
				FloatIOType.NormalizedShort => 2,
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

			static string GetBAMSF16Text(float value)
			{
				return ((ushort)BAMSFHelper.RadToBAMSF(value)).ToCHex();
			}

			static string GetBAMSF32Text(float value)
			{
				return ((uint)BAMSFHelper.RadToBAMSF(value)).ToCHex();
			}

			static string GetNormalizedShortText(float value)
			{
				return ((short)(value / _normalizedShortRange)).ToString();
			}

			return type switch
			{
				FloatIOType.Short => GetShortText,
				FloatIOType.Float => GetText,
				FloatIOType.Integer => GetIntegerText,
				FloatIOType.BAMS16 => GetBAMS16Text,
				FloatIOType.BAMS32 => GetBAMS32Text,
				FloatIOType.BAMSF16 => GetBAMSF16Text,
				FloatIOType.BAMSF32 => GetBAMSF32Text,
				FloatIOType.NormalizedShort => GetNormalizedShortText,
				_ => throw new ArgumentException("Type invalid", nameof(type)),
			};
		}


		/// <summary>
		/// Returns a function for writing a <see cref="float"/> value to a <see cref="BinaryValueWriter"/> in the given <see cref="FloatIOType"/>.
		/// </summary>
		/// <param name="type">The type to write out as.</param>
		/// <returns>The function for writing a float value to a writer.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static Action<BinaryValueWriter, float> GetWriter(this FloatIOType type)
		{
			return type switch
			{
				FloatIOType.Float => (w, value) => w.WriteSingle(value),
				FloatIOType.Short => (w, value) => w.WriteInt16((short)MathF.Round(value)),
				FloatIOType.Integer => (w, value) => w.WriteInt32((int)MathF.Round(value)),
				FloatIOType.BAMS16 => (w, value) => w.WriteBAMS16(value),
				FloatIOType.BAMS32 => (w, value) => w.WriteBAMS32(value),
				FloatIOType.BAMSF16 => (w, value) => w.WriteBAMSF16(value),
				FloatIOType.BAMSF32 => (w, value) => w.WriteBAMSF32(value),
				FloatIOType.NormalizedShort => (w, value) => w.WriteInt16((short)(value * _normalizedShortRange)),
				_ => throw new ArgumentException("Type invalid", nameof(type))
			};
		}

		/// <summary>
		/// Returns a function for writing a <see cref="Vector2"/> value to a <see cref="BinaryValueWriter"/> in the given <see cref="FloatIOType"/>.
		/// </summary>
		/// <param name="type">The type to write out as.</param>
		/// <returns>The function for writing a float value to a writer.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static Action<BinaryValueWriter, Vector2> GetVector2Writer(this FloatIOType type)
		{
			return type switch
			{
				FloatIOType.Float => (w, value) =>
				{
					w.WriteSingle(value.X);
					w.WriteSingle(value.Y);
				}
				,
				FloatIOType.Short => (w, value) =>
				{
					w.WriteInt16((short)MathF.Round(value.X));
					w.WriteInt16((short)MathF.Round(value.Y));
				}
				,
				FloatIOType.Integer => (w, value) =>
				{
					w.WriteInt32((int)MathF.Round(value.X));
					w.WriteInt32((int)MathF.Round(value.Y));
				}
				,
				FloatIOType.BAMS16 => (w, value) =>
				{
					w.WriteBAMS16(value.X);
					w.WriteBAMS16(value.Y);
				}
				,
				FloatIOType.BAMS32 => (w, value) =>
				{
					w.WriteBAMS32(value.X);
					w.WriteBAMS32(value.Y);
				}
				,
				FloatIOType.BAMSF16 => (w, value) =>
				{
					w.WriteBAMSF16(value.X);
					w.WriteBAMSF16(value.Y);
				}
				,
				FloatIOType.BAMSF32 => (w, value) =>
				{
					w.WriteBAMSF32(value.X);
					w.WriteBAMSF32(value.Y);
				}
				,
				FloatIOType.NormalizedShort => (w, value) =>
				{
					w.WriteInt16((short)(value.X * _normalizedShortRange));
					w.WriteInt16((short)(value.Y * _normalizedShortRange));
				}
				,
				_ => throw new ArgumentException("Type invalid", nameof(type)),
			};
		}

		/// <summary>
		/// Returns a function for writing a <see cref="Vector3"/> value to a <see cref="BinaryValueWriter"/> in the given <see cref="FloatIOType"/>.
		/// </summary>
		/// <param name="type">The type to write out as.</param>
		/// <returns>The function for writing a float value to a writer.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static Action<BinaryValueWriter, Vector3> GetVector3Writer(this FloatIOType type)
		{
			return type switch
			{
				FloatIOType.Float => (w, value) =>
				{
					w.WriteSingle(value.X);
					w.WriteSingle(value.Y);
					w.WriteSingle(value.Z);
				}
				,
				FloatIOType.Short => (w, value) =>
				{
					w.WriteInt16((short)MathF.Round(value.X));
					w.WriteInt16((short)MathF.Round(value.Y));
					w.WriteInt16((short)MathF.Round(value.Z));
				}
				,
				FloatIOType.Integer => (w, value) =>
				{
					w.WriteInt32((int)MathF.Round(value.X));
					w.WriteInt32((int)MathF.Round(value.Y));
					w.WriteInt32((int)MathF.Round(value.Z));
				}
				,
				FloatIOType.BAMS16 => (w, value) =>
				{
					w.WriteBAMS16(value.X);
					w.WriteBAMS16(value.Y);
					w.WriteBAMS16(value.Z);
				}
				,
				FloatIOType.BAMS32 => (w, value) =>
				{
					w.WriteBAMS32(value.X);
					w.WriteBAMS32(value.Y);
					w.WriteBAMS32(value.Z);
				}
				,
				FloatIOType.BAMSF16 => (w, value) =>
				{
					w.WriteBAMSF16(value.X);
					w.WriteBAMSF16(value.Y);
					w.WriteBAMSF16(value.Z);
				}
				,
				FloatIOType.BAMSF32 => (w, value) =>
				{
					w.WriteBAMSF32(value.X);
					w.WriteBAMSF32(value.Y);
					w.WriteBAMSF32(value.Z);
				}
				,
				FloatIOType.NormalizedShort => (w, value) =>
				{
					w.WriteInt16((short)(value.X * _normalizedShortRange));
					w.WriteInt16((short)(value.Y * _normalizedShortRange));
					w.WriteInt16((short)(value.Z * _normalizedShortRange));
				}
				,
				_ => throw new ArgumentException("Type invalid", nameof(type)),
			};
		}


		/// <summary>
		/// Returns a function for reading a <see cref="float"/> value off a <see cref="BinaryValueReader"/> in the given <see cref="FloatIOType"/>.
		/// </summary>
		/// <param name="type">The type to read as.</param>
		/// <returns>The function for reading a float value off a reader.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static Func<BinaryValueReader, float> GetReader(this FloatIOType type)
		{
			return type switch
			{
				FloatIOType.Float => r => r.ReadSingle(),
				FloatIOType.Short => r => r.ReadInt16(),
				FloatIOType.Integer => r => r.ReadInt32(),
				FloatIOType.BAMS16 => r => r.ReadBAMS16(),
				FloatIOType.BAMS32 => r => r.ReadBAMS32(),
				FloatIOType.BAMSF16 => r => r.ReadBAMSF16(),
				FloatIOType.BAMSF32 => r => r.ReadBAMSF32(),
				FloatIOType.NormalizedShort => r => r.ReadInt16() / _normalizedShortRange,
				_ => throw new ArgumentException("Type invalid", nameof(type)),
			};
		}

		/// <summary>
		/// Returns a function for reading a <see cref="Vector2"/> value off a <see cref="BinaryValueReader"/> in the given <see cref="FloatIOType"/>.
		/// </summary>
		/// <param name="type">The type to read as.</param>
		/// <returns>The function for reading a float value off a reader.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static Func<BinaryValueReader, Vector2> GetVector2Reader(this FloatIOType type)
		{
			Func<BinaryValueReader, float> read = type.GetReader();
			return (r) => new(read(r), read(r));
		}

		/// <summary>
		/// Returns a function for reading a <see cref="Vector3"/> value off a <see cref="BinaryValueReader"/> in the given <see cref="FloatIOType"/>.
		/// </summary>
		/// <param name="type">The type to read as.</param>
		/// <returns>The function for reading a float value off a reader.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static Func<BinaryValueReader, Vector3> GetVector3Reader(this FloatIOType type)
		{
			Func<BinaryValueReader, float> read = type.GetReader();
			return (r) => new(read(r), read(r), read(r));
		}


		/// <summary>
		/// Writes a <see cref="float"/> value to a <see cref="BinaryValueWriter"/> in the given <see cref="FloatIOType"/>.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="type">The type to write out as.</param>
		/// <returns>The function for writing a float value to a writer.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static void WriteSingle(this BinaryValueWriter writer, float value, FloatIOType type)
		{
			switch(type)
			{
				case FloatIOType.Float:
					writer.WriteSingle(value);
					break;
				case FloatIOType.Short:
					writer.WriteInt16((short)MathF.Round(value));
					break;
				case FloatIOType.Integer:
					writer.WriteInt32((int)MathF.Round(value));
					break;
				case FloatIOType.BAMS16:
					writer.WriteBAMS16(value);
					break;
				case FloatIOType.BAMS32:
					writer.WriteBAMS32(value);
					break;
				case FloatIOType.BAMSF16:
					writer.WriteBAMSF16(value);
					break;
				case FloatIOType.BAMSF32:
					writer.WriteBAMSF32(value);
					break;
				case FloatIOType.NormalizedShort:
					writer.WriteInt16((short)(value * _normalizedShortRange));
					break;
				default:
					throw new ArgumentException("Type invalid", nameof(type));
			}
		}

		/// <summary>
		/// Writes a <see cref="Vector2"/> value to a <see cref="BinaryValueWriter"/> in the given <see cref="FloatIOType"/>.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="type">The type to write out as.</param>
		/// <returns>The function for writing a float value to a writer.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static void WriteVector2(this BinaryValueWriter writer, Vector2 value, FloatIOType type)
		{
			switch(type)
			{
				case FloatIOType.Float:
					writer.WriteSingle(value.X);
					writer.WriteSingle(value.Y);
					break;
				case FloatIOType.Short:
					writer.WriteInt16((short)MathF.Round(value.X));
					writer.WriteInt16((short)MathF.Round(value.Y));
					break;
				case FloatIOType.Integer:
					writer.WriteInt32((int)MathF.Round(value.X));
					writer.WriteInt32((int)MathF.Round(value.Y));
					break;
				case FloatIOType.BAMS16:
					writer.WriteBAMS16(value.X);
					writer.WriteBAMS16(value.Y);
					break;
				case FloatIOType.BAMS32:
					writer.WriteBAMS32(value.X);
					writer.WriteBAMS32(value.Y);
					break;
				case FloatIOType.BAMSF16:
					writer.WriteBAMSF16(value.X);
					writer.WriteBAMSF16(value.Y);
					break;
				case FloatIOType.BAMSF32:
					writer.WriteBAMSF32(value.X);
					writer.WriteBAMSF32(value.Y);
					break;
				case FloatIOType.NormalizedShort:
					writer.WriteInt16((short)(value.X * _normalizedShortRange));
					writer.WriteInt16((short)(value.Y * _normalizedShortRange));
					break;
				default:
					throw new ArgumentException("Type invalid", nameof(type));
			}
		}

		/// <summary>
		/// Writes a <see cref="Vector3"/> value to a <see cref="BinaryValueWriter"/> in the given <see cref="FloatIOType"/>.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="value">The value to write.</param>
		/// <param name="type">The type to write out as.</param>
		/// <returns>The function for writing a float value to a writer.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static void WriteVector3(this BinaryValueWriter writer, Vector3 value, FloatIOType type)
		{
			switch(type)
			{
				case FloatIOType.Float:
					writer.WriteSingle(value.X);
					writer.WriteSingle(value.Y);
					writer.WriteSingle(value.Z);
					break;
				case FloatIOType.Short:
					writer.WriteInt16((short)MathF.Round(value.X));
					writer.WriteInt16((short)MathF.Round(value.Y));
					writer.WriteInt16((short)MathF.Round(value.Z));
					break;
				case FloatIOType.Integer:
					writer.WriteInt32((int)MathF.Round(value.X));
					writer.WriteInt32((int)MathF.Round(value.Y));
					writer.WriteInt32((int)MathF.Round(value.Z));
					break;
				case FloatIOType.BAMS16:
					writer.WriteBAMS16(value.X);
					writer.WriteBAMS16(value.Y);
					writer.WriteBAMS16(value.Z);
					break;
				case FloatIOType.BAMS32:
					writer.WriteBAMS32(value.X);
					writer.WriteBAMS32(value.Y);
					writer.WriteBAMS32(value.Z);
					break;
				case FloatIOType.BAMSF16:
					writer.WriteBAMSF16(value.X);
					writer.WriteBAMSF16(value.Y);
					writer.WriteBAMSF16(value.Z);
					break;
				case FloatIOType.BAMSF32:
					writer.WriteBAMSF32(value.X);
					writer.WriteBAMSF32(value.Y);
					writer.WriteBAMSF32(value.Z);
					break;
				case FloatIOType.NormalizedShort:
					writer.WriteInt16((short)(value.X * _normalizedShortRange));
					writer.WriteInt16((short)(value.Y * _normalizedShortRange));
					writer.WriteInt16((short)(value.Z * _normalizedShortRange));
					break;
				default:
					throw new ArgumentException("Type invalid", nameof(type));
			}
		}


		/// <summary>
		/// Reads a <see cref="float"/> value off a <see cref="BinaryValueReader"/> in the given <see cref="FloatIOType"/>.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="type">The type to read as.</param>
		/// <returns>The function for reading a float value off a reader.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static float ReadSingle(this BinaryValueReader reader, FloatIOType type)
		{
			return type switch
			{
				FloatIOType.Float => reader.ReadSingle(),
				FloatIOType.Short => reader.ReadInt16(),
				FloatIOType.Integer => reader.ReadInt32(),
				FloatIOType.BAMS16 => reader.ReadBAMS16(),
				FloatIOType.BAMS32 => reader.ReadBAMS32(),
				FloatIOType.BAMSF16 => reader.ReadBAMSF16(),
				FloatIOType.BAMSF32 => reader.ReadBAMSF32(),
				FloatIOType.NormalizedShort => reader.ReadInt16() / _normalizedShortRange,
				_ => throw new ArgumentException("Type invalid", nameof(type)),
			};
		}

		/// <summary>
		/// Reads a <see cref="Vector2"/> value off a <see cref="BinaryValueReader"/> in the given <see cref="FloatIOType"/>.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="type">The type to read as.</param>
		/// <returns>The function for reading a float value off a reader.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static Vector2 ReadVector2(this BinaryValueReader reader, FloatIOType type)
		{
			return type switch
			{
				FloatIOType.Float => new(reader.ReadSingle(), reader.ReadSingle()),
				FloatIOType.Short => new(reader.ReadInt16(), reader.ReadInt16()),
				FloatIOType.Integer => new(reader.ReadInt32(), reader.ReadInt32()),
				FloatIOType.BAMS16 => new(reader.ReadBAMS16(), reader.ReadBAMS16()),
				FloatIOType.BAMS32 => new(reader.ReadBAMS32(), reader.ReadBAMS32()),
				FloatIOType.BAMSF16 => new(reader.ReadBAMSF16(), reader.ReadBAMSF16()),
				FloatIOType.BAMSF32 => new(reader.ReadBAMSF32(), reader.ReadBAMSF32()),
				FloatIOType.NormalizedShort => new(reader.ReadInt16() / _normalizedShortRange, reader.ReadInt16() / _normalizedShortRange),
				_ => throw new ArgumentException("Type invalid", nameof(type)),
			};
		}

		/// <summary>
		/// Reads a <see cref="Vector3"/> value off a <see cref="BinaryValueReader"/> in the given <see cref="FloatIOType"/>.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="type">The type to read as.</param>
		/// <returns>The function for reading a float value off a reader.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static Vector3 ReadVector3(this BinaryValueReader reader, FloatIOType type)
		{
			return type switch
			{
				FloatIOType.Float => new(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
				FloatIOType.Short => new(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16()),
				FloatIOType.Integer => new(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32()),
				FloatIOType.BAMS16 => new(reader.ReadBAMS16(), reader.ReadBAMS16(), reader.ReadBAMS16()),
				FloatIOType.BAMS32 => new(reader.ReadBAMS32(), reader.ReadBAMS32(), reader.ReadBAMS32()),
				FloatIOType.BAMSF16 => new(reader.ReadBAMSF16(), reader.ReadBAMSF16(), reader.ReadBAMSF16()),
				FloatIOType.BAMSF32 => new(reader.ReadBAMSF32(), reader.ReadBAMSF32(), reader.ReadBAMSF32()),
				FloatIOType.NormalizedShort => new(reader.ReadInt16() / _normalizedShortRange, reader.ReadInt16() / _normalizedShortRange, reader.ReadInt16() / _normalizedShortRange),
				_ => throw new ArgumentException("Type invalid", nameof(type)),
			};
		}


	}
}
