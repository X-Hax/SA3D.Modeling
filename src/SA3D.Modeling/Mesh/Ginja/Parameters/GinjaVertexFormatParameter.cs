using SA3D.Modeling.Mesh.Ginja.Enums;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Parameter determining which types of vertex data is used by geometry.
	/// </summary>
	public struct GinjaVertexFormatParameter : IGinjaParameter
	{
		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.VertexFormat;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// The attribute type that this parameter applies for.
		/// </summary>
		public GinjaVertexType VertexType
		{
			readonly get => (GinjaVertexType)((Data >> 16) & 0xFF);
			set => Data = (Data & ~0xFF0000u) | ((uint)value << 16);
		}

		/// <summary>
		/// Vertex struct type being utilized.
		/// </summary>
		public GinjaStructType VertexStructType
		{
			readonly get => (GinjaStructType)((Data >> 12) & 0xF);
			set => Data = (Data & ~0xF000u) | (((uint)value) << 12);
		}

		/// <summary>
		/// Vertex data type being utilized.
		/// </summary>
		public GinjaDataType VertexDataType
		{
			readonly get => (GinjaDataType)((Data >> 8) & 0xF);
			set => Data = (Data & ~0xF00u) | (((uint)value) << 8);
		}

		/// <summary>
		/// Number of fractional bits in integer data
		/// </summary>
		public byte FractionalBitCount
		{
			readonly get => (byte)(Data & 0xFF);
			set => Data = (Data & ~0xFFu) | value;
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Vertex Format: {VertexType} - {VertexStructType} - {VertexDataType} - {FractionalBitCount:X2}";
		}
	}
}
