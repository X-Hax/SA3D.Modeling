using System.Collections.Generic;

namespace SA3D.Modeling.File
{
	internal class FileHeaders
	{
		public const ulong HeaderMask = ~((ulong)0xFF << 56);

		public const ulong LVL = 0x4C564Cu;
		public const ulong MDL = 0x4C444Du;

		public const ulong SA1 = 0x314153u;
		public const ulong SADX = 0x58444153u;
		public const ulong SA2 = 0x324153u;
		public const ulong SA2B = 0x42324153u;

		#region Landtable

		public const ulong SA1LVL = (LVL << 24) | SA1;
		public const ulong SADXLVL = (LVL << 32) | SADX;
		public const ulong SA2LVL = (LVL << 24) | SA2;
		public const ulong SA2BLVL = (LVL << 32) | SA2B;

		public const ulong CurrentLandtableVersion = 3;
		public const ulong CurrentLandtableVersionShifted = CurrentLandtableVersion << 56;

		public const ulong SA1LVLVer = SA1LVL | CurrentLandtableVersionShifted;
		public const ulong SADXLVLVer = SADXLVL | CurrentLandtableVersionShifted;
		public const ulong SA2LVLVer = SA2LVL | CurrentLandtableVersionShifted;
		public const ulong SA2BLVLVer = SA2BLVL | CurrentLandtableVersionShifted;

		#endregion

		#region Model

		public const ulong SA1MDL = (MDL << 24) | SA1;
		public const ulong SADXMDL = (MDL << 32) | SADX;
		public const ulong SA2MDL = (MDL << 24) | SA2;
		public const ulong SA2BMDL = (MDL << 32) | SA2B;

		public const ulong CurrentModelVersion = 3;
		public const ulong CurrentModelVersionShifted = CurrentModelVersion << 56;

		public const ulong SA1MDLVer = SA1MDL | CurrentModelVersionShifted;
		public const ulong SADXMDLVer = SADXMDL | CurrentModelVersionShifted;
		public const ulong SA2MDLVer = SA2MDL | CurrentModelVersionShifted;
		public const ulong SA2BMDLVer = SA2BMDL | CurrentModelVersionShifted;

		#endregion

		#region Animation

		public const ulong SAANIM = 0x4D494E414153u;

		public const ulong CurrentAnimVersion = 2;
		public const ulong CurrentAnimVersionShifted = CurrentAnimVersion << 56;

		public const ulong SAANIMVer = SAANIM | CurrentAnimVersionShifted;


		#endregion

		#region Other

		public const string NinjaModelBlockIdentifier = "NJ";
		public const string GinjaModelBlockIdentifier = "GJ";

		public const string ChunkModelBlockType = "CM";
		public const string BasicModelBlockType = "BM";
		public const string TextureListBlockType = "TL";

		public const string ModelMotionBlockHeader = "NMDM";
		public const string ShapeMotionBlockHeader = "NSSM";
		public const string CameraMotionBlockHeader = "NCAM";


		public static readonly HashSet<string> TextureListBlockHeaders =
		[
			NinjaModelBlockIdentifier + TextureListBlockType,
			GinjaModelBlockIdentifier + TextureListBlockType,
		];

		public static readonly HashSet<string> ModelBlockHeaders =
		[
			NinjaModelBlockIdentifier + ChunkModelBlockType,
			NinjaModelBlockIdentifier + BasicModelBlockType,
			GinjaModelBlockIdentifier + ChunkModelBlockType,
			GinjaModelBlockIdentifier + BasicModelBlockType
		];

		public static readonly HashSet<string> AnimationBlockHeaders =
		[
			ModelMotionBlockHeader,
			ShapeMotionBlockHeader,
			CameraMotionBlockHeader,
		];

		#endregion
	}
}
