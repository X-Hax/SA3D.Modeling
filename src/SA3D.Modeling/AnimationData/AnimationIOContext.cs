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

		internal KeyframeAttributes KeyframeType { get; set; }

		/// <summary>
		/// Offset lookup table
		/// </summary>
		public ModelOffsetLUT OffsetLUT { get; set; }

		/// <summary>
		/// IO Type for angles
		/// </summary>
		public readonly FloatIOType AngleType
			=> BAMSFAngles ? FloatIOType.BAMSF32 : FloatIOType.BAMS32;

		/// <summary>
		/// IO Type for rotation angles
		/// </summary>
		public readonly FloatIOType RotationAngleType
		{
			get
			{
				if(ShortRotations)
				{
					return BAMSFAngles ? FloatIOType.BAMSF16 : FloatIOType.BAMS16;
				}
				else
				{
					return BAMSFAngles ? FloatIOType.BAMSF32 : FloatIOType.BAMS32;
				}
			}
		}

		internal AnimationFileIOContext FileContext
		{
			set
			{
				BAMSFAngles = value.BAMSFAngles;
				KeyframeSetCount = value.KeyframeSetCount;
				ShortRotations = value.ShortRotations;
			}
		}
	}
}
