using SA3D.Common;
using SA3D.Modeling.Mesh.Weighted;
using SA3D.Modeling.ObjectData;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Converters
{
	internal abstract class OffsetableAttachConverter<TResult> where TResult : IOffsetableAttachResult
	{
		protected abstract TResult ConvertWeightless(WeightedMesh wba, bool optimize);

		protected abstract TResult ConvertWeighted(WeightedMesh wba, bool optimize);

		protected abstract void CorrectSpace(Attach attach, Matrix4x4 vertexMatrix);

		protected abstract TResult WeightedClone(string label, int vertexCount, int[] attachIndices, Attach[] attaches);

		protected abstract Attach CombineAttaches(List<Attach> attaches, string label);

		public void Convert(Node model, WeightedMesh[] meshData, bool optimize)
		{
			List<TResult> results = new();
			(Node node, Matrix4x4 worldMatrix)[] nodeMatrices = model.GetWorldMatrixTree();

			foreach(WeightedMesh wba in meshData)
			{
				if(!wba.IsWeighted)
				{
					results.Add(ConvertWeightless(wba, optimize));
				}
				else
				{
					TResult result = ConvertWeighted(wba, optimize);

					foreach(int rootIndex in wba.RootIndices)
					{
						int[] attachIndices = new int[result.AttachIndices.Length];
						Attach[] attaches = result.Attaches.ContentClone();

						Matrix4x4 baseMatrix = nodeMatrices[rootIndex].worldMatrix;

						for(int i = 0; i < attachIndices.Length; i++)
						{
							int nodeIndex = result.AttachIndices[i] + rootIndex;
							attachIndices[i] = nodeIndex;

							Matrix4x4 nodeMatrix = nodeMatrices[nodeIndex].worldMatrix;
							Matrix4x4.Invert(nodeMatrix, out Matrix4x4 invNodeMatrix);
							Matrix4x4 vertexMatrix = baseMatrix * invNodeMatrix;

							CorrectSpace(attaches[i], vertexMatrix);
						}

						string label = result.Label;
						if(wba.RootIndices.Count > 1)
						{
							label += "_" + rootIndex;
						}

						results.Add(WeightedClone(
							label,
							result.VertexCount,
							attachIndices,
							attaches));
					}
				}
			}

			IOffsetableAttachResult.PlanVertexOffsets(results.ToArray());

			List<Attach>[] nodeAttaches = new List<Attach>[nodeMatrices.Length];
			for(int i = 0; i < nodeAttaches.Length; i++)
			{
				nodeAttaches[i] = new();
			}

			foreach(TResult result in results)
			{
				for(int i = 0; i < result.AttachIndices.Length; i++)
				{
					nodeAttaches[result.AttachIndices[i]].Add(result.Attaches[i]);

					string attachLabel = result.Label;
					if(result.Attaches.Length > 1)
					{
						attachLabel += "_" + i;
					}

					result.Attaches[i].Label = attachLabel;
				}
			}

			model.ClearAttachesFromTree();

			for(int i = 0; i < nodeAttaches.Length; i++)
			{
				List<Attach> chunks = nodeAttaches[i];
				Node node = nodeMatrices[i].node;
				if(chunks.Count == 1)
				{
					node.Attach = chunks[0];
				}
				else
				{
					string combinedLabel = string.Join('_', chunks.Select(x => x.Label));
					node.Attach = CombineAttaches(chunks, combinedLabel);
				}
			}
		}
	}
}
