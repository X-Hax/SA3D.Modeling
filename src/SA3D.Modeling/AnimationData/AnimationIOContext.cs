using SA3D.Modeling.File;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.AnimationData
{
	/// <summary>
	/// Animation specific IO context
	/// </summary>
	public struct AnimationIOContext
	{
		/// <summary>
		/// Base IO context
		/// </summary>
		public IOContext BaseContext { get; set; }

		/// <summary>
		/// File specific IO context 
		/// </summary>
		public AnimationFileIOContext FileContext { get; set; }

		/// <summary>
		/// Keyframe type
		/// </summary>
		public KeyframeAttributes KeyframeType { get; set; }
	}
}
