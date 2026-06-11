using SA3D.Modeling.Structs;

namespace SA3D.Modeling.AnimationData
{
	/// <summary>
	/// Animation Ascii IO Context
	/// </summary>
	public struct AnimationAsciiIOContext
	{
		/// <summary>
		/// Base IO Context
		/// </summary>
		public AsciiIOContext BaseContext { init; get; }

		/// <summary>
		/// Keyframe type
		/// </summary>
		public KeyframeAttributes KeyframeType { get; set; }
	}
}
