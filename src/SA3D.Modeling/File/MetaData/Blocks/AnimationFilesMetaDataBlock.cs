namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing file paths to animations associated with this file
	/// </summary>
	public class AnimationFilesMetaDataBlock : StringListMetaDataBlock
	{
		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.Animation;

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Animation files: [{Values.Count}]";
		}
	}
}
