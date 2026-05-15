using SA3D.Modeling.Mesh;
using System;

namespace SA3D.Modeling.ObjectData.Events
{
	/// <summary>
	/// Arguments for the <see cref="MeshDataUpdatedEventHandler"/>.
	/// </summary>
	public class MeshDataUpdatedEventArgs : EventArgs
	{
		/// <summary>
		/// Meshdata before changing.
		/// </summary>
		public MeshData? OldMeshData { get; }

		/// <summary>
		/// Meshdata after changing.
		/// </summary>
		public MeshData? NewMeshData { get; }

		internal MeshDataUpdatedEventArgs(MeshData? oldMeshData, MeshData? newMeshData)
		{
			OldMeshData = oldMeshData;
			NewMeshData = newMeshData;
		}
	}

	/// <summary>
	/// Event raised when the attach of a node gets changed.
	/// </summary>
	/// <param name="node">Node that has raised the event.</param>
	/// <param name="args">Arguments passed to the event.</param>
	public delegate void MeshDataUpdatedEventHandler(Node node, MeshDataUpdatedEventArgs args);
}
