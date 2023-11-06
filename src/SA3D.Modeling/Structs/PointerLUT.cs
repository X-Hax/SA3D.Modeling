using SA3D.Common.Lookup;
using SA3D.Modeling.Animation;
using SA3D.Modeling.Mesh;
using SA3D.Modeling.Mesh.Chunk;
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
		public PointerDictionary<Attach> Attaches { get; }

		/// <summary>
		/// Pointer dictionary for motions.
		/// </summary>
		public PointerDictionary<Motion> Motions { get; }

		/// <summary>
		/// Pointer dictionary for nodemotions.
		/// </summary>
		public PointerDictionary<NodeMotion> NodeMotions { get; }

		/// <summary>
		/// Pointer dictionary for polygon chunks.
		/// </summary>
		public PointerDictionary<PolyChunk> PolyChunks { get; }

		/// <summary>
		/// Pointer dictionary for other objects.
		/// </summary>
		public PointerDictionary<object> Other { get; }

		/// <summary>
		/// Creates a new LUT with preexisting labels.
		/// </summary>
		/// <param name="labels">The labels to populate the LUT with.</param>
		public PointerLUT(Dictionary<uint, string> labels) : base(labels)
		{
			Nodes = new();
			Attaches = new();
			Motions = new();
			NodeMotions = new();
			PolyChunks = new();
			Other = new();
		}

		/// <summary>
		/// Creates a new empty LUT.
		/// </summary>
		public PointerLUT() : this(new()) { }

		/// <inheritdoc/>
		protected override void AddEntry(uint address, object value)
		{
			switch(value)
			{
				case Node node:
					Nodes.Add(address, node);
					break;
				case Attach attach:
					Attaches.Add(address, attach);
					break;
				case Motion motion:
					Motions.Add(address, motion);
					break;
				case NodeMotion action:
					NodeMotions.Add(address, action);
					break;
				case PolyChunk polychunk:
					PolyChunks.Add(address, polychunk);
					break;
				default:
					Other.Add(address, value);
					break;
			}
		}
	}
}
