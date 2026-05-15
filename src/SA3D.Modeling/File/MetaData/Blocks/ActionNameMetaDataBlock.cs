namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata container containing the actions name
	/// </summary>
	public class ActionNameMetaDataBlock : StringMetaDataBlock
	{
		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.ActionName;

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Action name: {Value}";
		}
	}
}
