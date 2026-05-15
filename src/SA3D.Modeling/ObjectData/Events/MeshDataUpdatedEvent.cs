using SA3D.Modeling.Mesh;
using System;

namespace SA3D.Modeling.ObjectData.Events
{
	/// <summary>
	/// Arguments for the <see cref="AttachUpdatedEventHandler"/>.
	/// </summary>
	public class AttachUpdatedEventArgs : EventArgs
	{
		/// <summary>
		/// Attach before changing.
		/// </summary>
		public Attach? OldAttach { get; }

		/// <summary>
		/// Attach after changing.
		/// </summary>
		public Attach? NewAttach { get; }

		internal AttachUpdatedEventArgs(Attach? oldAttach, Attach? newAttach)
		{
			OldAttach = oldAttach;
			NewAttach = newAttach;
		}
	}

	/// <summary>
	/// Event raised when the attach of a node gets changed.
	/// </summary>
	/// <param name="node">Node that has raised the event.</param>
	/// <param name="args">Arguments passed to the event.</param>
	public delegate void AttachUpdatedEventHandler(Node node, AttachUpdatedEventArgs args);
}
