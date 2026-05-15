namespace SA3D.Modeling.Mesh.Gamecube.Enums
{
	/// <summary>
	/// The types of parameter that exist
	/// </summary>
	public enum GCParameterType : uint
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
		Lighting = 2,

		//Unused = 3, // Yes, number 3 would probably crash the game

		/// <summary>
		/// Stores blending modes
		/// </summary>
		BlendAlpha = 4,

		/// <summary>
		/// Stores the ambient color
		/// </summary>
		AmbientColor = 5,

		/// <summary>
		/// Stores diffuse color (?).
		/// </summary>
		DiffuseColor = 6,

		/// <summary>
		/// Stores specular color (?).
		/// </summary>
		SpecularColor = 7,

		/// <summary>
		/// Store texture info.
		/// </summary>
		Texture = 8,

		/// <summary>
		/// Unknown functionality.
		/// </summary>
		Unknown = 9,

		/// <summary>
		/// Stores texture coordinate processing parameters.
		/// </summary>
		Texcoord = 10,
	}
}
