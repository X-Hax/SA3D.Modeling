using SA3D.Common;
using System;

namespace SA3D.Modeling.Mesh.Buffer
{
	/// <summary>
	/// Rendering attributes of materials.
	/// </summary>
	[Flags]
	public enum MaterialAttributes : ushort
	{
		/// <summary>
		/// Whether textures should be rendered.
		/// </summary>
		UseTexture = Flag16.B0,

		/// <summary>
		/// Enables anisotropic filtering.
		/// </summary>
		AnisotropicFiltering = Flag16.B1,

		/// <summary>
		/// Clamps texture corrdinates along the horizontal axis between -1 and 1.
		/// </summary>
		ClampU = Flag16.B2,

		/// <summary>
		/// Clamps texture corrdinates along the vertical axis between -1 and 1.
		/// </summary>
		ClampV = Flag16.B3,

		/// <summary>
		/// Mirrors texture coordinates along the horizontal axis every other full unit.
		/// </summary>
		MirrorU = Flag16.B4,

		/// <summary>
		/// Mirrors texture coordinates along the vertical axis every other full unit.
		/// </summary>
		MirrorV = Flag16.B5,

		/// <summary>
		/// Whether to use normal mapping for textures.
		/// </summary>
		NormalMapping = Flag16.B6,

		/// <summary>
		/// Ignores lighting as a whole.
		/// </summary>
		NoLighting = Flag16.B7,

		/// <summary>
		/// Ignores ambient lighting.
		/// </summary>
		NoAmbient = Flag16.B8,

		/// <summary>
		/// Ignores specular lighting.
		/// </summary>
		NoSpecular = Flag16.B9,

		/// <summary>
		/// Ignores interpolated normals and instead renders every polygon flat.
		/// </summary>
		Flat = Flag16.B10,

		/// <summary>
		/// Enables transparent rendering.
		/// </summary>
		UseAlpha = Flag16.B11,

		/// <summary>
		/// Enables backface culling.
		/// </summary>
		BackfaceCulling = Flag16.B12,
	}
}
