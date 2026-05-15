namespace SA3D.Modeling.File
{
	/// <summary>
	/// Animation file specific IO context
	/// </summary>
	public readonly struct AnimationFileIOContext
	{
		/// <summary>
		/// Number of keyframe sets in the animation (one for e.g. each node in a model)
		/// </summary>
		public uint KeyframeSetCount { init; get; }

		/// <summary>
		/// Rotations are 16 bit instead of 32
		/// </summary>
		public bool ShortRotations { init; get; }
	}
}
