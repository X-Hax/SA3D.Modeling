using SA3D.Modeling.Mesh.Gamecube.Enums;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.Mesh.Gamecube.Parameters
{
	/// <summary>
	/// Specular color of the geometry.
	/// </summary>
	public struct GCSpecularColorParameter : IGCParameter
	{
		/// <summary>
		/// Specular color parameter with the color white.
		/// </summary>
		public static readonly GCSpecularColorParameter White = new() { Data = uint.MaxValue };

		/// <inheritdoc/>
		public readonly GCParameterType Type => GCParameterType.SpecularColor;

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
