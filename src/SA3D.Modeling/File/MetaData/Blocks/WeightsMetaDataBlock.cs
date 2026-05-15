using Amicitia.IO.Binary;
using SA3D.Common.IO;
using SA3D.Modeling.File.MetaData.Weights;
using System.Collections.Generic;

namespace SA3D.Modeling.File.MetaData.Blocks
{
	/// <summary>
	/// Metadata block containing vertex welding information
	/// </summary>
	public class WeightsMetaDataBlock : MetaDataBlock
	{
		/// <inheritdoc/>
		public override MetaDataBlockType Type => MetaDataBlockType.Weight;

		/// <summary>
		/// Weights
		/// </summary>
		public List<MetaWeightNode> Weights { get; set; } = [];


		/// <inheritdoc/>
		protected override void ReadContents(BinaryObjectReader reader)
		{
			uint peek;
			using(reader.At())
			{
				peek = reader.ReadUInt32();
			}

			while(peek != uint.MaxValue)
			{
				Weights.Add(reader.ReadObject<MetaWeightNode>());

				using(reader.At())
				{
					peek = reader.ReadUInt32();
				}
			}
		}

		/// <inheritdoc/>
		protected override void WriteContents(BinaryObjectWriter writer)
		{
			writer.WriteObjectArray(Weights);
			writer.WriteUInt32(uint.MaxValue);
		}
	}
}
