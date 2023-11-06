using System;

namespace SA3D.Modeling.ObjectData.Events
{
	/// <summary>
	/// Event arguments for <see cref="TransformsUpdatedEventHandler"/>.
	/// </summary>
	public sealed class TransformsUpdatedEventArgs : EventArgs
	{
		/// <summary>
		/// Transforms before changing.
		/// </summary>
		public TransformSet OldTransforms { get; }

		/// <summary>
		/// Transforms after changing.
		/// </summary>
		public TransformSet NewTransforms { get; }

		/// <summary>
		/// The updated transform values.
		/// </summary>
		public UpdatedTransformValue UpdatedValues { get; }

		internal TransformsUpdatedEventArgs(TransformSet oldTransforms, TransformSet newTransforms, UpdatedTransformValue updatedValues)
		{
			OldTransforms = oldTransforms;
			NewTransforms = newTransforms;
			UpdatedValues = updatedValues;
		}
	}

	/// <summary>
	/// Event raised when any transform property of a node gets changed.
	/// </summary>
	/// <param name="node">Node that has raised the event.</param>
	/// <param name="args">Arguments passed to the event.</param>
	public delegate void TransformsUpdatedEventHandler(Node node, TransformsUpdatedEventArgs args);
}
