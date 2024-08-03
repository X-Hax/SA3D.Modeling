using SA3D.Common;
using SA3D.Modeling.Mesh.Weighted;
using SA3D.Modeling.ObjectData;
using SA3D.Modeling.Structs;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Converters
{
	internal abstract class OffsetableAttachConverter<TResult> where TResult : IOffsetableAttachResult
	{
		protected abstract TResult ConvertWeightless(WeightedMesh wba, bool optimize, out int[]? vertexMapping);

		protected abstract TResult ConvertWeighted(WeightedMesh wba, bool optimize);

		protected abstract void CorrectSpace(Attach attach, Matrix4x4 vertexMatrix, Matrix4x4 normalMatrix);

		protected abstract TResult CreateWeightedResult(string label, int vertexCount, int[] attachIndices, Attach[] attaches);

		protected abstract Attach CombineAttaches(List<Attach> attaches, string label);

		public void Convert(Node model, WeightedMesh[] meshData, bool optimize, out int[]?[] vertexMapping)
		{
			List<TResult> results = [];
			vertexMapping = new int[]?[meshData.Length];
			(Node node, Matrix4x4 worldMatrix)[] nodeMatrices = model.GetWorldMatrixTree();

			for(int i = 0; i < meshData.Length; i++)
			{
				WeightedMesh wba = meshData[i];

				if(!wba.IsWeighted)
				{
					results.Add(ConvertWeightless(wba, optimize, out vertexMapping[i]));
				}
				else
				{
					TResult result = ConvertWeighted(wba, optimize);

					void CorrectVertices(int rootIndex, int[] attachIndices, Attach[] attaches)
					{
						Matrix4x4 baseMatrix = nodeMatrices[rootIndex].worldMatrix;

						for(int j = 0; j < attachIndices.Length; j++)
						{
							int nodeIndex = result.AttachIndices[j] + rootIndex;
							attachIndices[j] = nodeIndex;

							Matrix4x4 nodeMatrix = nodeMatrices[nodeIndex].worldMatrix;
							Matrix4x4.Invert(nodeMatrix, out Matrix4x4 invNodeMatrix);
							Matrix4x4 vertexMatrix = baseMatrix * invNodeMatrix;
							Matrix4x4 normalMatrix = vertexMatrix.GetNormalMatrix();

							CorrectSpace(attaches[j], vertexMatrix, normalMatrix);
							attaches[j].RecalculateBounds();
						}
					}

					if(wba.RootIndices.Count > 1)
					{
						foreach(int rootIndex in wba.RootIndices)
						{
							string label = result.Label + '_' + rootIndex;
							int[] attachIndices = new int[result.AttachIndices.Length];
							Attach[] attaches = result.Attaches.ContentClone();

							CorrectVertices(rootIndex, attachIndices, attaches);

							results.Add(CreateWeightedResult(
								label,
								result.VertexCount,
								attachIndices,
								attaches));
						}
					}
					else
					{
						int rootIndex = 0;
						if(wba.RootIndices.Count > 0)
						{
							rootIndex = wba.RootIndices.First();
						}

						CorrectVertices(rootIndex, result.AttachIndices, result.Attaches);
						results.Add(result);
					}

				}
			}

			IOffsetableAttachResult.PlanVertexOffsets(results.ToArray());

			List<Attach>[] nodeAttaches = new List<Attach>[nodeMatrices.Length];
			for(int i = 0; i < nodeAttaches.Length; i++)
			{
				nodeAttaches[i] = [];
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
			model.ClearWeldingsFromTree();

			for(int i = 0; i < nodeAttaches.Length; i++)
			{
				Node node = nodeMatrices[i].node;

				List<Attach> attaches = nodeAttaches[i];

				if(attaches.Count == 0)
				{
					continue;
				}

				if(attaches.Count == 1)
				{
					node.Attach = attaches[0];
				}
				else
				{
					string combinedLabel = string.Join('_', attaches.Select(x => x.Label));
					node.Attach = CombineAttaches(attaches, combinedLabel);
				}
			}
		}
	}
}
