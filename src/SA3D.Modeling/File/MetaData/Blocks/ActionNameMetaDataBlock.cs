using System.Text.Json.Serialization;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata container containing the actions name
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public class ActionNameMetaDataBlock : StringMetaDataBlock
	{
		internal class JsonConverter : Base2JsonConverter<ActionNameMetaDataBlock>
		{
			protected override bool CheckTypeMatches(MetaDataBlockType key)
			{
				return key == MetaDataBlockType.ActionName;
			}
		}

		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.ActionName;

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Action name: {Value}";
		}
	}
}
