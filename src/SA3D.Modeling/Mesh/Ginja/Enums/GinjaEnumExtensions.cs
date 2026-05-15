namespace SA3D.Modeling.Mesh.Gamecube.Enums
{
	/// <summary>
	/// Extension methods for gamecube enums.
	/// </summary>
	public static class GCEnumExtensions
	{
		/// <summary>
		/// Returns the struct size of a struct-data combination
		/// </summary>
		/// <param name="structType">Type of structure.</param>
		/// <param name="dataType">Value datatype within the structure</param>
		/// <returns>The size in bytes.</returns>
		public static uint GetStructSize(GCStructType structType, GCDataType dataType)
		{
			uint num_components = structType switch
			{
				GCStructType.PositionXY
				or GCStructType.TexCoordUV => 2,

				GCStructType.PositionXYZ
				or GCStructType.NormalXYZ => 3,

				GCStructType.NormalNBT
				or GCStructType.NormalNBT3 => 9,

				GCStructType.ColorRGB
				or GCStructType.ColorRGBA
				or GCStructType.TexCoordU
				or _ => 1,
			};

			return (uint)(num_components * dataType switch
			{
				GCDataType.Unsigned8
				or GCDataType.Signed8 => 1,

				GCDataType.Unsigned16
				or GCDataType.Signed16
				or GCDataType.RGB565
				or GCDataType.RGBA4 => 2,

				GCDataType.RGBA6 => 3,

				GCDataType.Float32
				or GCDataType.RGB8
				or GCDataType.RGBX8
				or GCDataType.RGBA8
				or _ => 4,
			});
		}
	}
}
