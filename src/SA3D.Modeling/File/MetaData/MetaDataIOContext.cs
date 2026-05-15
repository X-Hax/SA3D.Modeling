namespace SA3D.Modeling.File.MetaData
{
	/// <summary>
	/// Metadata IO serialization context
	/// </summary>
	public readonly struct MetaDataIOContext
	{
		/// <summary>
		/// Metadata version
		/// </summary>
		public int Version { init; get; }

		/// <summary>
		/// Whether the metadata has anim morph files
		/// </summary>
		public bool HasAnimMorphFiles { init; get; }
	}
}
