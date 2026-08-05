using Amicitia.IO.Binary;
using SA3D.Common;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Ginja.Enums;
using SA3D.Modeling.Structs;
using System.Collections.Generic;

namespace SA3D.Modeling.Mesh.Ginja
{
	/// <summary>
	/// Ginja vertex set array
	/// </summary>
	public class GinjaVertexSetArray : LabeledArray<GinjaVertexSet>, IBinarySerializable<IOContext>
	{
		private const string _labelPrefix = "vertex_";

		/// <inheritdoc/>
		public override string LabelPrefix => _labelPrefix;

		/// <summary>
		/// Creates a new, empty vertex set array
		/// </summary>
		public GinjaVertexSetArray() : base(_labelPrefix.GenerateIdentifier()) { }

		/// <summary>
		/// Creates a new, empty vertex set array
		/// </summary>
		public GinjaVertexSetArray(int size) : base(_labelPrefix.GenerateIdentifier(), size) { }

		/// <summary>
		/// Creates a new vertex set array with preexisting vertex sets
		/// </summary>
		public GinjaVertexSetArray(IEnumerable<GinjaVertexSet> vertexSets) : base(_labelPrefix.GenerateIdentifier(), [.. vertexSets]) { }


		void IBinarySerializable<IOContext>.Read(BinaryObjectReader reader, IOContext context)
		{
			List<GinjaVertexSet> sets = [];

			while(reader.ReadObject<GinjaVertexSet, IOContext>(context) is GinjaVertexSet vertexSet && vertexSet.Type != GinjaVertexType.End)
			{
				sets.Add(vertexSet);
			}

			Array = [.. sets];
		}

		void IBinarySerializable<IOContext>.Write(BinaryObjectWriter writer, IOContext context)
		{
			foreach(GinjaVertexSet vertexSet in this)
			{
				writer.WriteObject(vertexSet, context);
			}

			writer.WriteObject(GinjaVertexSet.EndVertexSet);
		}
	}
}
