using System.Text.Json.Serialization;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing file paths to morph animations associated with this file
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class MorphFilesMetaDataBlock : StringListMetaDataBlock
	{
		internal class JsonConverter : Base2JsonConverter<MorphFilesMetaDataBlock>
		{
			protected override bool CheckTypeMatches(MetaDataBlockType key)
			{
				return key == MetaDataBlockType.Morph;
			}
		}

		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.Morph;

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Morph files: [{Values.Count}]";
		}
	}
}
