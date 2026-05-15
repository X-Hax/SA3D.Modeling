using SA3D.Common.Lookup;
using SA3D.Modeling.AnimationData;
using SA3D.Modeling.Mesh;
using SA3D.Modeling.ObjectData;
using System.Collections.Generic;

namespace SA3D.Modeling.Structs
{
	/// <summary>
	/// Pointer Lookup Table.
	/// </summary>
	public class PointerLUT : BaseLUT
	{
		/// <summary>
		/// Pointer dictionary for nodes.
		/// </summary>
		public PointerDictionary<Node> Nodes { get; }

		/// <summary>
		/// Pointer dictionary for attaches.
		/// </summary>
		public PointerDictionary<MeshData> Attaches { get; }

		/// <summary>
		/// Pointer dictionary for motions.
		/// </summary>
		public PointerDictionary<Animation> Motions { get; }

		/// <summary>
		/// Pointer dictionary for nodemotions.
		/// </summary>
		public PointerDictionary<ModelAnimation> NodeMotions { get; }

		/// <summary>
		/// Pointer dictionary for other objects.
		/// </summary>
		public PointerDictionary<object> Other { get; }

		/// <summary>
		/// Creates a new LUT with preexisting labels.
		/// </summary>
		/// <param name="labels">The labels to populate the LUT with.</param>
		public PointerLUT(Dictionary<long, string> labels) : base(labels)
		{
			Nodes = new();
			Attaches = new();
			Motions = new();
			NodeMotions = new();
			Other = new();
		}

		/// <summary>
		/// Creates a new empty LUT.
		/// </summary>
		public PointerLUT() : this([]) { }

		/// <inheritdoc/>
		protected override void AddEntry(long address, object value)
		{
			switch(value)
			{
				case Node node:
					Nodes.Add(address, node);
					break;
				case MeshData attach:
					Attaches.Add(address, attach);
					break;
				case Animation motion:
					Motions.Add(address, motion);
					break;
				case ModelAnimation action:
					NodeMotions.Add(address, action);
					break;
				default:
					Other.Add(address, value);
					break;
			}
		}
	}
}
