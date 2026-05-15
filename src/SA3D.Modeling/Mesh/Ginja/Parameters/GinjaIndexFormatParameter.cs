using SA3D.Modeling.Mesh.Ginja.Enums;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Holds information about the triangle lists of geometry.
	/// </summary>
	public struct GinjaIndexFormatParameter : IGinjaParameter
	{
		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.IndexFormat;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Holds information about the triangle lists of geometry.
		/// </summary>
		public GinjaIndexFormat IndexFormat
		{
			readonly get => (GinjaIndexFormat)Data;
			set => Data = (uint)value;
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Index Format: {(uint)IndexFormat}";
		}
	}
}
