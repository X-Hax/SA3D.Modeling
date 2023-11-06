using SA3D.Common;
using System;

namespace SA3D.Modeling.ObjectData.Enums
{
	/// <summary>
	/// SA2 geometry surface information.
	/// </summary>
	[Flags]
	public enum SA2SurfaceAttributes : uint
	{
		/// <summary>
		/// Makes the geometry collidable with.
		/// </summary>
		Solid = Flag32.B0,

		/// <summary>
		/// Adds water ripple effect when passing through geometry.
		/// </summary>
		Water = Flag32.B1,

		/// <summary>
		/// Surface has no (or less) friction. Player will slip around, like on ice.
		/// </summary>
		NoFriction = Flag32.B2,

		/// <summary>
		/// Reduces the players acceleration to 0.
		/// </summary>
		NoAcceleration = Flag32.B3,

		/// <summary>
		/// Reduces the players acceleration.
		/// </summary>
		LowAcceleration = Flag32.B4,

		/// <summary>
		/// Lets the player dig on the surface.
		/// </summary>
		Diggable = Flag32.B5,

		/// <summary>
		/// Unknown functionality.
		/// </summary>
		Unknown6 = Flag32.B6,

		/// <summary>
		/// Surface cannot be climbed by players.
		/// </summary>
		Unclimbable = Flag32.B7,

		/// <summary>
		/// The player wont slip on the surface, regardless of how steep it is.
		/// </summary>
		Stairs = Flag32.B8,

		/// <summary>
		/// Unknown functionality.
		/// </summary>
		Unknown9 = Flag32.B9,

		/// <summary>
		/// Damages the player on contact.
		/// </summary>
		Hurt = Flag32.B10,

		/// <summary>
		/// Renders footprints when running on the surface.
		/// </summary>
		Footprints = Flag32.B11,

		/// <summary>
		/// Surface will prevent players from standing on it.
		/// </summary>
		CannotLand = Flag32.B12,

		/// <summary>
		/// Adds water ripple effect when passing through geometry, but doesn't mess with the transparent queue (?).
		/// </summary>
		WaterNoAlpha = Flag32.B13,

		/// <summary>
		/// Unknown functionality.
		/// </summary>
		Unknown14 = Flag32.B14,

		/// <summary>
		/// Surface will not render shadows.
		/// </summary>
		NoShadows = Flag32.B15,

		/// <summary>
		/// Unknown functionality.
		/// </summary>
		Unknown16 = Flag32.B16,

		/// <summary>
		/// Unknown functionality.
		/// </summary>
		Unknown17 = Flag32.B17,

		/// <summary>
		/// Unknown functionality.
		/// </summary>
		Unknown18 = Flag32.B18,

		/// <summary>
		/// Keeps the player stuck to surfaces by altering gravitional direction (?).
		/// </summary>
		Gravity = Flag32.B19,

		/// <summary>
		/// Tube acceleration (?).
		/// </summary>
		TubeAcceleration = Flag32.B20,

		/// <summary>
		/// Increases the players acceleration.
		/// </summary>
		IncreasedAcceleration = Flag32.B21,

		/// <summary>
		/// Surface ignores fog when rendering.
		/// </summary>
		NoFog = Flag32.B22,

		/// <summary>
		/// Uses skybox drawdistance.
		/// </summary>
		UseSkyDrawDistance = Flag32.B23,

		/// <summary>
		/// Uses easy drawing function (dreamcast functionality).
		/// </summary>
		EasyDraw = Flag32.B24,

		/// <summary>
		/// Unknown functionality.
		/// </summary>
		Unknown25 = Flag32.B25,

		/// <summary>
		/// Unknown functionality.
		/// </summary>
		Unknown26 = Flag32.B26,

		/// <summary>
		/// Moving collision
		/// </summary>
		DynamicCollision = Flag32.B27,

		/// <summary>
		/// Bounds center is offset from 0,0,0 and gets influenced by rotation and scale.
		/// </summary>
		TransformBounds = Flag32.B28,

		/// <summary>
		/// Bounds radius is &gt; 10 and &lt;= 20 units.
		/// <br/> If combined with <see cref="BoundsRadiusTiny"/>, its &gt;= 3.
		/// </summary>
		BoundsRadiusSmall = Flag32.B29,

		/// <summary>
		/// Bounds radius is &gt; 3 and &lt;= 10 units.
		/// <br/> If combined with <see cref="BoundsRadiusSmall"/>, its &gt;= 3.
		/// </summary>
		BoundsRadiusTiny = Flag32.B30,

		/// <summary>
		/// Enables rendering for the geometry.
		/// </summary>
		Visible = Flag32.B31
	}
}
