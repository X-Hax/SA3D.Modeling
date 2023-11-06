namespace SA3D.Modeling.ObjectData.Enums
{
	/// <summary>
	/// Extension methods for object data enums.
	/// </summary>
	public static class EnumExtensions
	{
		private static readonly (SA1SurfaceAttributes sa1, SurfaceAttributes universal)[] _sa1SurfaceAttributeMapping = new[]
		{
			( SA1SurfaceAttributes.Solid, SurfaceAttributes.Solid ),
			( SA1SurfaceAttributes.Water, SurfaceAttributes.Water ),
			( SA1SurfaceAttributes.NoFriction, SurfaceAttributes.NoFriction ),
			( SA1SurfaceAttributes.NoAcceleration, SurfaceAttributes.NoAcceleration ),

			( SA1SurfaceAttributes.LowAcceleration, SurfaceAttributes.LowAcceleration ),
			( SA1SurfaceAttributes.UseSkyDrawDistance, SurfaceAttributes.UseSkyDrawDistance ),
			( SA1SurfaceAttributes.CannotLand, SurfaceAttributes.CannotLand ),
			( SA1SurfaceAttributes.IncreasedAcceleration, SurfaceAttributes.IncreasedAcceleration ),

			( SA1SurfaceAttributes.Diggable, SurfaceAttributes.Diggable ),
			( SA1SurfaceAttributes.Unknown9, SurfaceAttributes.SA1_Unknown9 ),
			( SA1SurfaceAttributes.Waterfall, SurfaceAttributes.Waterfall ),
			( SA1SurfaceAttributes.Unknown11, SurfaceAttributes.SA1_Unknown11 ),

			( SA1SurfaceAttributes.Unclimbable, SurfaceAttributes.Unclimbable ),
			( SA1SurfaceAttributes.Chaos0Land, SurfaceAttributes.Chaos0Land ),
			( SA1SurfaceAttributes.Stairs, SurfaceAttributes.Stairs ),
			( SA1SurfaceAttributes.Unknown15, SurfaceAttributes.SA1_Unknown15 ),

			( SA1SurfaceAttributes.Hurt, SurfaceAttributes.Hurt ),
			( SA1SurfaceAttributes.TubeAcceleration, SurfaceAttributes.TubeAcceleration ),
			( SA1SurfaceAttributes.LowDepth, SurfaceAttributes.LowDepth ),
			( SA1SurfaceAttributes.Unknown19, SurfaceAttributes.SA1_Unknown19 ),

			( SA1SurfaceAttributes.Footprints, SurfaceAttributes.Footprints ),
			( SA1SurfaceAttributes.Accelerate, SurfaceAttributes.Accelerate ),
			( SA1SurfaceAttributes.WaterCollision, SurfaceAttributes.WaterCollision ),
			( SA1SurfaceAttributes.Gravity, SurfaceAttributes.Gravity ),

			( SA1SurfaceAttributes.NoZWrite, SurfaceAttributes.NoZWrite ),
			( SA1SurfaceAttributes.DrawByMesh, SurfaceAttributes.DrawByMesh ),
			( SA1SurfaceAttributes.EnableManipulation, SurfaceAttributes.EnableManipulation ),
			( SA1SurfaceAttributes.DynamicCollision, SurfaceAttributes.DynamicCollision ),

			( SA1SurfaceAttributes.TransformBounds, SurfaceAttributes.TransformBounds ),
			( SA1SurfaceAttributes.BoundsRadiusSmall, SurfaceAttributes.BoundsRadiusSmall ),
			( SA1SurfaceAttributes.BoundsRadiusTiny, SurfaceAttributes.BoundsRadiusTiny ),
			( SA1SurfaceAttributes.Visible, SurfaceAttributes.Visible ),
		};

		private static readonly (SA2SurfaceAttributes sa2, SurfaceAttributes universal)[] _sa2SurfaceAttributeMapping = new[]
		{
			( SA2SurfaceAttributes.Solid, SurfaceAttributes.Solid ),
			( SA2SurfaceAttributes.Water, SurfaceAttributes.Water ),
			( SA2SurfaceAttributes.NoFriction, SurfaceAttributes.NoFriction ),
			( SA2SurfaceAttributes.NoAcceleration, SurfaceAttributes.NoAcceleration ),

			( SA2SurfaceAttributes.LowAcceleration, SurfaceAttributes.LowAcceleration ),
			( SA2SurfaceAttributes.Diggable, SurfaceAttributes.Diggable ),
			( SA2SurfaceAttributes.Unknown6, SurfaceAttributes.SA2_Unknown6 ),
			( SA2SurfaceAttributes.Unclimbable, SurfaceAttributes.Unclimbable ),

			( SA2SurfaceAttributes.Stairs, SurfaceAttributes.Stairs ),
			( SA2SurfaceAttributes.Unknown9, SurfaceAttributes.SA2_Unknown9 ),
			( SA2SurfaceAttributes.Hurt, SurfaceAttributes.Hurt ),
			( SA2SurfaceAttributes.Footprints, SurfaceAttributes.Footprints ),

			( SA2SurfaceAttributes.CannotLand, SurfaceAttributes.CannotLand ),
			( SA2SurfaceAttributes.WaterNoAlpha, SurfaceAttributes.WaterNoAlpha ),
			( SA2SurfaceAttributes.Unknown14, SurfaceAttributes.SA2_Unknown14 ),
			( SA2SurfaceAttributes.NoShadows, SurfaceAttributes.NoShadows ),

			( SA2SurfaceAttributes.Unknown16, SurfaceAttributes.SA2_Unknown16 ),
			( SA2SurfaceAttributes.Unknown17, SurfaceAttributes.SA2_Unknown17 ),
			( SA2SurfaceAttributes.Unknown18, SurfaceAttributes.SA2_Unknown18 ),
			( SA2SurfaceAttributes.Gravity, SurfaceAttributes.Gravity ),

			( SA2SurfaceAttributes.TubeAcceleration, SurfaceAttributes.TubeAcceleration ),
			( SA2SurfaceAttributes.IncreasedAcceleration, SurfaceAttributes.IncreasedAcceleration ),
			( SA2SurfaceAttributes.NoFog, SurfaceAttributes.NoFog ),
			( SA2SurfaceAttributes.UseSkyDrawDistance, SurfaceAttributes.UseSkyDrawDistance ),

			( SA2SurfaceAttributes.EasyDraw, SurfaceAttributes.EasyDraw ),
			( SA2SurfaceAttributes.Unknown25, SurfaceAttributes.SA2_Unknown25 ),
			( SA2SurfaceAttributes.Unknown26, SurfaceAttributes.SA2_Unknown26 ),
			( SA2SurfaceAttributes.DynamicCollision, SurfaceAttributes.DynamicCollision ),

			( SA2SurfaceAttributes.TransformBounds, SurfaceAttributes.TransformBounds ),
			( SA2SurfaceAttributes.BoundsRadiusSmall, SurfaceAttributes.BoundsRadiusSmall ),
			( SA2SurfaceAttributes.BoundsRadiusTiny, SurfaceAttributes.BoundsRadiusTiny ),
			( SA2SurfaceAttributes.Visible, SurfaceAttributes.Visible ),
		};

		/// <summary>
		/// Converts from SA1 surface flags to the combined surface flags.
		/// </summary>
		/// <param name="attributes">Attributes to convert.</param>
		/// <returns>The converted attributes.</returns>
		public static SurfaceAttributes ToUniversal(this SA1SurfaceAttributes attributes)
		{
			SurfaceAttributes result = 0;

			foreach((SA1SurfaceAttributes sa1, SurfaceAttributes universal) in _sa1SurfaceAttributeMapping)
			{
				if(attributes.HasFlag(sa1))
				{
					result |= universal;
				}
			}

			return result;
		}

		/// <summary>
		/// Converts from SA2 surface flags to the combined surface flags.
		/// </summary>
		/// <param name="attributes">Attributes to convert.</param>
		/// <returns>The converted attributes.</returns>
		public static SurfaceAttributes ToUniversal(this SA2SurfaceAttributes attributes)
		{
			SurfaceAttributes result = 0;

			foreach((SA2SurfaceAttributes sa2, SurfaceAttributes universal) in _sa2SurfaceAttributeMapping)
			{
				if(attributes.HasFlag(sa2))
				{
					result |= universal;
				}
			}

			return result;
		}

		/// <summary>
		/// Converts from the combined surface flags to SA1 surface flags.
		/// </summary>
		/// <param name="attributes">Attributes to convert.</param>
		/// <returns>The converted attributes.</returns>
		public static SA1SurfaceAttributes ToSA1(this SurfaceAttributes attributes)
		{
			SA1SurfaceAttributes result = 0;

			foreach((SA1SurfaceAttributes sa1, SurfaceAttributes universal) in _sa1SurfaceAttributeMapping)
			{
				if(attributes.HasFlag(universal))
				{
					result |= sa1;
				}
			}

			return result;
		}

		/// <summary>
		/// Converts from the combined surface flags to SA2 surface flags.
		/// </summary>
		/// <param name="attributes">Attributes to convert.</param>
		/// <returns>The converted attributes.</returns>
		public static SA2SurfaceAttributes ToSA2(this SurfaceAttributes attributes)
		{
			SA2SurfaceAttributes result = 0;

			foreach((SA2SurfaceAttributes sa2, SurfaceAttributes universal) in _sa2SurfaceAttributeMapping)
			{
				if(attributes.HasFlag(universal))
				{
					result |= sa2;
				}
			}

			return result;
		}

		/// <summary>
		/// Checks whether the surface attributes enable rendering.
		/// </summary>
		/// <param name="attributes">The attributes to check.</param>
		/// <returns></returns>
		public static bool CheckIsVisual(this SurfaceAttributes attributes)
		{
			return attributes.HasFlag(SurfaceAttributes.Visible);
		}

		/// <summary>
		/// Checks whether the surface attributes use collision behavior.
		/// </summary>
		/// <param name="attributes">The attributes to check.</param>
		/// <returns></returns>
		public static bool CheckIsCollision(this SurfaceAttributes attributes)
		{
			return attributes.HasFlag(SurfaceAttributes.Solid)
				|| attributes.HasFlag(SurfaceAttributes.Water)
				|| attributes.HasFlag(SurfaceAttributes.WaterNoAlpha);
		}
	}
}
