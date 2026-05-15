namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing the file authors name
	/// </summary>
	public class AuthorMetaDataBlock : StringMetaDataBlock
	{
		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.Author;

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Author: {Value}";
		}
	}
}
