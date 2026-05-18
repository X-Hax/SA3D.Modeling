using System.Text.Json.Serialization;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing the object name
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public class ObjectNameMetaDataBlock : StringMetaDataBlock
	{
		internal class JsonConverter : Base2JsonConverter<ObjectNameMetaDataBlock>
		{
			protected override bool CheckTypeMatches(MetaDataBlockType key)
			{
				return key == MetaDataBlockType.ObjectName;
			}
		}

		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.ObjectName;

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Object name: {Value}";
		}
	}
}
