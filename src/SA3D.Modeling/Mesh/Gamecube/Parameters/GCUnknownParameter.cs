using SA3D.Modeling.Mesh.Gamecube.Enums;

namespace SA3D.Modeling.Mesh.Gamecube.Parameters
{
	/// <summary>
	/// Unknown but not unused.
	/// </summary>
	public struct GCUnknownParameter : IGCParameter
	{
		/// <summary>
		/// Parameter with default values.
		/// </summary>
		public static readonly GCUnknownParameter DefaultValues = new() { Unknown1 = 4 };

		/// <inheritdoc/>
		public readonly GCParameterType Type => GCParameterType.Unknown;

		/// <inheritdoc/>
		public uint Data { get; set; }

		/// <summary>
		/// No idea what this does.
		/// </summary>
		public ushort Unknown1
		{
			readonly get => (ushort)(Data & 0xFFFF);
			set => Data = (Data & 0xFFFF0000) | value;
		}

		/// <summary>
		/// No idea what this does.
		/// </summary>
		public ushort Unknown2
		{
			readonly get => (ushort)(Data >> 16);
			set => Data = (Data & 0xFFFF) | ((uint)value << 16);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Unknown: {Unknown1} - {Unknown2}";
		}
	}
}
