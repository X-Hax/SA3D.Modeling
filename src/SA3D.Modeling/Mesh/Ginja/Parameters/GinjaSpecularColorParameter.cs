using SA3D.Modeling.Mesh.Ginja.Enums;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Specular color of the geometry.
	/// </summary>
	public struct GinjaSpecularColorParameter : IGinjaParameter
	{
		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.SpecularColor;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Specular color of the mesh.
		/// </summary>
		public Color SpecularColor
		{
			get => new() { RGBA = Data };
			set => Data = value.RGBA;
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Specular color: {SpecularColor}";
		}
	}
}
