namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing file description
	/// </summary>
	public class DescriptionMetaDataBlock : StringMetaDataBlock
	{
		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.Description;

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Description: {Value}";
		}
	}
}
