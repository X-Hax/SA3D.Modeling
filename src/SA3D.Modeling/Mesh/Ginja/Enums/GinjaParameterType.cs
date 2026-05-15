namespace SA3D.Modeling.Mesh.Ginja.Enums
{
	/// <summary>
	/// The types of parameter that exist
	/// </summary>
	public enum GinjaParameterType : uint
	{
		/// <summary>
		/// Stores vertex attribute format information.
		/// </summary>
		VertexFormat = 0,

		/// <summary>
		/// Stores vertex index attributes.
		/// </summary>
		IndexFormat = 1,

		/// <summary>
		/// Stores lighting values.
		/// </summary>
		StripFlags = 2,

		//Unused = 3, // Yes, number 3 would probably crash the game

		/// <summary>
		/// Stores blending modes
		/// </summary>
		BlendAlpha = 4,

		/// <summary>
		/// Stores the diffuse color
		/// </summary>
		DiffuseColor = 5,

		/// <summary>
		/// Stores ambient color.
		/// </summary>
		AmbientColor = 6,

		/// <summary>
		/// Stores specular color.
		/// </summary>
		SpecularColor = 7,

		/// <summary>
		/// Store texture info.
		/// </summary>
		Texture = 8,

		/// <summary>
		/// Unknown functionality.
		/// </summary>
		TevStage = 9,

		/// <summary>
		/// Stores texture coordinate processing parameters.
		/// </summary>
		TexGen = 10,
	}
}
