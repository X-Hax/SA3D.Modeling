using System;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// RGBA Color value.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public partial struct Color : IEquatable<Color>
	{
		#region Constants

		/// <summary>
		/// White color; #FFFFFFFF
		/// </summary>
		public static readonly Color ColorWhite
			= new(0xFF, 0xFF, 0xFF, 0xFF);

		/// <summary>
		/// Black color; #000000FF
		/// </summary>
		public static readonly Color ColorBlack
			= new(0x00, 0x00, 0x00, 0xFF);

		/// <summary>
		/// Red color; #FF0000FF
		/// </summary>
		public static readonly Color ColorRed
			= new(0xFF, 0x00, 0x00, 0xFF);

		/// <summary>
		/// Green color; 0#0FF00FF
		/// </summary>
		public static readonly Color ColorGreen
			= new(0x00, 0xFF, 0x00, 0xFF);

		/// <summary>
		/// Blue color; #0000FFFF
		/// </summary>
		public static readonly Color ColorBlue
			= new(0x00, 0x00, 0xFF, 0xFF);

		/// <summary>
		/// Transparent color; #00000000
		/// </summary>
		public static readonly Color ColorTransparent
			= default;

		[GeneratedRegex("^[0-9A-F]+$")]
		private static partial Regex HexNumRegex();

		#endregion

		/// <summary>
		/// Red.
		/// </summary>
		public byte Red { get; set; }

		/// <summary>
		/// Green.
		/// </summary>
		public byte Green { get; set; }

		/// <summary>
		/// Blue.
		/// </summary>
		public byte Blue { get; set; }

		/// <summary>
		/// Alpha/Transparency.
		/// </summary>
		public byte Alpha { get; set; }

		#region Converter Properties

		/// <summary>
		/// Red as a float. Ranges from 0 - 1.
		/// </summary>
		public float RedF
		{
			readonly get => Red / 255.0f;
			set => Red = FromFloat(value);
		}

		/// <summary>
		/// Green as a float. Ranges from 0 - 1.
		/// </summary>
		public float GreenF
		{
			readonly get => Green / 255.0f;
			set => Green = FromFloat(value);
		}

		/// <summary>
		/// Blue as a float. Ranges from 0 - 1.
		/// </summary>
		public float BlueF
		{
			readonly get => Blue / 255.0f;
			set => Blue = FromFloat(value);
		}

		/// <summary>
		/// Alpha as a float. Ranges from 0 - 1.
		/// </summary>
		public float AlphaF
		{
			readonly get => Alpha / 255.0f;
			set => Alpha = FromFloat(value);
		}

		/// <summary>
		/// The color as a 4 component float vector.
		/// </summary>
		public Vector4 FloatVector
		{
			readonly get => new(RedF, GreenF, BlueF, AlphaF);
			set
			{
				RedF = value.X;
				BlueF = value.Y;
				GreenF = value.Z;
				AlphaF = value.W;
			}
		}

		/// <summary>
		/// RGBA representation of the color (32 bits).
		/// </summary>
		public uint RGBA
		{
			readonly get => (uint)(Alpha | (Blue << 8) | (Green << 16) | (Red << 24));
			set
			{
				Alpha = (byte)(value & 0xFF);
				Blue = (byte)((value >> 8) & 0xFF);
				Green = (byte)((value >> 16) & 0xFF);
				Red = (byte)(value >> 24);
			}
		}

		/// <summary>
		/// ARGB representation of the color (32 bits).
		/// </summary>
		public uint ARGB
		{
			readonly get => (uint)(Blue | (Green << 8) | (Red << 16) | (Alpha << 24));
			set
			{
				Blue = (byte)(value & 0xFF);
				Green = (byte)((value >> 8) & 0xFF);
				Red = (byte)((value >> 16) & 0xFF);
				Alpha = (byte)(value >> 24);
			}
		}

		/// <summary>
		/// ARGB representation of the color (16 bits).
		/// </summary>
		public ushort ARGB4
		{
			readonly get => (ushort)((Blue >> 4) | (Green & 0xF) | ((Red << 4) & 0xF) | ((Alpha << 8) & 0xF));
			set
			{
				Blue = (byte)((value << 4) & 0xF0);
				Blue |= (byte)(Blue >> 4);

				Green = (byte)(value & 0xF0);
				Green |= (byte)(Green >> 4);

				Red = (byte)((value >> 4) & 0xF0);
				Red |= (byte)(Red >> 4);

				Alpha = (byte)((value >> 8) & 0xF0);
				Alpha |= (byte)(Alpha >> 4);
			}
		}

		/// <summary>
		/// RGB565 representation of the color (16 bits).
		/// </summary>
		public ushort RGB565
		{
			readonly get => (ushort)((Blue >> 3) | ((Green << 3) & 0x3F) | ((Red << 8) & 0x1F));
			set
			{
				Blue = (byte)(((value << 3) | (value >> 2)) & 0xFFu);
				Green = (byte)(((value >> 3) | (value >> 9)) & 0xFFu);
				Red = (byte)(((value >> 8) | (value >> 13)) & 0xFFu);
				Alpha = 0xFF;
			}
		}

		/// <summary>
		/// System.Drawing representation of the color.
		/// </summary>
		public System.Drawing.Color SystemColor
		{
			readonly get => System.Drawing.Color.FromArgb(Alpha, Red, Green, Blue);
			set
			{
				Red = value.R;
				Green = value.G;
				Blue = value.B;
				Alpha = value.A;
			}
		}

		/// <summary>
		/// RGBA string - #RRGGBBAA <br/>
		/// setter formats: <br/>
		/// #RRGGBBAA <br/>
		/// #RRGGBB <br/>
		/// #RGBA <br/>
		/// #RGB <br/>
		/// (The # is optional for all formats)
		/// </summary>
		public string Hex
		{
			readonly get => $"#{Red:X2}{Green:X2}{Blue:X2}{Alpha:X2}";
			set
			{
				string hex = value.Replace(" ", "");
				hex = hex.StartsWith("#") ? hex[1..] : hex;
				hex = hex.ToUpper();

				// check if the format is valid
				if(!HexNumRegex().IsMatch(hex))
				{
					throw new FormatException("Invalid Color format!");
				}

				static byte conv(char character)
				{
					return (byte)(character >= 'A' ? character - 'A' + 10 : character - '0');
				}

				static byte comp(byte lo, byte hi)
				{
					return (byte)(lo | (hi << 4));
				}

				switch(hex.Length)
				{
					case 3: // RGB
						Red = conv(hex[0]);
						Green = conv(hex[1]);
						Blue = conv(hex[2]);
						Alpha = byte.MaxValue;

						Red = comp(Red, Red);
						Green = comp(Green, Green);
						Blue = comp(Blue, Blue);
						break;
					case 4: // RGBA
						Red = conv(hex[0]);
						Green = conv(hex[1]);
						Blue = conv(hex[2]);
						Alpha = conv(hex[3]);

						Red = comp(Red, Red);
						Green = comp(Green, Green);
						Blue = comp(Blue, Blue);
						Alpha = comp(Alpha, Alpha);
						break;
					case 6: // RRGGBB
						Red = comp(conv(hex[1]), conv(hex[0]));
						Green = comp(conv(hex[3]), conv(hex[2]));
						Blue = comp(conv(hex[5]), conv(hex[4]));
						Alpha = byte.MaxValue;
						break;
					case 8: // RRGGBBAA
						Red = comp(conv(hex[1]), conv(hex[0]));
						Green = comp(conv(hex[3]), conv(hex[2]));
						Blue = comp(conv(hex[5]), conv(hex[4]));
						Alpha = comp(conv(hex[7]), conv(hex[6]));
						break;
					default:
						throw new FormatException("Invalid Color format!");
				}
			}
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Creates a new color.
		/// </summary>
		/// <param name="red">Red color.</param>
		/// <param name="green">Green color.</param>
		/// <param name="blue">Blue color.</param>
		/// <param name="alpha">alpha color.</param>
		public Color(byte red, byte green, byte blue, byte alpha)
		{
			Red = red;
			Green = green;
			Blue = blue;
			Alpha = alpha;
		}

		/// <summary>
		/// Creates a new opaque color.
		/// </summary>
		/// <param name="red">Red color.</param>
		/// <param name="green">Green color.</param>
		/// <param name="blue">Blue color.</param>
		public Color(byte red, byte green, byte blue) : this(red, green, blue, 0xFF) { }

		/// <summary>
		/// Creates a new color from floating point values.
		/// </summary>
		/// <param name="red">Red color.</param>
		/// <param name="green">Green color.</param>
		/// <param name="blue">Blue color.</param>
		/// <param name="alpha">alpha color.</param>
		public Color(float red, float green, float blue, float alpha) : this()
		{
			RedF = red;
			GreenF = green;
			BlueF = blue;
			AlphaF = alpha;
		}

		/// <summary>
		/// Creates a new opaque color from floating point values.
		/// </summary>
		/// <param name="red">Red color.</param>
		/// <param name="green">Green color.</param>
		/// <param name="blue">Blue color.</param>
		public Color(float red, float green, float blue) : this(red, green, blue, 1) { }

		/// <summary>
		/// Creates a new color from a 4 component floating point vector.
		/// </summary>
		/// <param name="vector">Color value vector.</param>
		public Color(Vector4 vector) : this(vector.X, vector.Y, vector.Z, vector.W) { }

		#endregion

		#region Arithmetic Operators/Methods

		/// <summary>
		/// Calculates the colors luminance value.
		/// </summary>
		public readonly float GetLuminance()
		{
			return (0.2126f * RedF) + (0.7152f * GreenF) + (0.0722f * BlueF);
		}

		/// <summary>
		/// Linearly interpolates two colors.
		/// </summary>
		/// <param name="from">The color from which to interpolate.</param>
		/// <param name="to">The color to which to interpolate.</param>
		/// <param name="t">Time value to interpolate with. Range 0 - 1.</param>
		/// <returns>The interpolated color.</returns>
		public static Color Lerp(Color from, Color to, float t)
		{
			float inverse = 1 - t;
			return new(
				(to.RedF * t) + (from.RedF * inverse),
				(to.GreenF * t) + (from.GreenF * inverse),
				(to.BlueF * t) + (from.BlueF * inverse),
				(to.AlphaF * t) + (from.AlphaF * inverse));
		}

		/// <summary>
		/// Calculates the vector distance between two colors (Calculates with floating point values).
		/// </summary>
		/// <param name="from">First color.</param>
		/// <param name="to">Second color.</param>
		/// <returns>The distance.</returns>
		public static float Distance(Color from, Color to)
		{
			return MathF.Sqrt(
				MathF.Pow(from.RedF - to.RedF, 2) +
				MathF.Pow(from.GreenF - to.GreenF, 2) +
				MathF.Pow(from.BlueF - to.BlueF, 2) +
				MathF.Pow(from.AlphaF - to.AlphaF, 2)
				);
		}

		/// <summary>
		/// Adds the individual color components of two colors together (calculates with bytes).
		/// </summary>
		/// <param name="l">Lefthand color.</param>
		/// <param name="r">Righthand color.</param>
		/// <returns>The calculated color.</returns>
		public static Color operator +(Color l, Color r)
		{
			return new()
			{
				Red = (byte)Math.Min(l.Red + r.Red, byte.MaxValue),
				Green = (byte)Math.Min(l.Green + r.Green, byte.MaxValue),
				Blue = (byte)Math.Min(l.Blue + r.Blue, byte.MaxValue),
				Alpha = (byte)Math.Min(l.Alpha + r.Alpha, byte.MaxValue)
			};
		}

		/// <summary>
		/// Subtracts the individual color compoenents of two colors. (calculates with bytes).
		/// </summary>
		/// <param name="l">The color to subtract from</param>
		/// <param name="r">The color to subtract.</param>
		/// <returns>The calculated color.</returns>
		public static Color operator -(Color l, Color r)
		{
			return new()
			{
				Red = (byte)Math.Max(l.Red - r.Red, 0),
				Green = (byte)Math.Max(l.Green - r.Green, 0),
				Blue = (byte)Math.Max(l.Blue - r.Blue, 0),
				Alpha = (byte)Math.Max(l.Alpha - r.Alpha, 0)
			};
		}

		/// <summary>
		/// Multiplies the individual color components by a value (calculates with floats).
		/// </summary>
		/// <param name="l">The color to multiply.</param>
		/// <param name="r">The value to multiply by</param>
		/// <returns>The calculated color.</returns>
		public static Color operator *(Color l, float r)
		{
			return new()
			{
				RedF = Math.Clamp(l.RedF * r, 0, 1),
				GreenF = Math.Clamp(l.GreenF * r, 0, 1),
				BlueF = Math.Clamp(l.BlueF * r, 0, 1),
				AlphaF = Math.Clamp(l.AlphaF * r, 0, 1)
			};
		}

		/// <summary>
		/// Multiplies the individual color components by a value (calculates with floats).
		/// </summary>
		/// <param name="l">The value to multiply by</param>
		/// <param name="r">The color to multiply.</param>
		/// <returns>The calculated color.</returns>
		public static Color operator *(float l, Color r)
		{
			return r * l;
		}

		/// <summary>
		/// Divides the individual color components by a value (calculates with floats).
		/// </summary>
		/// <param name="l">The color to divide.</param>
		/// <param name="r">The value to divide by.</param>
		/// <returns>The calculated color.</returns>
		public static Color operator /(Color l, float r)
		{
			return l * (1f / r);
		}

		#endregion

		#region Logical Operators/Methods

		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is Color color &&
				   Red == color.Red &&
				   Green == color.Green &&
				   Blue == color.Blue &&
				   Alpha == color.Alpha;
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(Red, Green, Blue, Alpha);
		}

		/// <summary>
		/// Compares the components of 2 colors for equality.
		/// </summary>
		/// <param name="l">Lefthand color.</param>
		/// <param name="r">Righthand color.</param>
		/// <returns>Whether the colors are equal.</returns>
		public static bool operator ==(Color l, Color r)
		{
			return l.Equals(r);
		}

		/// <summary>
		/// Compares the components of 2 colors for inequality.
		/// </summary>
		/// <param name="l">Lefthand color.</param>
		/// <param name="r">Righthand color.</param>
		/// <returns>Whether the colors are inequal.</returns>
		public static bool operator !=(Color l, Color r)
		{
			return !l.Equals(r);
		}

		readonly bool IEquatable<Color>.Equals(Color other)
		{
			return Equals(other);
		}

		#endregion


		private static byte FromFloat(float value)
		{
			return (byte)(float.Clamp(value, 0, 1) * 255);
		}

		/// <summary>
		/// Returns a hexadecimal string represntation of the color.
		/// </summary>
		/// <returns>The hexadecimal string.</returns>
		public override readonly string ToString()
		{
			return Hex;
		}
	}
}
