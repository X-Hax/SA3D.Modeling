using SA3D.Modeling.File;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.AnimationData
{
	/// <summary>
	/// Animation specific IO context
	/// </summary>
	public readonly struct AnimationIOContext
	{
		/// <summary>
		/// Base IO context
		/// </summary>
		public IOContext BaseContext { init; get; }

		/// <summary>
		/// File specific IO context 
		/// </summary>
		public AnimationFileIOContext FileContext { init; get; }

		/// <summary>
		/// Keyframe type
		/// </summary>
		public KeyframeAttributes KeyframeType { init; get; }
	}
}
