using SA3D.Modeling.Mesh.Gamecube.Enums;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.Mesh.Gamecube.Parameters
{
	/// <summary>
	/// Diffuse color of the geometry.
	/// </summary>
	public struct GCDiffuseColorParameter : IGCParameter
	{
		/// <summary>
		/// Diffuse color parameter with the color white.
		/// </summary>
		public static readonly GCDiffuseColorParameter White = new() { Data = uint.MaxValue };

		/// <inheritdoc/>
		public readonly GCParameterType Type => GCParameterType.DiffuseColor;

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
