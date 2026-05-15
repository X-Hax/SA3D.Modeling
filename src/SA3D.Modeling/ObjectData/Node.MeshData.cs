using SA3D.Modeling.Mesh;
using SA3D.Modeling.ObjectData.Events;
using SA3D.Modeling.ObjectData.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Modeling.ObjectData
{
	public partial class Node
	{
		private MeshData? _meshData;


		/// <summary>
		/// Mesh data attached to this node.
		/// </summary>
		public MeshData? MeshData
		{
			get => _meshData;
			set
			{
				if(value == _meshData)
				{
					return;
				}

				MeshFormat? format = GetMeshFormat();

				if(value != null && format != null && format != value.MeshFormat)
				{
					throw new FormatException($"Node uses {format} mesh data, and the mesh data that is being set is of type {value.MeshFormat}! Cannot set mesh data!");
				}

				MeshData? previous = _meshData;
				_meshData = value;

				OnMeshDataUpdated?.Invoke(this, new(previous, _meshData));
			}
		}

		/// <summary>
		/// Raised when the mesh data of the node changes.
		/// </summary>
		public event MeshDataUpdatedEventHandler? OnMeshDataUpdated;

		/// <summary>
		/// Vertex welding info. Used for Deforming meshes with no native weight support.
		/// </summary>
		public VertexWelding[]? Welding { get; set; }


		/// <summary>
		/// Determines the MeshData format across the whole tree that this node belongs to.
		/// </summary>
		public MeshFormat? GetMeshFormat()
		{
			foreach(Node node in GetTreeNodeEnumerable())
			{
				if(node.MeshData != null)
				{
					return node.MeshData.MeshFormat;
				}
			}

			return null;
		}

		private void CheckMeshDataCompatibility(Node other)
		{
			MeshFormat? format = GetMeshFormat();

			if(other.CheckHasBranchMeshData()
				&& format != null
				&& other.GetMeshFormat() != format)
			{
				throw new InvalidOperationException("The node you are trying to insert has an incompatible meshdata format!");
			}
		}

		/// <summary>
		/// Checks if <see langword="this"/> or any node directly below has an meshdata.
		/// </summary>
		public bool CheckHasBranchMeshData()
		{
			return GetBranchNodeEnumerable(false).Any(x => x._meshData != null);
		}

		/// <summary>
		/// Removes all of the meshdata from the tree, allowing for changing the mesh data format.
		/// </summary>
		public void ClearMeshDataFromTree()
		{
			foreach(Node node in GetTreeNodeEnumerable())
			{
				if(node._meshData != null)
				{
					MeshData? previous = node._meshData;
					node._meshData = null;
					node.OnMeshDataUpdated?.Invoke(node, new(previous, null));
				}
			}
		}

		/// <summary>
		/// Whether this node tree has weighted meshdata
		/// </summary>
		public bool CheckHasTreeWeightedMesh()
		{
			return GetTreeMeshDataEnumerable().Any(x => x.CheckHasWeights());
		}

		/// <summary>
		/// Returns a dictionary of links between nodes that influence each other with welds.
		/// </summary>
		/// <param name="twoway">Whether to make all links twoay</param>
		public Dictionary<Node, HashSet<Node>> GetTreeWeldingLinks(bool twoway)
		{
			Dictionary<Node, HashSet<Node>> result = [];

			foreach(Node node in GetTreeNodeEnumerable())
			{
				if(node.Welding == null)
				{
					continue;
				}

				Weld[] t = node.Welding.SelectMany(x => x.Welds).ToArray();

				IEnumerable<Node> weldNodes = node.Welding
					.SelectMany(x => x.Welds)
					.Select(x => x.SourceNode);

				if(result.TryGetValue(node, out HashSet<Node>? links))
				{
					links.UnionWith(weldNodes);
				}
				else
				{
					links = weldNodes.ToHashSet();
					result.Add(node, links);
				}

				if(twoway)
				{
					foreach(Node linkedNode in links)
					{
						if(!result.TryGetValue(linkedNode, out HashSet<Node>? targetLinks))
						{
							targetLinks = [];
							result.Add(linkedNode, targetLinks);
						}

						targetLinks.Add(node);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Returns groups of nodes in the tree that influence each others meshdata via weldings.
		/// </summary>
		/// <param name="includeUnwelded">Create one-node groups for nodes that are not linked to other nodes. Otherwise ignore them</param>
		/// <exception cref="InvalidOperationException"/>
		public Node[][] GetTreeWeldingGroups(bool includeUnwelded)
		{
			Dictionary<Node, HashSet<Node>> weldLinks = GetTreeWeldingLinks(true);

			Dictionary<Node, int> nodeIndices = [];
			HashSet<Node> treeNodes = [];

			int index = 0;
			foreach(Node node in GetTreeNodeEnumerable())
			{
				nodeIndices.Add(node, index);
				treeNodes.Add(node);
				index++;
			}

			int notInTreeCount = weldLinks.Keys.Count(x => !treeNodes.Contains(x));
			if(notInTreeCount > 0)
			{
				throw new InvalidOperationException($"The welds reference {notInTreeCount} nodes that are not in the tree!");
			}

			List<Node[]> result = [];
			while(treeNodes.Count > 0)
			{
				Node start = treeNodes.First();
				treeNodes.Remove(start);

				if(!weldLinks.ContainsKey(start))
				{
					if(includeUnwelded)
					{
						result.Add(new[] { start });
					}

					continue;
				}

				HashSet<Node> group = [];
				Queue<Node> linkQueue = new();
				linkQueue.Enqueue(start);

				while(linkQueue.Count > 0)
				{
					Node node = linkQueue.Dequeue();
					group.Add(node);

					foreach(Node weldLink in weldLinks[node])
					{
						if(treeNodes.Remove(weldLink))
						{
							linkQueue.Enqueue(weldLink);
						}
					}
				}

				result.Add(group.OrderBy(x => nodeIndices[x]).ToArray());
			}

			return result.ToArray();
		}

		/// <summary>
		/// Removes all welding from the entire node tree that this node belongs to.
		/// </summary>
		public void ClearWeldingsFromTree()
		{
			foreach(Node node in GetTreeNodeEnumerable())
			{
				node.Welding = null;
			}
		}
	}
}
