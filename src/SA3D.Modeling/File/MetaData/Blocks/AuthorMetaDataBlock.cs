using System.Text.Json.Serialization;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing the file authors name
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class AuthorMetaDataBlock : StringMetaDataBlock
	{
		internal class JsonConverter : Base2JsonConverter<AuthorMetaDataBlock>
		{
			protected override bool CheckTypeMatches(MetaDataBlockType key)
			{
				return key == MetaDataBlockType.Author;
			}
		}

		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.Author;

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Author: {Value}";
		}
	}
}
