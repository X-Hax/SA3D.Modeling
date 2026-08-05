namespace SA3D.Modeling.File
{
	/// <summary>
	/// Animation file specific IO context
	/// </summary>
	public struct AnimationFileIOContext
	{
		/// <summary>
		/// Number of keyframe sets in the animation (one for e.g. each node in a model)
		/// </summary>
		public uint KeyframeSetCount { get; set; }

		/// <summary>
		/// Rotation angles are 16 bit
		/// </summary>
		public bool ShortRotations { get; set; }

		/// <summary>
		/// Angles use 0xFFFF for 360°, not 0x10000
		/// </summary>
		public bool BAMSFAngles { get; set; }
	}
}
