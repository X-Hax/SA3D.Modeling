using SA3D.Common;
using System;

namespace SA3D.Modeling.ObjectData.Enums
{
	/// <summary>
	/// Geometry surface information <br/>
	/// Combination of <see cref="SA1SurfaceAttributes"/> and <see cref="SA2SurfaceAttributes"/>
	/// </summary>
	[Flags]
	public enum SurfaceAttributes : ulong
	{
		/// <summary>
		/// Enables rendering for the geometry.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		Visible = Flag64.B0,

		/// <summary>
		/// Makes the geometry collidable with.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		Solid = Flag64.B1,

		/// <summary>
		/// Adds water ripple effect when passing through geometry.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		Water = Flag64.B2,

		/// <summary>
		/// Adds water ripple effect when passing through geometry, but doesn't mess with the transparent queue (?).
		/// <br/> SA2
		/// </summary>
		WaterNoAlpha = Flag64.B3,

		/// <summary>
		/// Bounds center is offset from 0,0,0 and gets influenced by rotation and scale.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		TransformBounds = Flag64.B4,

		/// <summary>
		/// Bounds radius is &gt; 10 and &lt;= 20 units.
		/// <br/> If combined with <see cref="BoundsRadiusTiny"/>, its &gt;= 3.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		BoundsRadiusSmall = Flag64.B5,

		/// <summary>
		/// Bounds radius is &gt; 3 and &lt;= 10 units.
		/// <br/> If combined with <see cref="BoundsRadiusSmall"/>, its &gt;= 3.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		BoundsRadiusTiny = Flag64.B6,

		/// <summary>
		/// Bound transform related attributes.
		/// </summary>
		BoundTransforms = TransformBounds
						| BoundsRadiusSmall
						| BoundsRadiusTiny,


		/// <summary>
		/// Accelerates the player (?).
		/// <br/> SA1
		/// </summary>
		Accelerate = Flag64.B10,

		/// <summary>
		/// Reduces the players acceleration.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		LowAcceleration = Flag64.B12,

		/// <summary>
		/// Reduces the players acceleration to 0.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		NoAcceleration = Flag64.B13,

		/// <summary>
		/// Increases the players acceleration.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		IncreasedAcceleration = Flag64.B14,

		/// <summary>
		/// Tube acceleration (?).
		/// <br/> SA1 &amp; SA2
		/// </summary>
		TubeAcceleration = Flag64.B15,

		/// <summary>
		/// Surface has no (or less) friction. Player will slip around, like on ice.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		NoFriction = Flag64.B16,

		/// <summary>
		/// Surface will prevent players from standing on it.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		CannotLand = Flag64.B17,

		/// <summary>
		/// Surface cannot be climbed by players.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		Unclimbable = Flag64.B18,

		/// <summary>
		/// The player wont slip on the surface, regardless of how steep it is.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		Stairs = Flag64.B19,

		/// <summary>
		/// Lets the player dig on the surface.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		Diggable = Flag64.B20,

		/// <summary>
		/// Damages the player on contact.
		/// </summary>
		Hurt = Flag64.B21,

		/// <summary>
		/// Allows for transforming collisions.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		DynamicCollision = Flag64.B22,

		/// <summary>
		/// Changes player state to swimming when in contact.
		/// <br/> SA1
		/// </summary>
		WaterCollision = Flag64.B23,

		/// <summary>
		/// Keeps the player stuck to surfaces by altering gravitional direction (?).
		/// <br/> SA1 &amp; SA2
		/// </summary>
		Gravity = Flag64.B24,

		/// <summary>
		/// All attribute values that affect collision or physics.
		/// </summary>
		Collisions = Solid
				   | Water
				   | WaterNoAlpha
				   | Accelerate
				   | LowAcceleration
				   | NoAcceleration
				   | IncreasedAcceleration
				   | TubeAcceleration
				   | NoFriction
				   | CannotLand
				   | Unclimbable
				   | Stairs
				   | Diggable
				   | Hurt
				   | DynamicCollision
				   | WaterCollision
				   | Gravity,


		/// <summary>
		/// Renders footprints when running on the surface.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		Footprints = Flag64.B30,

		/// <summary>
		/// Surface will not render shadows.
		/// <br/> SA2
		/// </summary>
		NoShadows = Flag64.B31,

		/// <summary>
		/// Surface ignores fog when rendering.
		/// <br/> SA2
		/// </summary>
		NoFog = Flag64.B32,

		/// <summary>
		/// Affects drawing queue (?).
		/// <br/> SA1
		/// </summary>
		LowDepth = Flag64.B33,

		/// <summary>
		/// Uses skybox drawdistance.
		/// <br/> SA1 &amp; SA2
		/// </summary>
		UseSkyDrawDistance = Flag64.B34,

		/// <summary>
		/// Uses easy drawing function (dreamcast functionality).
		/// <br/> SA2
		/// </summary>
		EasyDraw = Flag64.B35,

		/// <summary>
		/// Does not write to the Z buffer.
		/// <br/> SA1
		/// </summary>
		NoZWrite = Flag64.B36,

		/// <summary>
		/// Sorts the transparent meshes within the attaches, instead of grouping them all together for sorting (?).
		/// <br/> SA1
		/// </summary>
		DrawByMesh = Flag64.B37,

		/// <summary>
		/// Dont store vertices in buffer.
		/// <br/> SA1
		/// </summary>
		EnableManipulation = Flag64.B38,

		/// <summary>
		/// Force alpha sorting; Disable Z Write when used together with Water; Force disable Z write in all levels except Lost World 2
		/// <br/> SA1
		/// </summary>
		Waterfall = Flag64.B39,

		/// <summary>
		/// Turns off Visible when Chaos 0 jumps up a pole.
		/// <br/> SA1
		/// </summary>
		Chaos0Land = Flag64.B40,

		/// <summary>
		/// All attribute values that affect visuals.
		/// </summary>
		Visuals = Visible
				| Footprints
				| NoShadows
				| NoFog
				| LowDepth
				| UseSkyDrawDistance
				| EasyDraw
				| NoZWrite
				| DrawByMesh
				| EnableManipulation
				| Waterfall
				| Chaos0Land,


		/// <summary>
		/// Unknown functionality from SA1.
		/// </summary>
		SA1_Unknown9 = Flag64.B52,

		/// <summary>
		/// Unknown functionality from SA1.
		/// </summary>
		SA1_Unknown11 = Flag64.B53,

		/// <summary>
		/// Unknown functionality from SA1.
		/// </summary>
		SA1_Unknown15 = Flag64.B54,

		/// <summary>
		/// Unknown functionality from SA1.
		/// </summary>
		SA1_Unknown19 = Flag64.B55,

		/// <summary>
		/// All unknown attribute values from SA1 combined.
		/// </summary>
		SA1_Unknowns = SA1_Unknown9
					 | SA1_Unknown11
					 | SA1_Unknown15
					 | SA1_Unknown19,


		/// <summary>
		/// Unknown functionality from SA2.
		/// </summary>
		SA2_Unknown6 = Flag64.B56,

		/// <summary>
		/// Unknown functionality from SA2.
		/// </summary>
		SA2_Unknown9 = Flag64.B57,

		/// <summary>
		/// Unknown functionality from SA2.
		/// </summary>
		SA2_Unknown14 = Flag64.B58,

		/// <summary>
		/// Unknown functionality from SA2.
		/// </summary>
		SA2_Unknown16 = Flag64.B59,

		/// <summary>
		/// Unknown functionality from SA2.
		/// </summary>
		SA2_Unknown17 = Flag64.B60,

		/// <summary>
		/// Unknown functionality from SA2.
		/// </summary>
		SA2_Unknown18 = Flag64.B61,

		/// <summary>
		/// Unknown functionality from SA2.
		/// </summary>
		SA2_Unknown25 = Flag64.B62,

		/// <summary>
		/// Unknown functionality from SA2.
		/// </summary>
		SA2_Unknown26 = Flag64.B63,

		/// <summary>
		/// All unknown attribute values from SA2 combined.
		/// </summary>
		SA2_Unknowns = SA2_Unknown6
					 | SA2_Unknown9
					 | SA2_Unknown14
					 | SA2_Unknown16
					 | SA2_Unknown17
					 | SA2_Unknown18
					 | SA2_Unknown25
					 | SA2_Unknown26,


		/// <summary>
		/// All unknown attribute values combined.
		/// </summary>
		Unknowns = SA1_Unknowns | SA2_Unknowns,

		/// <summary>
		/// All attribute values valid for collision geometry.
		/// </summary>
		CollisionMask = Collisions | Unknowns | BoundTransforms,

		/// <summary>
		/// All attribute values valid for visual geometry.
		/// </summary>
		VisualMask = Visuals | Unknowns | BoundTransforms,

		/// <summary>
		/// All valid attribute values combined.
		/// </summary>
		ValidMask = Visuals | Collisions | Unknowns | BoundTransforms,

	}
}
