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
		/// Solid collision
		/// </summary>
		[IsCollisionAttribute]
		Solid = Flag32.B0,

		/// <summary>
		/// Water flag (ripples, breath counter, heavy physics)
		/// </summary>
		[IsCollisionAttribute]
		Water = Flag32.B1,

		/// <summary>
		/// low friction, icy
		/// </summary>
		[IsCollisionAttribute]
		LowFriction = Flag32.B2,

		/// <summary>
		/// High friction, sticky
		/// </summary>
		[IsCollisionAttribute]
		HighFriction = Flag32.B3,

		/// <summary>
		/// Medium (increased) friction
		/// </summary>
		[IsCollisionAttribute]
		MediumFriction = Flag32.B4,

		/// <summary>
		/// Hunters can dig
		/// </summary>
		[IsCollisionAttribute]
		Diggable = Flag32.B5,

		/// <summary>
		/// Unknown purpose
		/// </summary>
		[IsCollisionAttribute]
		Unknown6 = Flag32.B6,

		/// <summary>
		/// Surface cannot be climbed by hunters
		/// </summary>
		[IsCollisionAttribute]
		Unclimbable = Flag32.B7,

		/// <summary>
		/// Stairs behavior (upward normal, high floor grip)
		/// </summary>
		[IsCollisionAttribute]
		Stairs = Flag32.B8,

		/// <summary>
		/// Unknown purpose
		/// </summary>
		[IsCollisionAttribute]
		Unknown9 = Flag32.B9,

		/// <summary>
		/// Damages the player on contact.
		/// </summary>
		[IsCollisionAttribute]
		Hurt = Flag32.B10,

		/// <summary>
		/// Footprint behavior
		/// </summary>
		[IsCollisionAttribute]
		Footprints = Flag32.B11,

		/// <summary>
		/// Surface will prevent players from standing on it
		/// </summary>
		[IsCollisionAttribute]
		CannotLand = Flag32.B12,

		/// <summary>
		/// Heavy water
		/// </summary>
		[IsCollisionAttribute]
		HeavyWater = Flag32.B13,

		/// <summary>
		/// Unknown functionality
		/// </summary>
		[IsCollisionAttribute]
		Unknown14 = Flag32.B14,

		/// <summary>
		/// Surface will not render shadows
		/// </summary>
		[IsDisplayAttribute]
		NoShadows = Flag32.B15,

		/// <summary>
		/// Unknown functionality
		/// </summary>
		[IsCollisionAttribute]
		Unknown16 = Flag32.B16,

		/// <summary>
		/// Unknown functionality
		/// </summary>
		[IsCollisionAttribute]
		Unknown17 = Flag32.B17,

		/// <summary>
		/// Unknown functionality
		/// </summary>
		[IsCollisionAttribute]
		Unknown18 = Flag32.B18,

		/// <summary>
		/// Unknown functionality
		/// </summary>
		[IsCollisionAttribute]
		Unknown19 = Flag32.B19,

		/// <summary>
		/// Top speed increased, faster acceleration
		/// </summary>
		[IsCollisionAttribute]
		HighSpeed = Flag32.B20,

		/// <summary>
		/// Faster acceleration, same top speed
		/// </summary>
		[IsCollisionAttribute]
		HighAcceleration = Flag32.B21,

		/// <summary>
		/// Increased gravity above the mesh
		/// </summary>
		[IsCollisionAttribute]
		HighGravity = Flag32.B22,

		/// <summary>
		/// Disable fog when rendering
		/// </summary>
		[IsDisplayAttribute]
		NoFog = Flag32.B22,

		/// <summary>
		/// Uses the skybox draw distance instead of the level draw distance
		/// </summary>
		[IsDisplayAttribute]
		UseSkyDrawDistance = Flag32.B23,

		/// <summary>
		/// Unknown purpose, but easy to pass through(?)
		/// </summary>
		[IsCollisionAttribute]
		UnknownCollision24 = Flag32.B24,

		/// <summary>
		/// Use SimpleDraw, else EasyDraw (Dreamcast/Ninja technology)
		/// </summary>
		[IsDisplayAttribute]
		EasyDraw = Flag32.B24,

		/// <summary>
		/// Use DirectDraw (internal) (Dreamcast/Ninja technology)
		/// </summary>
		[IsDisplayAttribute]
		DirectDraw = Flag32.B25,

		/// <summary>
		/// Don't compile model, use other draw
		/// </summary>
		[IsDisplayAttribute]
		NoCompile = Flag32.B26,

		/// <summary>
		/// Collision should update every frame (for moving tasks)
		/// </summary>
		[IsCollisionAttribute]
		DynamicCollision = Flag32.B27,

		/// <summary>
		/// Do not apply rotation (ZXY) to collision
		/// </summary>
		[IsCollisionAttribute]
		NoRotation = Flag32.B28,

		/// <summary>
		/// Bounds radius is &gt; 10 and &lt;= 20 units.
		/// <br/> If combined with <see cref="BoundsRadiusTiny"/>, its &gt;= 3.
		/// </summary>
		[IsCollisionAttribute]
		BoundsRadiusSmall = Flag32.B29,

		/// <summary>
		/// Bounds radius is &gt; 3 and &lt;= 10 units.
		/// <br/> If combined with <see cref="BoundsRadiusSmall"/>, its &gt;= 3.
		/// </summary>
		[IsCollisionAttribute]
		BoundsRadiusTiny = Flag32.B30,

		/// <summary>
		/// Enables rendering for the geometry.
		/// </summary>
		[IsDisplayAttribute]
		Visible = Flag32.B31
	}
}
