using SA3D.Modeling.Mesh.Ginja.Enums;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Determines where or how the geometry gets the texture coordinates.
	/// </summary>
	public struct GinjaTexGenParameter : IGinjaParameter
	{
		/// <summary>
		/// Default values parameter.
		/// </summary>
		public static readonly GinjaTexGenParameter DefaultValues = new()
		{
			TexCoord = GinjaTexCoordID.TexCoord0,
			TexGenType = GinjaTexGenType.Matrix2x4,
			TexGenSource = GinjaTexGenSource.TexCoord0,
			MatrixID = GinjaTexGenMatrix.Identity
		};

		/// <summary>
		/// Environment mapping parameter.
		/// </summary>
		public static readonly GinjaTexGenParameter EnvironmentMapValues = new()
		{
			TexCoord = GinjaTexCoordID.TexCoord0,
			TexGenType = GinjaTexGenType.Matrix3x4,
			TexGenSource = GinjaTexGenSource.Normal,
			MatrixID = GinjaTexGenMatrix.Matrix4
		};


		/// <inheritdoc/>
		public readonly GinjaParameterType Type => GinjaParameterType.TexGen;

		/// <inheritdoc/>
		public uint Data { get; set; }


		/// <summary>
		/// Output channel to which calculated texture coordinates should be written to.
		/// </summary>
		public GinjaTexCoordID TexCoord
		{
			readonly get => (GinjaTexCoordID)((Data >> 16) & 0xFF);
			set => Data = (Data & 0xFF00FFFF) | ((uint)value << 16);
		}

		/// <summary>
		/// The function type used to generate the texture coordinates.
		/// </summary>
		public GinjaTexGenType TexGenType
		{
			readonly get => (GinjaTexGenType)((Data >> 12) & 0xF);
			set => Data = (Data & 0xFFFF0FFF) | ((uint)value << 12);
		}

		/// <summary>
		/// Input values to use for when calculating texture coordinates.
		/// </summary>
		public GinjaTexGenSource TexGenSource
		{
			readonly get => (GinjaTexGenSource)((Data >> 4) & 0xFF);
			set => Data = (Data & 0xFFFFF00F) | ((uint)value << 4);
		}

		/// <summary>
		/// Matrix slot to use when using <see cref="GinjaTexGenType.Matrix2x4"/> or <see cref="GinjaTexGenType.Matrix3x4"/>.
		/// </summary>
		public GinjaTexGenMatrix MatrixID
		{
			readonly get => (GinjaTexGenMatrix)(Data & 0xF);
			set => Data = (Data & 0xFFFFFFF0) | (uint)value;
		}


		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Texcoord: {TexCoord} - {TexGenType} - {TexGenSource} - {MatrixID}";
		}
	}
}
