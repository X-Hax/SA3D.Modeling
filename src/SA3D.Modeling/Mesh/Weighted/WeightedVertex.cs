using System;
using System.Collections.Generic;
using System.Numerics;

namespace SA3D.Modeling.Mesh.Weighted
{
	/// <summary>
	/// Vertex with a position, direction and weight values.
	/// </summary>
	public struct WeightedVertex : IEquatable<WeightedVertex>, ICloneable
	{
		/// <summary>
		/// Position in 3D space.
		/// </summary>
		public Vector3 Position { get; set; }

		/// <summary>
		/// Normalized direction.
		/// </summary>
		public Vector3 Normal { get; set; }

		/// <summary>
		/// Weights per node relative to the base node of the weighted mesh that this vertex belongs to.
		/// </summary>
		public float[]? Weights { get; }


		/// <summary>
		/// Create a new vertex with no weight values.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="normal">Normalized direction.</param>
		public WeightedVertex(Vector3 position, Vector3 normal)
		{
			Position = position;
			Normal = normal;
			Weights = null;
		}

		/// <summary>
		/// Create a new vertex with empty weight values.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="normal">Normalized direction.</param>
		/// <param name="nodeCount">Number of nodes that this vertex covers.</param>
		public WeightedVertex(Vector3 position, Vector3 normal, int nodeCount)
		{
			Position = position;
			Normal = normal;
			Weights = new float[nodeCount];
		}

		/// <summary>
		/// Create a new vertex.
		/// </summary>
		/// <param name="position">Position in 3D space.</param>
		/// <param name="normal">Normalized direction.</param>
		/// <param name="weights">Weight values to use.</param>
		public WeightedVertex(Vector3 position, Vector3 normal, float[]? weights)
		{
			Position = position;
			Normal = normal;
			Weights = weights;
		}


		/// <summary>
		/// Returns a clone of the vertex with a normalized normal
		/// </summary>
		/// <returns></returns>
		public readonly WeightedVertex Normalized()
		{
			return new(Position, Vector3.Normalize(Normal), (float[]?)Weights?.Clone());
		}

		/// <summary>
		/// Checks whether the vertex has more than one weight.
		/// </summary>
		/// <returns>Whether the vertex has more than one weight.</returns>
		public readonly bool IsWeighted()
		{
			if(Weights == null)
			{
				return false;
			}

			bool oneWeight = false;
			for(int i = 0; i < Weights.Length; i++)
			{
				if(Weights[i] > 0f)
				{
					if(oneWeight)
					{
						return true;
					}

					oneWeight = true;
				}
			}

			return false;
		}

		/// <summary>
		/// Gets the number of weight values greater than 0.
		/// </summary>
		/// <returns>The number of weight values greater than 0.</returns>
		public readonly int GetWeightCount()
		{
			if(Weights == null)
			{
				return 0;
			}

			int count = 0;
			for(int i = 0; i < Weights.Length; i++)
			{
				float weight = Weights[i];
				if(weight > 0f)
				{
					count++;
				}
			}

			return count;
		}

		/// <summary>
		/// Compiles an array of node indices mapped to their weight values for weights greater than 0.
		/// </summary>
		/// <returns>The mapped weight values.</returns>
		/// <exception cref="InvalidOperationException"/>
		public readonly (int nodeIndex, float weight)[] GetWeightMap()
		{
			if(Weights == null)
			{
				throw new InvalidOperationException();
			}

			List<(int nodeIndex, float weight)> result = new();
			for(int i = 0; i < Weights.Length; i++)
			{
				float weight = Weights[i];
				if(weight > 0f)
				{
					result.Add((i, weight));
				}
			}

			return result.ToArray();
		}

		/// <summary>
		/// Determines the index of the first weight greater than 0.
		/// </summary>
		/// <returns>The index of the first weight greater than 0</returns>
		/// <exception cref="InvalidOperationException"/>
		public readonly int GetFirstWeightIndex()
		{
			if(Weights == null)
			{
				throw new InvalidOperationException();
			}

			for(int i = 0; i < Weights.Length; i++)
			{
				if(Weights[i] > 0f)
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Determines the index of the last weight greater than 0.
		/// </summary>
		/// <returns>The index of the last weight greater than 0</returns>
		/// <exception cref="InvalidOperationException"/>
		public readonly int GetLastWeightIndex()
		{
			if(Weights == null)
			{
				throw new InvalidOperationException();
			}

			for(int i = Weights.Length - 1; i >= 0; i--)
			{
				if(Weights[i] > 0f)
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Determines the index of the greatest weight greater than 0. If all weights are 0, it will return -1.
		/// </summary>
		/// <returns>The index of the greatest weight greater than 0.</returns>
		/// <exception cref="InvalidOperationException"/>
		public readonly int GetMaxWeightIndex()
		{
			if(Weights == null)
			{
				throw new InvalidOperationException();
			}

			int result = -1;
			float weightCheck = 0;
			for(int i = 0; i < Weights.Length; i++)
			{
				float weight = Weights[i];
				if(weight > weightCheck)
				{
					weightCheck = weight;
					result = i;
				}
			}

			return result;
		}

		/// <summary>
		/// Normalizes the weights.
		/// </summary>
		public readonly void NormalizeWeights()
		{
			if(Weights == null)
			{
				return;
			}

			float total = 0;

			for(int i = 0; i < Weights.Length; i++)
			{
				total += Weights[i];
			}

			if(total == 1f)
			{
				return;
			}

			float fac = 1f / total;
			for(int i = 0; i < Weights.Length; i++)
			{
				Weights[i] *= fac;
			}
		}

		/// <summary>
		/// Normalizes and rounds the weights so their packed (0 - 255 instead of 0.0 - 1.0) values add up to exactly 255.
		/// </summary>
		public readonly void NormalizedPackedWeights()
		{
			const float fac = 1f / 255f;

			if(Weights == null)
			{
				return;
			}

			NormalizeWeights();

			int[] packedWeights = new int[Weights.Length];
			int total = 0;
			int weightCount = 0;

			for(int i = 0; i < Weights.Length; i++)
			{
				float weight = Weights[i];
				if(weight == 0)
				{
					continue;
				}

				int packedWeight = (int)Math.Round(weight * 255f);
				packedWeights[i] = packedWeight;
				total += packedWeight;
				weightCount++;
			}

			if(total != 255)
			{
				int dif = 255 - total;

				int lowerDifPerVert = dif / weightCount;
				int upperDifPerVert = lowerDifPerVert + (dif > 0 ? 1 : -1);
				int upperDifVerts = int.Abs(dif) % weightCount;

				int weightNum = 0;
				for(int i = 0; i < packedWeights.Length; i++)
				{
					int packedWeight = packedWeights[i];
					if(packedWeight != 0)
					{
						if(weightNum < upperDifVerts)
						{
							packedWeight += upperDifPerVert;
						}
						else
						{
							packedWeight += lowerDifPerVert;
						}

						packedWeights[i] = packedWeight;

						weightNum++;
					}
				}
			}

			for(int i = 0; i < Weights.Length; i++)
			{
				Weights[i] = packedWeights[i] * fac;
			}
		}

		#region Comparisons

		/// <inheritdoc/>
		public override readonly bool Equals(object? obj)
		{
			return obj is WeightedVertex vertex &&
				   Position.Equals(vertex.Position) &&
				   Normal.Equals(vertex.Normal) &&
				   EqualityComparer<float[]?>.Default.Equals(Weights, vertex.Weights);
		}

		/// <inheritdoc/>
		public override readonly int GetHashCode()
		{
			return HashCode.Combine(Position, Normal, Weights);
		}

		readonly bool IEquatable<WeightedVertex>.Equals(WeightedVertex other)
		{
			return Equals(other);
		}

		/// <summary>
		/// Compares two weighted vertices for equality.
		/// </summary>
		/// <param name="left">Lefthand vertex.</param>
		/// <param name="right">Righthand vertex.</param>
		/// <returns>Whether the vertices are equal.</returns>
		public static bool operator ==(WeightedVertex left, WeightedVertex right)
		{
			return left.Equals(right);
		}

		/// <summary>
		/// Compares two weighted vertices for inequality.
		/// </summary>
		/// <param name="left">Lefthand vertex.</param>
		/// <param name="right">Righthand vertex.</param>
		/// <returns>Whether the vertices are inequal.</returns>
		public static bool operator !=(WeightedVertex left, WeightedVertex right)
		{
			return !(left == right);
		}

		#endregion

		/// <summary>
		/// Creates a clone of the vertex.
		/// </summary>
		/// <returns>The cloned vertex.</returns>
		public readonly WeightedVertex Clone()
		{
			return new(Position, Normal, (float[]?)Weights?.Clone());
		}

		readonly object ICloneable.Clone()
		{
			return Clone();
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			if(Weights == null)
			{
				return "NULL";
			}

			int weightCount = 0;
			string result = "";

			for(int i = 0; i < Weights.Length; i++)
			{
				float weight = Weights[i];
				if(weight == 0f)
				{
					continue;
				}

				weightCount++;
				result += i + ", ";
			}

			return $"{weightCount} - {result}";
		}

	}
}
