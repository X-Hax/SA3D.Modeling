namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing the object name
	/// </summary>
	public class ObjectNameMetaDataBlock : StringMetaDataBlock
	{
		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.ObjectName;

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Object name: {Value}";
		}
	}
}
