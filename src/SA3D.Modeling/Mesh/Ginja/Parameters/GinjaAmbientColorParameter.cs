using SA3D.Modeling.Mesh.Ginja.Enums;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Ambient color of the geometry.
	/// </summary>
	public struct GinjaAmbientColorParameter : IGinjaParameter
	{
		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.AmbientColor;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Ambient color of the mesh.
		/// </summary>
		public Color AmbientColor
		{
			get => new() { RGBA = Data };
			set => Data = value.RGBA;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Ambient color: {AmbientColor}";
		}
	}
}
