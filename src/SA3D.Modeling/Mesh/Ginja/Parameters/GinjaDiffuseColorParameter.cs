using SA3D.Modeling.Mesh.Ginja.Enums;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Diffuse color of the geometry.
	/// </summary>
	public struct GinjaDiffuseColorParameter : IGinjaParameter
	{
		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.DiffuseColor;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Diffuse color of the mesh.
		/// </summary>
		public Color DiffuseColor
		{
			get => new() { RGBA = Data };
			set => Data = value.RGBA;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Diffuse color: {DiffuseColor}";
		}
	}
}
