using SA3D.Modeling.AnimationData;
using SA3D.Modeling.Mesh;
using SA3D.Modeling.Mesh.Chunk;
using SA3D.Modeling.ObjectData.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// Ascii flag maps
	/// </summary>
	public static class AsciiMaps
	{
		/// <summary>
		/// Node attributes map
		/// </summary>
		public static ReadOnlyDictionary<string, NodeAttributes> NodeAttributesMap { get; } = new(new Dictionary<string, NodeAttributes>()
		{
			{ "FEV_UT", NodeAttributes.NoPosition },
			{ "FEV_UR", NodeAttributes.NoRotation },
			{ "FEV_US", NodeAttributes.NoScale },
			{ "FEV_HD", NodeAttributes.SkipDraw },
			{ "FEV_BR", NodeAttributes.SkipChildren },
			{ "FEV_ZXY", NodeAttributes.RotateZYX },
			{ "FEV_SK", NodeAttributes.NoAnimate },
			{ "FEV_SSK", NodeAttributes.NoMorph },
			{ "FEV_CL", NodeAttributes.Clip },
			{ "FEV_MD", NodeAttributes.Modifier },
			{ "FEV_QU", NodeAttributes.UseQuaternionRotation },
			{ "FEV_RB", NodeAttributes.CacheMatrix },
			{ "FEV_RS", NodeAttributes.ApplyCachedMatrix },
			{ "FEV_EN", NodeAttributes.Envelope }
		});

		/// <summary>
		/// Vertex Chunk type map
		/// </summary>
		public static ReadOnlyDictionary<string, VertexChunkType> VertexChunkTypeMap { get; } = new(new Dictionary<string, VertexChunkType>()
		{
			{ "CnkV_SH", VertexChunkType.BlankVec4 },
			{ "CnkV_VN_SH", VertexChunkType.NormalVec4 },
			{ "CnkV", VertexChunkType.Blank },
			{ "CnkV_D8", VertexChunkType.Diffuse },
			{ "CnkV_UF", VertexChunkType.UserAttributes },
			{ "CnkV_NF", VertexChunkType.Attributes },
			{ "CnkV_S5", VertexChunkType.DiffuseSpecular5 },
			{ "CnkV_S4", VertexChunkType.DiffuseSpecular4 },
			{ "CnkV_IN", VertexChunkType.Intensity },
			{ "CnkV_VN", VertexChunkType.Normal },
			{ "CnkV_VN_D8", VertexChunkType.NormalDiffuse },
			{ "CnkV_VN_UF", VertexChunkType.NormalUserAttributes },
			{ "CnkV_VN_NF", VertexChunkType.NormalAttributes },
			{ "CnkV_VN_S5", VertexChunkType.NormalDiffuseSpecular5 },
			{ "CnkV_VN_S4", VertexChunkType.NormalDiffuseSpecular4 },
			{ "CnkV_VN_IN", VertexChunkType.NormalIntensity },
			{ "CnkV_VNX", VertexChunkType.Normal32 },
			{ "CnkV_VNX_D8", VertexChunkType.Normal32Diffuse },
			{ "CnkV_VNX_UF", VertexChunkType.Normal32UserAttributes },
			{ "CnkV_D8_S8", VertexChunkType.DiffuseSpecular },
			{ "CnkV_NF_D8", VertexChunkType.AttributesDiffuse },
		});

		/// <summary>
		/// Weight mode map
		/// </summary>
		public static  ReadOnlyDictionary<string, WeightMode> WeightModeMap { get; } = new(new Dictionary<string, WeightMode>()
		{
			{ "FW_START", WeightMode.Start },
			{ "FW_MIDDLE", WeightMode.Middle },
			{ "FW_END", WeightMode.End },
		});

		/// <summary>
		/// Poly Chunk type map
		/// </summary>
		public static ReadOnlyDictionary<string, PolyChunkType> PolyChunkTypeMap { get; } = new(new Dictionary<string, PolyChunkType>()
		{
			{ "CnkB_BA", PolyChunkType.BlendAlpha },
			{ "CnkB_DA", PolyChunkType.MipmapDistanceMultiplier },
			{ "CnkB_EXP", PolyChunkType.SpecularExponent },
			{ "CnkB_CP", PolyChunkType.CacheList },
			{ "CnkB_DP", PolyChunkType.DrawList },

			{ "CnkT_TID", PolyChunkType.TextureID },
			{ "CnkT_TID2", PolyChunkType.TextureID2 },

			{ "CnkM", PolyChunkType.Material_Empty },
			{ "CnkM_D", PolyChunkType.Material_Diffuse },
			{ "CnkM_A", PolyChunkType.Material_Ambient },
			{ "CnkM_DA", PolyChunkType.Material_DiffuseAmbient },
			{ "CnkM_S", PolyChunkType.Material_Specular },
			{ "CnkM_DS", PolyChunkType.Material_DiffuseSpecular },
			{ "CnkM_AS", PolyChunkType.Material_AmbientSpecular },
			{ "CnkM_DAS", PolyChunkType.Material_DiffuseAmbientSpecular },
			{ "CnkM_BU", PolyChunkType.Material_Bump },
			{ "CnkM_D2", PolyChunkType.Material_Diffuse2 },
			{ "CnkM_A2", PolyChunkType.Material_Ambient2 },
			{ "CnkM_DA2", PolyChunkType.Material_DiffuseAmbient2 },
			{ "CnkM_S2", PolyChunkType.Material_Specular2 },
			{ "CnkM_DS2", PolyChunkType.Material_DiffuseSpecular2 },
			{ "CnkM_AS2", PolyChunkType.Material_AmbientSpecular2 },
			{ "CnkM_DAS2", PolyChunkType.Material_DiffuseAmbientSpecular2 },

			{ "CnkO_P3", PolyChunkType.Volume_Triangle },
			{ "CnkO_P4", PolyChunkType.Volume_Quad },
			{ "CnkO_ST", PolyChunkType.Volume_Strip },

			{ "CnkS", PolyChunkType.Strip_Blank },
			{ "CnkS_UVN", PolyChunkType.Strip_Tex },
			{ "CnkS_UVH", PolyChunkType.Strip_HDTex },
			{ "CnkS_VN", PolyChunkType.Strip_Normal },
			{ "CnkS_VN_UVN", PolyChunkType.Strip_TexNormal },
			{ "CnkS_VN_UVH", PolyChunkType.Strip_HDTexNormal },
			{ "CnkS_D8", PolyChunkType.Strip_Color },
			{ "CnkS_D8_UVN", PolyChunkType.Strip_TexColor },
			{ "CnkS_D8_UVH", PolyChunkType.Strip_HDTexColor },
			{ "CnkS_2", PolyChunkType.Strip_BlankDouble },
			{ "CnkS_UVN2", PolyChunkType.Strip_TexDouble },
			{ "CnkS_UVH2", PolyChunkType.Strip_HDTexDouble },
		});

		/// <summary>
		/// Source blend mode map
		/// </summary>
		public static Dictionary<string, BlendMode> SourceBlendModeMap { get; } = new(new Dictionary<string, BlendMode>()
		{
			{ "FBS_ZER", BlendMode.Zero },
			{ "FBS_ONE", BlendMode.One },
			{ "FBS_OC", BlendMode.Other },
			{ "FBS_IOC", BlendMode.OtherInverted },
			{ "FBS_SA", BlendMode.SrcAlpha },
			{ "FBS_ISA", BlendMode.SrcAlphaInverted },
			{ "FBS_DA", BlendMode.DstAlpha },
			{ "FBS_IDA", BlendMode.DstAlphaInverted },
		});

		/// <summary>
		/// Destination blend mode map
		/// </summary>
		public static Dictionary<string, BlendMode> DestinationBlendModeMap { get; } = new(new Dictionary<string, BlendMode>()
		{
			{ "FBD_ZER", BlendMode.Zero },
			{ "FBD_ONE", BlendMode.One },
			{ "FBD_OC", BlendMode.Other },
			{ "FBD_IOC", BlendMode.OtherInverted },
			{ "FBD_SA", BlendMode.SrcAlpha },
			{ "FBD_ISA", BlendMode.SrcAlphaInverted },
			{ "FBD_DA", BlendMode.DstAlpha },
			{ "FBD_IDA", BlendMode.DstAlphaInverted },
		});

		/// <summary>
		/// Filter mode map
		/// </summary>
		public static Dictionary<string, FilterMode> FilterModeMap { get; } = new(new Dictionary<string, FilterMode>()
		{
			{ "FFM_PS", FilterMode.Nearest },
			{ "FFM_BF", FilterMode.Bilinear },
			{ "FFM_TFA", FilterMode.TrilinearA },
			{ "FFM_TFB", FilterMode.TrilinearB },
		});

		/// <summary>
		/// Keyframe attributes map
		/// </summary>
		public static Dictionary<string, KeyframeAttributes> KeyframeAttributesMap { get; } = new(new Dictionary<string, KeyframeAttributes>()
		{
			{ "FMK_POS0", KeyframeAttributes.Position },
			{ "FMK_ANG1", KeyframeAttributes.EulerRotation },
			{ "FMK_SCA2", KeyframeAttributes.Scale },
			{ "FMK_VEC3", KeyframeAttributes.Vector },
			{ "FMK_VEC0", KeyframeAttributes.Vertex },
			{ "FMK_SAN1", KeyframeAttributes.Normal },
			{ "FMK_TAR3", KeyframeAttributes.Target },
			{ "FMK_ROL6", KeyframeAttributes.Roll },
			{ "FMK_ANG7", KeyframeAttributes.Angle },
			{ "FMK_RGB8", KeyframeAttributes.LightColor },
			{ "FMK_INT9", KeyframeAttributes.Intensity },
			{ "FMK_SPOT", KeyframeAttributes.Spot },
			{ "FMK_POI9", KeyframeAttributes.Point },
			{ "FMK_QUA1", KeyframeAttributes.QuaternionRotation },
		});

		/// <summary>
		/// Keyframe attributes map
		/// </summary>
		public static Dictionary<string, InterpolationMode> InterpolationModeMap { get; } = new(new Dictionary<string, InterpolationMode>()
		{
			{ "FMT_L", InterpolationMode.Linear },
			{ "FMT_S", InterpolationMode.Spline },
			{ "FMT_U", InterpolationMode.User },
		});
	}
}
