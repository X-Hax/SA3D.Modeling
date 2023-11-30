using SA3D.Modeling.Mesh;
using SA3D.Modeling.Mesh.Converters;
using SA3D.Modeling.Mesh.Weighted;
using SA3D.Modeling.ObjectData.Events;
using SA3D.Modeling.ObjectData.Structs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Modeling.ObjectData
{
	public partial class Node
	{
		private Attach? _attach;


		/// <summary>
		/// Mesh data attached to this node.
		/// </summary>
		public Attach? Attach
		{
			get => _attach;
			set
			{
				if(value == _attach)
				{
					return;
				}

				AttachFormat? format = GetAttachFormat();

				if(value != null && format != null && format != value.Format)
				{
					throw new FormatException($"Node uses {format} attaches, and the attach that is being set is of type {value.Format}! Cannot set attach!");
				}

				Attach? previous = _attach;
				_attach = value;

				OnAttachUpdated?.Invoke(this, new(previous, _attach));
			}
		}

		/// <summary>
		/// Raised when the attach of the node changes.
		/// </summary>
		public event AttachUpdatedEventHandler? OnAttachUpdated;

		/// <summary>
		/// Vertex welding info. Used for Deforming meshes with no native weight support.
		/// </summary>
		public VertexWelding[]? Welding { get; set; }


		/// <summary>
		/// Determines Attach format across the whole tree that this node belongs to.
		/// </summary>
		public AttachFormat? GetAttachFormat()
		{
			foreach(Node node in GetTreeNodeEnumerable())
			{
				if(node.Attach != null)
				{
					return node.Attach.Format;
				}
			}

			return null;
		}

		private void CheckAttachCompatibility(Node other)
		{
			AttachFormat? format = GetAttachFormat();

			if(other.CheckHasBranchAttaches()
				&& format != null
				&& other.GetAttachFormat() != format)
			{
				throw new InvalidOperationException("The node you are trying to insert has an incompatible attach format!");
			}
		}

		/// <summary>
		/// Checks if <see langword="this"/> or any node directly below has an attach.
		/// </summary>
		public bool CheckHasBranchAttaches()
		{
			return GetBranchNodeEnumerable(false).Any(x => x._attach != null);
		}

		/// <summary>
		/// Removes all of the attaches from the tree, allowing for changing the attach format.
		/// </summary>
		public void ClearAttachesFromTree()
		{
			foreach(Node node in GetTreeNodeEnumerable())
			{
				if(node._attach != null)
				{
					Attach? previous = node._attach;
					node._attach = null;
					node.OnAttachUpdated?.Invoke(node, new(previous, null));
				}
			}
		}

		/// <summary>
		/// Whether this node tree has weighted attaches
		/// </summary>
		public bool CheckHasTreeWeightedMesh()
		{
			return GetTreeAttachEnumerable().Any(x => x.CheckHasWeights());
		}


		/// <summary>
		/// Generates buffer mesh data for the attaches in the entire tree.
		/// </summary>
		/// <param name="optimize">Whether to optimize vertex and polygon data of the buffered meshes.</param>
		public void BufferMeshData(bool optimize)
		{
			AttachFormat? format = GetAttachFormat();
			if(format == null)
			{
				return;
			}

			Node rootNode = GetRootNode();

			switch(format)
			{
				case AttachFormat.BASIC:
					BasicConverter.BufferBasicModel(rootNode, optimize);
					break;
				case AttachFormat.CHUNK:
					ChunkConverter.BufferChunkModel(rootNode, optimize);
					break;
				case AttachFormat.GC:
					GCConverter.BufferGCModel(rootNode, optimize);
					break;
				case AttachFormat.Buffer:
				default:
					break;
			}
		}

		/// <summary>
		/// Converts the entire Model to a different attach format.
		/// </summary>
		/// <param name="newAttachFormat">The attach format to convert to.</param>
		/// <param name="bufferMode">How to handle buffered mesh data of the model.</param>
		/// <param name="optimize">Whether to optimize the new attach data.</param>
		/// <param name="ignoreWeights">Convert regardless of weight information being lost.</param>
		/// <param name="forceUpdate">Force conversion, even if the attach format ends up being the same.</param>
		/// <param name="updateBuffer">Whether to generate buffer mesh data after conversion.</param>
		public void ConvertAttachFormat(
			AttachFormat newAttachFormat,
			BufferMode bufferMode,
			bool optimize,
			bool ignoreWeights = false,
			bool forceUpdate = false,
			bool updateBuffer = false)
		{
			AttachFormat? format = GetAttachFormat();
			if(format == null || (newAttachFormat == format && !forceUpdate))
			{
				return;
			}

			if(newAttachFormat == AttachFormat.Buffer)
			{
				BufferMeshData(optimize);

				Dictionary<Attach, Node> attachPairs = new();
				foreach(Node node in GetTreeNodeEnumerable())
				{
					if(node.Attach != null)
					{
						attachPairs.Add(node.Attach, node);
					}
				}

				ClearAttachesFromTree();

				foreach(KeyValuePair<Attach, Node> pair in attachPairs)
				{
					if(pair.Key.MeshData.Length == 0)
					{
						continue;
					}

					pair.Value.Attach = new(pair.Key.MeshData)
					{
						Label = pair.Key.Label,
						MeshBounds = pair.Key.MeshBounds
					};
				}

				return;
			}

			Node rootNode = GetRootNode();
			WeightedMesh[] weightedMeshes = WeightedMesh.FromModel(rootNode, bufferMode);

			if(weightedMeshes.Length == 0)
			{
				throw new InvalidOperationException("No weighted meshes have been generated! Did you perhaps forget to buffer the mesh data?");
			}

			switch(newAttachFormat)
			{
				case AttachFormat.BASIC:
					BasicConverter.ConvertWeightedToBasic(rootNode, weightedMeshes, optimize, ignoreWeights);
					break;
				case AttachFormat.CHUNK:
					ChunkConverter.ConvertWeightedToChunk(rootNode, weightedMeshes, optimize);
					break;
				case AttachFormat.GC:
					GCConverter.ConvertWeightedToGC(rootNode, weightedMeshes, optimize, ignoreWeights);
					break;
				case AttachFormat.Buffer:
				default:
					break;
			}

			if(updateBuffer)
			{
				BufferMeshData(optimize);
			}
		}
	}
}
