using SA3D.Modeling.Structs;
using System;

namespace SA3D.Modeling.Mesh.Ginja.Enums
{
	/// <summary>
	/// Extension methods for gamecube enums.
	/// </summary>
	public static class GinjaEnumExtensions
	{
		/// <summary>
		/// Returns the number of components for the given <see cref="GinjaStructType"/>
		/// </summary>
		/// <param name="structType">The type to get the number of components for</param>
		public static int GetStructComponentCount(this GinjaStructType structType)
		{
			return structType switch
			{
				GinjaStructType.PositionXY
				or GinjaStructType.TexCoordUV => 2,

				GinjaStructType.PositionXYZ
				or GinjaStructType.NormalXYZ => 3,

				GinjaStructType.NormalNBT
				or GinjaStructType.NormalNBT3 => 9,

				GinjaStructType.ColorRGB
				or GinjaStructType.ColorRGBA
				or GinjaStructType.TexCoordU => 1,

				_ => throw new InvalidOperationException($"Invalid struct type {structType}"),
			};
		}

		/// <summary>
		/// Returns the number of bytes occupied by a given <see cref="GinjaDataType"/>
		/// </summary>
		/// <param name="dataType">The datatype to get the bytesize for</param>
		public static int GetDataByteSize(this GinjaDataType dataType)
		{
			return dataType switch
			{
				GinjaDataType.Unsigned8
				or GinjaDataType.Signed8 => 1,

				GinjaDataType.Unsigned16
				or GinjaDataType.Signed16
				or GinjaDataType.RGB565
				or GinjaDataType.RGBA4 => 2,

				GinjaDataType.RGBA6 => 3,

				GinjaDataType.Float32
				or GinjaDataType.RGB8
				or GinjaDataType.RGBX8
				or GinjaDataType.RGBA8 => 4,

				_ => throw new InvalidOperationException($"Invalid data type {dataType}"),
			};
		}

		/// <summary>
		/// Returns the <see cref="Type"/> for the given <see cref="GinjaDataType"/>
		/// </summary>
		/// <param name="dataType">The datatype to get the reflection type for</param>
		public static Type GetDataReflectionType(this GinjaDataType dataType)
		{
			return dataType switch
			{
				GinjaDataType.Unsigned8 => typeof(byte),
				GinjaDataType.Signed8 => typeof(sbyte),
				GinjaDataType.Unsigned16 => typeof(ushort),
				GinjaDataType.Signed16 => typeof(short),
				GinjaDataType.Float32 => typeof(float),

				GinjaDataType.RGB565
				or GinjaDataType.RGB8
				or GinjaDataType.RGBX8
				or GinjaDataType.RGBA4
				or GinjaDataType.RGBA6
				or GinjaDataType.RGBA8 => typeof(Color),

				_ => throw new InvalidOperationException($"Invalid data type {dataType}"),
			};
		}
	}
}
