using System.Text.Json.Serialization;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing file description
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class FlagMetaDataBlock : StringMetaDataBlock
	{
		internal class JsonConverter : Base2JsonConverter<FlagMetaDataBlock>
		{
			protected override bool CheckTypeMatches(MetaDataBlockType key)
			{
				return key == MetaDataBlockType.Flag;
			}
		}

		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.Flag;

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Flag: {Value}";
		}
	}
}
