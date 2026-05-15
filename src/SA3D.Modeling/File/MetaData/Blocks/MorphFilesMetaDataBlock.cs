namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing file paths to morph animations associated with this file
	/// </summary>
	public class MorphFilesMetaDataBlock : StringListMetaDataBlock
	{
		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.Morph;

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Morph files: [{Values.Count}]";
		}
	}
}
