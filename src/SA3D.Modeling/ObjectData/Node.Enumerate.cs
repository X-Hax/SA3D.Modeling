using SA3D.Modeling.Mesh;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Modeling.ObjectData
{
	public partial class Node : IEnumerable<Node>
	{
		#region Base

		/// <summary>
		/// Returns an enumerators that iterates over the direct children of this node.
		/// </summary>
		public IEnumerator<Node> GetEnumerator()
		{
			Node? current = Child;
			while(current != null)
			{
				yield return current;
				current = current.Next;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <summary>
		/// Returns all children of this node in an array.
		/// </summary>
		/// <returns>The child array.</returns>
		public Node[] GetChildren()
		{
			return this.ToArray();
		}

		/// <summary>
		/// Iterates over the branch, starting at this node.
		/// <br/> First <see cref="Child"/>, then <see cref="Next"/>.
		/// </summary>
		/// <param name="includeSiblings">Iterate over the sibling nodes of <see langword="this"/> node too (starts at the root sibling).</param>
		public IEnumerable<Node> GetBranchNodeEnumerable(bool includeSiblings)
		{
			Stack<Node> nodeStack = new();

			if(!includeSiblings)
			{
				yield return this;
				if(Child != null)
				{
					nodeStack.Push(Child);
				}
			}
			else
			{
				nodeStack.Push(GetRootSibling());
			}

			while(nodeStack.TryPop(out Node? node))
			{
				yield return node;

				if(node.Next != null)
				{
					nodeStack.Push(node.Next);
				}

				if(node.Child != null)
				{
					nodeStack.Push(node.Child);
				}
			}
		}

		/// <summary>
		/// Counts the number of nodes in the branch, starting at this node.
		/// </summary>
		/// <param name="includeSiblings">Iterate over the sibling nodes of <see langword="this"/> node too (starts at the root sibling).</param>
		public int GetBranchNodeCount(bool includeSiblings)
		{
			return GetBranchNodeEnumerable(includeSiblings).Count();
		}

		/// <summary>
		/// Returns the nodes in the branch, starting at this node.
		/// <br/> First <see cref="Child"/>, then <see cref="Next"/>.
		/// </summary>
		/// <param name="includeSiblings">Iterate over the sibling nodes of <see langword="this"/> node too (starts at the root sibling).</param>
		public Node[] GetBranchNodes(bool includeSiblings)
		{
			return GetBranchNodeEnumerable(includeSiblings).ToArray();
		}


		/// <summary>
		/// Iterates over the entire tree, starting at the root node.
		/// <br/> First <see cref="Child"/>, then <see cref="Next"/>.
		/// </summary>
		public IEnumerable<Node> GetTreeNodeEnumerable()
		{
			// Branch gets the root sibling either way, so its enough calling the root parent.
			return GetRootParent().GetBranchNodeEnumerable(true);
		}

		/// <summary>
		/// Returns the number of nodes in the entire tree, starting at the root node.
		/// </summary>
		/// <returns></returns>
		public int GetTreeNodeCount()
		{
			return GetTreeNodeEnumerable().Count();
		}

		/// <summary>
		/// Returns the entire tree, starting at the root node.
		/// <br/> First <see cref="Child"/>, then <see cref="Next"/>.
		/// </summary>
		public Node[] GetTreeNodes()
		{
			return GetTreeNodeEnumerable().ToArray();
		}

		#endregion

		#region Animate

		/// <summary>
		/// Iterates over the entire tree, starting at the root node.
		/// <br/> Includes only nodes where <see cref="NoAnimate"/> is <see langword="false"/>.
		/// <br/> First <see cref="Child"/>, then <see cref="Next"/>.
		/// </summary>
		public IEnumerable<Node> GetAnimTreeNodeEnumerable()
		{
			return GetTreeNodeEnumerable().Where(x => !x.NoAnimate);
		}

		/// <summary>
		/// Returns the number of nodes in the entire tree, starting at the root node.
		/// <br/> Includes only nodes where <see cref="NoAnimate"/> is <see langword="false"/>.
		/// </summary>
		/// <returns></returns>
		public int GetAnimTreeNodeCount()
		{
			return GetAnimTreeNodeEnumerable().Count();
		}

		/// <summary>
		/// Returns the entire tree, starting at the root node.
		/// <br/> Includes only nodes where <see cref="NoAnimate"/> is <see langword="false"/>.
		/// <br/> First <see cref="Child"/>, then <see cref="Next"/>.
		/// </summary>
		public Node[] GetAnimTreeNodes()
		{
			return GetAnimTreeNodeEnumerable().ToArray();
		}

		#endregion

		#region Morph

		/// <summary>
		/// Iterates over the entire tree, starting at the root node.
		/// <br/> Includes only nodes where <see cref="NoMorph"/> is <see langword="false"/>.
		/// <br/> First <see cref="Child"/>, then <see cref="Next"/>.
		/// </summary>
		public IEnumerable<Node> GetMorphTreeNodeEnumerable()
		{
			return GetTreeNodeEnumerable().Where(x => !x.NoMorph);
		}

		/// <summary>
		/// Returns the number of nodes in the entire tree, starting at the root node.
		/// <br/> Includes only nodes where <see cref="NoMorph"/> is <see langword="false"/>.
		/// </summary>
		/// <returns></returns>
		public int GetMorphTreeNodeCount()
		{
			return GetMorphTreeNodeEnumerable().Count();
		}

		/// <summary>
		/// Returns the entire tree, starting at the root node.
		/// <br/> Includes only nodes where <see cref="NoMorph"/> is <see langword="false"/>.
		/// <br/> First <see cref="Child"/>, then <see cref="Next"/>.
		/// </summary>
		public Node[] GetMorphTreeNodes()
		{
			return GetMorphTreeNodeEnumerable().ToArray();
		}

		#endregion

		#region Attaches

		/// <summary>
		/// Iterates over the attaches of the entire tree, starting at the root node.
		/// <br/> First <see cref="Child"/>, then <see cref="Next"/>.
		/// </summary>
		public IEnumerable<Attach> GetTreeAttachEnumerable()
		{
			foreach(Node node in GetTreeNodeEnumerable())
			{
				if(node.Attach != null)
				{
					yield return node.Attach;
				}
			}
		}

		/// <summary>
		/// Returns the number of attaches in the entire tree, starting at the root node.
		/// </summary>
		/// <returns></returns>
		public int GetTreeAttachCount()
		{
			return GetTreeAttachEnumerable().Count();
		}

		/// <summary>
		/// Returns the attaches in the entire tree, starting at the root node.
		/// <br/> First <see cref="Child"/>, then <see cref="Next"/>.
		/// </summary>
		public Attach[] GetTreeAttaches()
		{
			return GetTreeAttachEnumerable().ToArray();
		}

		#endregion
	}
}
