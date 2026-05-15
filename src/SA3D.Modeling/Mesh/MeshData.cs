using Amicitia.IO.Binary;
using SA3D.Common.Lookup;
using SA3D.Modeling.Structs;
using System;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.Mesh
{
	/// <summary>
	/// 3D mesh data. Its possible for multiple attaches to make up one full mesh.
	/// </summary>
	public abstract class MeshData : ICloneable, ILabel, IBinarySerializable<IOContext>
	{
		/// <inheritdoc/>
		public abstract string LabelPrefix { get; }

		/// <inheritdoc/>
		public string Label { get; set; }

		/// <summary>
		/// Format of the attach.
		/// </summary>
		public abstract AttachFormat MeshFormat { get; }

		/// <summary>
		/// Bounding sphere of the attach.
		/// </summary>
		public Bounds MeshBounds { get; set; }


		/// <summary>
		/// Base constructor for derived attach types.
		/// </summary>
		protected MeshData()
		{
			Label = LabelPrefix.GenerateIdentifier();
		}


		/// <summary>
		/// Checks whether the attaches mesh data has/relies on weights.
		/// </summary>
		/// <returns>Whether the attaches mesh data has/relies on weights</returns>
		public virtual bool CheckHasWeights()
		{
			return false;
		}

		/// <summary>
		/// Recalculates <see cref="Bounds"/> from the attach data.
		/// </summary>
		public abstract void RecalculateBounds();

		/// <summary>
		/// Checks whether the attach can be written in the given model format.
		/// </summary>
		/// <param name="format">The format to check.</param>
		/// <returns>Whether the model can be written.</returns>
		public abstract bool CanWrite(Format format);


		/// <inheritdoc/>
		public abstract void Read(BinaryObjectReader reader, IOContext context);

		/// <inheritdoc/>
		public abstract void Write(BinaryObjectWriter writer, IOContext context);


		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the attach.
		/// </summary>
		/// <returns>The cloned attach.</returns>
		public abstract MeshData Clone();

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Label} - Buffer";
		}
	}
}
