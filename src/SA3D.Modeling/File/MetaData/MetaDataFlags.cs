using SA3D.Modeling.File.MetaData.Blocks;

namespace SA3D.Modeling.File.MetaData
{
	/// <summary>
	/// Known flag values used by <see cref="FlagMetaDataBlock"/>
	/// </summary>
	public static class MetaDataFlags
	{
		/// <summary>
		/// Indicates that a chunk model is a chao chunk model. <br/>
		/// Chao chunk models have following unique properties: <br/>
		/// - Colors use ARGB instead of BGRA
		/// </summary>
		public const string ChaoChunk = "ChaoChunk";
	}
}
