using System.Text.Json.Serialization;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing file description
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public class DescriptionMetaDataBlock : StringMetaDataBlock
	{
		internal class JsonConverter : Base2JsonConverter<DescriptionMetaDataBlock>
		{
			protected override bool CheckTypeMatches(MetaDataBlockType key)
			{
				return key == MetaDataBlockType.Description;
			}
		}

		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.Description;

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Description: {Value}";
		}
	}
}
