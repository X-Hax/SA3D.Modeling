using System.Text.Json.Serialization;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing file paths to animations associated with this file
	/// </summary>
	[JsonConverter(typeof(JsonConverter))]
	public sealed class AnimationFilesMetaDataBlock : StringListMetaDataBlock
	{
		internal class JsonConverter : Base2JsonConverter<AnimationFilesMetaDataBlock>
		{
			protected override bool CheckTypeMatches(MetaDataBlockType key)
			{
				return key == MetaDataBlockType.Animation;
			}
		}

		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.Animation;

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"Animation files: [{Values.Count}]";
		}
	}
}
