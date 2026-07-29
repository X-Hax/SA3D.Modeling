using SA3D.Common.Lookup;
using SA3D.Modeling.AnimationData;
using SA3D.Modeling.Mesh;
using SA3D.Modeling.Mesh.Chunk;
using SA3D.Modeling.ObjectData;
using System.Collections.Generic;

namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// Pointer Lookup Table.
	/// </summary>
	public class ModelOffsetLUT : OffsetLUT
	{
		/// <summary>
		/// Pointer dictionary for nodes.
		/// </summary>
		public OffsetDictionary<Node> Nodes { get; } = new();

		/// <summary>
		/// Pointer dictionary for attaches.
		/// </summary>
		public OffsetDictionary<MeshData> MeshData { get; } = new();

		/// <summary>
		/// Pointer dictionary for motions.
		/// </summary>
		public OffsetDictionary<Animation> Motions { get; } = new();

		/// <summary>
		/// Offset dictionary for polychunks; Not actually tied to the offset lut, has to be manually added to
		/// </summary>
		public OffsetDictionary<PolyChunk> PolyChunks { get; } = new();


		/// <summary>
		/// Creates a new LUT with preexisting labels.
		/// </summary>
		/// <param name="labels">The labels to populate the LUT with.</param>
		public ModelOffsetLUT(Dictionary<long, string> labels) : base(labels) { }

		/// <summary>
		/// Creates a new empty LUT.
		/// </summary>
		public ModelOffsetLUT() : base() { }


		/// <inheritdoc/>
		protected override void OnAddEntry(long address, object value)
		{
			switch(value)
			{
				case Node node:
					Nodes.Add(address, node);
					break;
				case MeshData attach:
					MeshData.Add(address, attach);
					break;
				case Animation motion:
					Motions.Add(address, motion);
					break;
			}
		}
	}
}
