using SA3D.Modeling.Mesh.Gamecube.Enums;

namespace SA3D.Modeling.Mesh.Gamecube.Parameters
{
	/// <summary>
	/// Determines where or how the geometry gets the texture coordinates.
	/// </summary>
	public struct GCTexCoordParameter : IGCParameter
	{
		/// <summary>
		/// Default values parameter.
		/// </summary>
		public static readonly GCTexCoordParameter DefaultValues = new()
		{
			TexCoordID = GCTexCoordID.TexCoord0,
			TexCoordType = GCTexCoordType.Matrix2x4,
			TexCoordSource = GCTexCoordSource.TexCoord0,
			MatrixID = GCTexcoordMatrix.Identity
		};

		/// <summary>
		/// Environment mapping parameter.
		/// </summary>
		public static readonly GCTexCoordParameter EnvironmentMapValues = new()
		{
			TexCoordID = GCTexCoordID.TexCoord0,
			TexCoordType = GCTexCoordType.Matrix3x4,
			TexCoordSource = GCTexCoordSource.Normal,
			MatrixID = GCTexcoordMatrix.Matrix4
		};


		/// <inheritdoc/>
		public readonly GCParameterType Type => GCParameterType.Texcoord;

		/// <inheritdoc/>
		public uint Data { get; set; }


		/// <summary>
		/// Output channel to which calculated texture coordinates should be written to.
		/// </summary>
		public GCTexCoordID TexCoordID
		{
			readonly get => (GCTexCoordID)((Data >> 16) & 0xFF);
			set => Data = (Data & 0xFF00FFFF) | ((uint)value << 16);
		}

		/// <summary>
		/// The function type used to generate the texture coordinates.
		/// </summary>
		public GCTexCoordType TexCoordType
		{
			readonly get => (GCTexCoordType)((Data >> 12) & 0xF);
			set => Data = (Data & 0xFFFF0FFF) | ((uint)value << 12);
		}

		/// <summary>
		/// Input values to use for when calculating texture coordinates.
		/// </summary>
		public GCTexCoordSource TexCoordSource
		{
			readonly get => (GCTexCoordSource)((Data >> 4) & 0xFF);
			set => Data = (Data & 0xFFFFF00F) | ((uint)value << 4);
		}

		/// <summary>
		/// Matrix slot to use when using <see cref="GCTexCoordType.Matrix2x4"/> or <see cref="GCTexCoordType.Matrix3x4"/>.
		/// </summary>
		public GCTexcoordMatrix MatrixID
		{
			readonly get => (GCTexcoordMatrix)(Data & 0xF);
			set => Data = (Data & 0xFFFFFFF0) | (uint)value;
		}


		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Texcoord: {TexCoordID} - {TexCoordType} - {TexCoordSource} - {MatrixID}";
		}
	}
}
