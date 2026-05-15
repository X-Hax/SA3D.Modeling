using SA3D.Modeling.Mesh.Gamecube.Enums;

namespace SA3D.Modeling.Mesh.Gamecube.Parameters
{
	/// <summary>
	/// Holds information about the triangle lists of geometry.
	/// </summary>
	public struct GCIndexFormatParameter : IGCParameter
	{
		/// <inheritdoc/>
		public readonly GCParameterType Type => GCParameterType.IndexFormat;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Holds information about the triangle lists of geometry.
		/// </summary>
		public GCIndexFormat IndexFormat
		{
			readonly get => (GCIndexFormat)Data;
			set => Data = (uint)value;
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Index Format: {(uint)IndexFormat}";
		}
	}
}
