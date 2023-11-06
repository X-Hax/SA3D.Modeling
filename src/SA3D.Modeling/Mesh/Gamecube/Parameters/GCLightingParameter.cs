using SA3D.Modeling.Mesh.Gamecube.Enums;

namespace SA3D.Modeling.Mesh.Gamecube.Parameters
{
	/// <summary>
	/// Holds lighting information
	/// </summary>
	public struct GCLightingParameter : IGCParameter
	{
		/// <summary>
		/// Default lighting parameter values for geometry using vertex colors.
		/// </summary>
		public static ushort DefaultColorParam = 0xB11;

		/// <summary>
		/// Default lighting parameter values for geometry using normals (shading).
		/// </summary>
		public static ushort DefaultNormalParam = 0x211;

		/// <inheritdoc/>
		public readonly GCParameterType Type => GCParameterType.Lighting;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// Lighting attributes. Pretty much unknown how they work.
		/// </summary>
		public ushort LightingAttributes
		{
			readonly get => (ushort)(Data & 0xFFFF);
			set => Data = (Data & 0xFFFF0000) | value;
		}

		/// <summary>
		/// Which shadow stencil the geometry should use. (?) 
		/// <br/> Ranges from 0 - 15.
		/// </summary>
		public byte ShadowStencil
		{
			readonly get => (byte)((Data >> 16) & 0xF);
			set => Data = (Data & 0xFFF0FFFF) | (uint)((value & 0xF) << 16);
		}

		/// <summary>
		/// Unknown functionality.
		/// </summary>
		public byte Unknown1
		{
			readonly get => (byte)((Data >> 20) & 0xF);
			set => Data = (Data & 0xFF0FFFFF) | (uint)((value & 0xF) << 20);
		}

		/// <summary>
		/// Unknown functionality.
		/// </summary>
		public byte Unknown2
		{
			readonly get => (byte)(Data >> 24);
			set => Data = (Data & 0x00FFFFFF) | (uint)(value << 24);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Lighting: {LightingAttributes} - {ShadowStencil} - {Unknown1} - {Unknown2}";
		}
	}
}
