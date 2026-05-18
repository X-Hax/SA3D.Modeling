using System.Text.Json.Serialization;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing the tool with which the file was created
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public class ToolMetaDataBlock : StringMetaDataBlock
	{
		internal class JsonConverter : Base2JsonConverter<ToolMetaDataBlock>
		{
			protected override bool CheckTypeMatches(MetaDataBlockType key)
			{
				return key == MetaDataBlockType.Tool;
			}
		}

		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.Tool;

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Tool: {Value}";
		}
	}
}
