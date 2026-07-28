using SA3D.Modeling.Mesh.Chunk;
using SA3D.Modeling.Structs;
using SA3D.Modeling.Mesh;
using System.Linq;

namespace SA3D.Modeling.ObjectData.Structs
{
	/// <summary>
	/// Ascii context for when serializing a model
	/// </summary>
	public struct ModelAsciiIOContext
	{
		/// <summary>
		/// Base context
		/// </summary>
		public AsciiIOContext BaseContext { get; set; }

		/// <summary>
		/// Format that data is serialized with
		/// </summary>
		public Format Format { get; set; }

		/// <summary>
		/// Whether any node in the model has quaternions
		/// </summary>
		public bool HasQuaternions { get; set; }

		/// <summary>
		/// Whether any chunk mesh in the model uses version 2 weights
		/// </summary>
		public bool UseVersion2Weights { get; set; }

		/// <summary>
		/// Create a model ascii context from a model
		/// </summary>
		/// <param name="format">The format of the model. Determines format based on model if null</param>
		/// <param name="model"></param>
		/// <param name="baseContext"></param>
		/// <returns></returns>
		public static ModelAsciiIOContext FromModel(Format? format, Node model, AsciiIOContext baseContext)
		{
			format ??= model.GetMeshFormat()?.ToFormat() ?? Format.Chunk;
			bool hasQuaternions = model.GetTreeNodeEnumerable().Any(x => x.UseQuaternionRotation);

			bool useVersion2Weights = false;
			if(format is Format.Chunk)
			{
				useVersion2Weights = model.GetTreeMeshDataEnumerable()
					.OfType<ChunkMesh>()
					.Any(x => x.VertexChunks?
						.Any(x => x.Type.CheckHasAttributes() && x.Vertices
							.Any(x => x.Attributes > 0x100_0000) // checking if the second weight byte is used
						) == true
					);
			}

			return new()
			{
				Format = format.Value,
				BaseContext = baseContext,
				HasQuaternions = hasQuaternions,
				UseVersion2Weights = useVersion2Weights
			};
		}
	}
}
