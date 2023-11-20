using System;
using System.Linq;

namespace SA3D.Modeling.ObjectData
{
	public partial class Node
	{
		/// <summary>
		/// Direct child of the node.
		/// </summary>
		public Node? Child { get; private set; }

		/// <summary>
		/// Parent of the node.
		/// </summary>
		public Node? Parent { get; private set; }

		/// <summary>
		/// The succeeding node.
		/// </summary>
		public Node? Next { get; private set; }

		/// <summary>
		/// The preceeding node.
		/// </summary>
		public Node? Previous { get; private set; }

		/// <summary>
		/// Number of direct children this node has.
		/// </summary>
		public int ChildCount => this.Count();


		/// <summary>
		/// Whether this node has a previous or next node.
		/// </summary>
		public bool HasSiblings => Next != null || Previous != null;

		/// <summary>
		/// Whether this node has a parent and/or siblings.
		/// </summary>
		public bool HasImmediates => Parent != null || HasSiblings;


		/// <summary>
		/// Returns child at a specific index.
		/// </summary>
		/// <param name="index">Index of the child to get.</param>
		/// <returns></returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public Node this[int index]
		{
			get
			{
				if(index < 0)
				{
					throw new IndexOutOfRangeException($"Index {index} out of range! Must be a positive value!");
				}

				if(Child == null)
				{
					throw new IndexOutOfRangeException($"Index {index} out of range! Node has no children!");
				}

				Node target = Child;
				for(int i = 0; i < index; i++)
				{
					if(target.Next == null)
					{
						throw new IndexOutOfRangeException($"Index {index} out of range! Maximum valid index: {i}");
					}

					target = target.Next;
				}

				return target;
			}
		}

		/// <summary>
		/// Returns the root parent of this tree.
		/// </summary>
		public Node GetRootParent()
		{
			Node root = this;
			while(root.Parent != null)
			{
				root = root.Parent;
			}

			return root;
		}

		/// <summary>
		/// Returns the root sibling of this node.
		/// </summary>
		/// <returns></returns>
		public Node GetRootSibling()
		{
			Node root = this;
			while(root.Previous != null)
			{
				root = root.Previous;
			}

			return root;
		}

		/// <summary>
		/// Returns the absolute root of the node tree (root sibling of the root parent).
		/// </summary>
		/// <returns></returns>
		public Node GetRootNode()
		{
			return GetRootParent().GetRootSibling();
		}


		/// <summary>
		/// Detaches the node from its parent and siblings. Children will be kept.
		/// </summary>
		public void Detach()
		{
			if(!HasImmediates)
			{
				return;
			}

			if(Previous != null)
			{
				Previous.Next = Next;
			}
			else if(Parent != null)
			{
				Parent.Child = Next;
			}

			if(Next != null)
			{
				Next.Previous = Previous;
			}

			Parent = null;
			Next = null;
			Previous = null;
		}

		/// <summary>
		/// Detaches all children from this node.
		/// </summary>
		/// <param name="remainSiblings">Whether to keep the sibling relationships between the detached child nodes.</param>
		public void DetachChildren(bool remainSiblings)
		{
			if(Child == null)
			{
				return;
			}

			Node? current = Child;
			while(current != null)
			{
				Node? next = current.Next;

				current.Parent = null;

				if(!remainSiblings)
				{
					current.Next = null;
					current.Previous = null;
				}

				current = next;
			}

			Child = null;
		}

		/// <summary>
		/// Detaches the successor node from this node.
		/// </summary>
		/// <param name="remainSiblings">Whether to keep the sibling relationships between the detached successor nodes.</param>
		public void DetachSuccessors(bool remainSiblings)
		{
			if(Next == null)
			{
				return;
			}

			Node? current = Next;
			while(current != null)
			{
				Node? next = current.Next;

				if(current.Parent != null)
				{
					current.Parent = null;
				}

				if(!remainSiblings)
				{
					current.Next = null;
					current.Previous = null;
				}

				current = next;
			}

			Next.Previous = null;
			Next = null;
		}


		/// <summary>
		/// Inserts a <paramref name="node"/> before <see langword="this"/> node.
		/// <br/> ( <see langword="this"/>.Previous = <paramref name="node"/> ).
		/// </summary>
		/// <remarks>Runs <see cref="Detach"/> on node.</remarks>
		/// <param name="node">The node to insert.</param>
		/// <exception cref="InvalidOperationException"/>
		public void InsertBefore(Node node)
		{
			CheckAttachCompatibility(node);

			node.Detach();

			if(Previous != null)
			{
				Previous.Next = node;
			}
			else if(Parent != null)
			{
				Parent.Child = node;
			}

			node.Parent = Parent;
			node.Next = this;
			Previous = node;
		}

		/// <summary>
		/// Inserts a <paramref name="node"/> after <see langword="this"/> node.
		/// <br/> ( <see langword="this"/>.Next = <paramref name="node"/> ).
		/// </summary>
		/// <remarks>Runs <see cref="Detach"/> on node.</remarks>
		/// <param name="node">The node to insert.</param>
		/// <exception cref="InvalidOperationException"/>
		public void InsertAfter(Node node)
		{
			CheckAttachCompatibility(node);

			node.Detach();

			if(Next != null)
			{
				Next.Previous = node;
			}

			node.Parent = Parent;
			node.Previous = this;
			Next = node;
		}

		/// <summary>
		/// Inserts the node after the last child node.
		/// </summary>
		/// <param name="node">The node to append.</param>
		public void AppendChild(Node node)
		{
			if(Child == null)
			{
				CheckAttachCompatibility(node);
				node.Detach();

				node.Parent = this;
				Child = node;

				return;
			}

			Node target = Child;
			while(target.Next != null)
			{
				target = target.Next;
			}

			target.InsertAfter(node);
		}

		/// <summary>
		/// Inserts the node to be a specific child index.
		/// </summary>
		/// <param name="index">Index at which to insert the node.</param>
		/// <param name="node">The node to insert.</param>
		/// <exception cref="IndexOutOfRangeException"></exception>
		public void InsertChild(int index, Node node)
		{
			if(index < 0)
			{
				throw new IndexOutOfRangeException($"Index {index} out of range! Must be a positive value!");
			}

			Node? previous = null;
			Node? target = Child;
			for(int i = 0; i < index; i++)
			{
				if(target == null)
				{
					throw new IndexOutOfRangeException($"Index {index} out of range! Maximum valid insert index: {i}");
				}

				previous = target;
				target = target.Next;
			}


			if(target != null)
			{
				target.InsertBefore(node);
			}
			else if(previous != null)
			{
				previous.InsertAfter(node);
			}
			else
			{
				AppendChild(node);
			}
		}


		/// <summary>
		/// Replaces the child of <see langword="this"/> node. Old and new child will keep sibling relationships.
		/// </summary>
		/// <param name="node">The new child to set</param>
		public void SetChild(Node? node)
		{
			if(Child == node)
			{
				return;
			}

			if(node?.Previous != null)
			{
				throw new InvalidOperationException("The node you are trying to set has a previous node, only root siblings can be set as a direct child!");
			}

			if(node != null)
			{
				CheckAttachCompatibility(node);
			}

			DetachChildren(true);

			if(node == null)
			{
				return;
			}

			Node? current = node;
			while(current != null)
			{
				current.Parent = this;
				current = current.Next;
			}

			Child = node;
		}

		/// <summary>
		/// Replaces the successor of <see langword="this"/> node. Old and new child will keep their own successors.
		/// </summary>
		/// <param name="node">The new successor to set.</param>
		public void SetNext(Node? node)
		{
			if(Child == node)
			{
				return;
			}

			if(node?.Parent != null)
			{
				throw new InvalidOperationException("The node you are trying to set has a parent node, only parentless nodes can be set as a direct successor!");
			}

			if(node != null)
			{
				CheckAttachCompatibility(node);
			}

			DetachSuccessors(true);

			if(node == null)
			{
				return;
			}

			if(Parent != null)
			{
				Node? current = node;
				while(current != null)
				{
					current.Parent = Parent;
					current = current.Next;
				}
			}

			Next = node;
			node.Previous = this;
		}
	}
}
