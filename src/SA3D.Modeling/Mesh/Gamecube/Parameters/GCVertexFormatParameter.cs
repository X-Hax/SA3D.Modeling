using SA3D.Modeling.Mesh.Gamecube.Enums;

namespace SA3D.Modeling.Mesh.Gamecube.Parameters
{
	/// <summary>
	/// Parameter determining which types of vertex data is used by geometry.
	/// </summary>
	public struct GCVertexFormatParameter : IGCParameter
	{
		/// <inheritdoc/>
		public readonly GCParameterType Type => GCParameterType.VertexFormat;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// The attribute type that this parameter applies for.
		/// </summary>
		public GCVertexType VertexType
		{
			readonly get => (GCVertexType)(Data >> 16);
			set => Data = (Data & 0xFFFF) | ((uint)value << 16);
		}

		/// <summary>
		/// Vertex struct type being utilized.
		/// </summary>
		public GCStructType VertexStructType
		{
			readonly get => (GCStructType)((Data >> 12) & 0xF);
			set => Data = (Data & 0xF000) | (((uint)value) << 12);
		}

		/// <summary>
		/// Vertex data type being utilized.
		/// </summary>
		public GCDataType VertexDataType
		{
			readonly get => (GCDataType)((Data >> 8) & 0xF);
			set => Data = (Data & 0xF00) | (((uint)value) << 8);
		}

		/// <summary>
		/// Formatting info.
		/// </summary>
		public byte Formatting
		{
			readonly get => (byte)(Data & 0xFF);
			set => Data = (Data & 0xFFFFFF00) | value;
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Vertex Format: {VertexType} - {VertexStructType} - {VertexDataType} - {Formatting:X2}";
		}
	}
}
