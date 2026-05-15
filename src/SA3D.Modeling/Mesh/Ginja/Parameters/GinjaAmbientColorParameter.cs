using SA3D.Modeling.Mesh.Gamecube.Enums;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.Mesh.Gamecube.Parameters
{
	/// <summary>
	/// Ambient color of the geometry.
	/// </summary>
	public struct GCAmbientColorParameter : IGCParameter
	{
		/// <summary>
		/// Ambient color parameter with the color white.
		/// </summary>
		public static readonly GCAmbientColorParameter White = new() { Data = uint.MaxValue };

		/// <inheritdoc/>
		public readonly GCParameterType Type => GCParameterType.AmbientColor;

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
