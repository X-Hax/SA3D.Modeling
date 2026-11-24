using SA3D.Common;
using System;

namespace SA3D.Modeling.ObjectData.Enums
{
	/// <summary>
	/// SA1 geometry surface information.
	/// </summary>
	[Flags]
	public enum SA1SurfaceAttributes : uint
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
		/// Ice behavior, friction is 0.1
		/// </summary>
		[IsCollisionAttribute]
		Ice = Flag32.B2,

		/// <summary>
		/// Snowboard terrain, friction is 0.15
		/// </summary>
		[IsCollisionAttribute]
		Snow = Flag32.B3,

		/// <summary>
		/// Sandboard terrain, friction is 0.5
		/// </summary>
		[IsCollisionAttribute]
		Sand = Flag32.B4,

		/// <summary>
		/// Uses the skybox draw distance instead of the level draw distance
		/// </summary>
		[IsDisplayAttribute]
		UseSkyDrawDistance = Flag32.B5,

		/// <summary>
		/// Surface will prevent players from standing on it
		/// </summary>
		[IsCollisionAttribute]
		CannotLand = Flag32.B6,

		/// <summary>
		/// Terrain friction is 1.5
		/// </summary>
		[IsCollisionAttribute]
		HighFriction = Flag32.B7,

		/// <summary>
		/// Knuckles can dig
		/// </summary>
		[IsCollisionAttribute]
		Diggable = Flag32.B8,

		/// <summary>
		/// Unused, but likely intended to be used for disabling collision with the camera
		/// </summary>
		[IsCollisionAttribute]
		NoCameraCollision = Flag32.B9,

		/// <summary>
		/// Mystic Ruin waterfalls (Force alpha sorting; Disable Z Write when used together with Water; Force disable Z write in all levels except Lost World 2)
		/// </summary>
		[IsDisplayAttribute]
		Waterfall = Flag32.B10,

		/// <summary>
		/// Unused, but found on lost world waterslide 
		/// </summary>
		[IsCollisionAttribute]
		Waterslide = Flag32.B11,

		/// <summary>
		/// Surface cannot be climbed by Knuckles
		/// </summary>
		[IsCollisionAttribute]
		Unclimbable = Flag32.B12,

		/// <summary>
		/// Turns off Visible when Chaos 0 jumps up a pole
		/// </summary>
		[IsDisplayAttribute]
		Chaos0Land = Flag32.B13,

		/// <summary>
		/// Stairs behavior (upward normal, high floor grip)
		/// </summary>
		[IsCollisionAttribute]
		Stairs = Flag32.B14,

		/// <summary>
		/// Unused, but likely intended to be used for lava surfaces
		/// </summary>
		[IsCollisionAttribute]
		Lava = Flag32.B15,

		/// <summary>
		/// Collision inflicts damage on the player
		/// </summary>
		[IsCollisionAttribute]
		ContactDamage = Flag32.B16,

		/// <summary>
		/// Improves character rotation against floor and camera angles. Also has dedicated acceleration code, but its unused.
		/// </summary>
		[IsCollisionAttribute]
		Tube = Flag32.B17,

		/// <summary>
		/// Higher priority in draw queue to improve some transparent elements
		/// </summary>
		[IsDisplayAttribute]
		RenderPriority = Flag32.B18,

		/// <summary>
		/// Unused, but likely "ninja simple draw mode"
		/// </summary>
		[IsDisplayAttribute]
		DrawSimple = Flag32.B19,

		/// <summary>
		/// Footprint behavior, only implemented in Emerald Coast
		/// </summary>
		[IsCollisionAttribute]
		Footprints = Flag32.B20,

		/// <summary>
		/// Barrier/border behavior, usually invisible
		/// </summary>
		[IsCollisionAttribute]
		Air = Flag32.B21,

		/// <summary>
		/// Water flag with heavier physics
		/// </summary>
		[IsCollisionAttribute]
		HeavyWater = Flag32.B22,

		/// <summary>
		/// Keeps the player stuck to surfaces by altering gravitional direction (?).
		/// </summary>
		[IsCollisionAttribute]
		MovementSlopeGravity = Flag32.B23,

		/// <summary>
		/// No Z write, used for shadow/decal meshes
		/// </summary>
		[IsDisplayAttribute]
		NoZWrite = Flag32.B24,

		/// <summary>
		/// Sort and draw by mesh, intended to improve self transparency
		/// </summary>
		[IsDisplayAttribute]
		DrawByMesh = Flag32.B25,

		/// <summary>
		/// Landtable entry should not be prebuffered (allows mesh data manipulations)
		/// </summary>
		[IsDisplayAttribute]
		NoDirect = Flag32.B26,

		/// <summary>
		/// Collision should update every frame (for moving tasks)
		/// </summary>
		[IsCollisionAttribute]
		DynamicCollision = Flag32.B27,

		/// <summary>
		/// Collision should rotate with task (ZXY)
		/// </summary>
		[IsCollisionAttribute]
		DynamicCollisionRotation = Flag32.B28,

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
