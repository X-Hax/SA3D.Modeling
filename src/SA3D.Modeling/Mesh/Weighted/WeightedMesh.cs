using SA3D.Common;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Mesh.Converters;
using SA3D.Modeling.ObjectData;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Modeling.Mesh.Weighted
{
	/// <summary>
	/// Helper class for model conversions
	/// </summary>
	public class WeightedMesh : ICloneable
	{
		/// <summary>
		/// Label of the weighted mesh, if available.
		/// </summary>
		public string? Label { get; set; }

		/// <summary>
		/// Vertex data.
		/// </summary>
		public WeightedVertex[] Vertices { get; }

		/// <summary>
		/// Triangle sets.
		/// </summary>
		public BufferCorner[][] TriangleSets { get; private set; }

		/// <summary>
		/// Materials for each triangle set.
		/// </summary>
		public BufferMaterial[] Materials { get; private set; }

		/// <summary>
		/// Indices to nodes at which this mesh is used.
		/// <br/> That means if this mesh has 2 root nodes, then it is rendered 2 times; Once for every root node.
		/// </summary>
		public HashSet<int> RootIndices { get; }

		/// <summary>
		/// Indices to nodes influencing this mesh, relative to the root node.
		/// </summary>
		public SortedSet<int> DependingNodeIndices { get; }

		/// <summary>
		/// Whether the model is influenced by multiple nodes.
		/// </summary>
		public bool IsWeighted => DependingNodeIndices.Count > 0;

		/// <summary>
		/// Whether the model has colors (even if all colors are white).
		/// </summary>
		public bool HasColors { get; }

		/// <summary>
		/// If true, when converting to Chunk, the final model keeps vertex colors, 
		/// even if weights are involved (binary weights are produced as a result).
		/// </summary>
		public bool ForceVertexColors { get; set; }

		/// <summary>
		/// Whether Specular colors should be written to the models.
		/// </summary>
		public bool WriteSpecular { get; set; }


		internal WeightedMesh(
			WeightedVertex[] vertices,
			BufferCorner[][] triangleSets,
			BufferMaterial[] materials,
			HashSet<int> rootIndices,
			SortedSet<int> dependingNodeIndices,
			bool hasColors)
		{
			Vertices = vertices;
			TriangleSets = triangleSets;
			Materials = materials;
			RootIndices = rootIndices;
			DependingNodeIndices = dependingNodeIndices;
			HasColors = hasColors;
		}

		/// <summary>
		/// Creates a new weighted mesh from model data.
		/// </summary>
		/// <param name="vertices">Vertex data.</param>
		/// <param name="triangleSets">Triangle sets.</param>
		/// <param name="materials">Materials for each triangle set.</param>
		/// <param name="hasColors">Whether the model contains colors.</param>
		/// <returns>The created weighted mesh.</returns>
		public static WeightedMesh Create(
			WeightedVertex[] vertices,
			BufferCorner[][] triangleSets,
			BufferMaterial[] materials,
			bool hasColors)
		{
			SortedSet<int> dependingNodes = new();

			bool storesWeights = vertices.Any(x => x.Weights != null);

			if(storesWeights && vertices.Any(x => x.Weights == null))
			{
				throw new ArgumentException("Vertex weights are inconsistent! All weights must be either null or not!");
			}

			if(storesWeights)
			{
				foreach(WeightedVertex vtx in vertices)
				{
					for(int i = 0; i < vtx.Weights!.Length; i++)
					{
						if(vtx.Weights[i] > 0)
						{
							dependingNodes.Add(i);
						}
					}
				}

				// Only root node is used, we can remove the weight arrays.
				if(dependingNodes.Count == 1 && dependingNodes.Contains(0))
				{
					dependingNodes.Clear();
					WeightedVertex[] cleanedVerts = new WeightedVertex[vertices.Length];

					for(int i = 0; i < vertices.Length; i++)
					{
						WeightedVertex vert = vertices[i];
						cleanedVerts[i] = new(vert.Position, vert.Normal);
					}

					vertices = cleanedVerts;
				}
			}

			return new(
				vertices,
				triangleSets,
				materials,
				new(),
				dependingNodes,
				hasColors);
		}


		#region Conversion

		/// <summary>
		/// Attempts to convert an attach to a weighted mesh in local space. Weight information that is dependend on other attaches gets lost.
		/// </summary>
		/// <param name="attach">Attach to convert.</param>
		/// <param name="bufferMode">How to handle buffered mesh data of the attach.</param>
		/// <returns>The converted weight mesh.</returns>
		public static WeightedMesh FromAttach(Attach attach, BufferMode bufferMode)
		{
			Node dummy = new()
			{
				Attach = attach
			};

			WeightedMesh weightedMesh = FromModel(dummy, bufferMode)[0];
			weightedMesh.RootIndices.Clear();

			return weightedMesh;
		}

		/// <summary>
		/// Converts buffer meshdata from attaches of a model to weighted meshes.
		/// </summary>
		/// <param name="model">Model to convert.</param>
		/// <param name="bufferMode">How to handle buffered mesh data of the model.</param>
		/// <returns>The converted weighted meshes.</returns>
		public static WeightedMesh[] FromModel(Node model, BufferMode bufferMode)
		{
			switch(bufferMode)
			{
				case BufferMode.Generate:
					model.BufferMeshData(false);
					break;
				case BufferMode.GenerateOptimized:
					model.BufferMeshData(true);
					break;
				case BufferMode.None:
				default:
					break;
			}

			AttachFormat? attachFormat = model.GetAttachFormat();
			bool hasWelding = model.GetTreeNodeEnumerable().Any(x => x.Welding != null);

			WeightedMesh[] result;

			if(attachFormat == AttachFormat.BASIC && hasWelding)
			{
				Node[][] weldingGroups = model.GetTreeWeldingGroups(false);
				result = WeldedBasicConverter.CreateWeightedFromWeldedBasicModel(model, weldingGroups);
			}
			else
			{
				result = ToWeightedConverter.ConvertToWeighted(model);
			}

			EnsurePolygonsValid(ref result);
			return result;
		}

		/// <summary>
		/// Attempts to convert the weighted mesh to a standalone attach.
		/// </summary>
		/// <param name="format">Attach format to convert to.</param>
		/// <param name="optimize">Whether to optimize the mesh info.</param>
		/// <param name="ignoreWeights">Whether to ignore losing weight information.</param>
		/// <returns>The converted attach.</returns>
		/// <exception cref="InvalidOperationException"></exception>
		public Attach ToAttach(AttachFormat format, bool optimize, bool ignoreWeights)
		{
			HashSet<int> backup = new(RootIndices);
			RootIndices.Clear();
			RootIndices.Add(0);

			Node dummy = new();
			ToModel(dummy, new[] { this }, format, optimize, ignoreWeights);

			RootIndices.Clear();
			RootIndices.UnionWith(backup);

			return dummy.Attach ?? throw new InvalidOperationException("Attach failed to convert.");
		}

		/// <summary>
		/// Converts weighted meshes to a given attach format and assigns them to a model.
		/// </summary>
		/// <param name="model">Model to assign the converted meshes to.</param>
		/// <param name="meshes">Meshes to convert.</param>
		/// <param name="format">Attach format to convert to.</param>
		/// <param name="optimize">Whether to optimize the mesh info.</param>
		/// <param name="ignoreWeights">Whether to ignore losing weight information.</param>
		public static void ToModel(Node model, WeightedMesh[] meshes, AttachFormat format, bool optimize, bool ignoreWeights = false)
		{
			EnsurePolygonsValid(ref meshes);

			switch(format)
			{
				case AttachFormat.Buffer:
					FromWeightedConverter.Convert(model, meshes, optimize);
					break;
				case AttachFormat.BASIC:
					BasicConverter.ConvertWeightedToBasic(model, meshes, optimize, ignoreWeights);
					break;
				case AttachFormat.CHUNK:
					ChunkConverter.ConvertWeightedToChunk(model, meshes, optimize);
					break;
				case AttachFormat.GC:
					GCConverter.ConvertWeightedToGC(model, meshes, optimize, ignoreWeights);
					break;
				default:
					throw new ArgumentException("Invalid attach format.", nameof(format));
			}
		}

		#endregion

		#region Utilities

		/// <summary>
		/// Checks for degenerate triangles and removes them.
		/// </summary>
		/// <returns>Whether the model contained degenerate triangles that have been removed.</returns>
		public bool EnsurePolygonsValid()
		{
			List<BufferCorner[]> newTriangleSets = new();
			List<BufferMaterial> newMaterials = new();
			bool changed = false;

			for(int i = 0; i < TriangleSets.Length; i++)
			{
				List<int> degenerates = new();

				BufferCorner[] triangles = TriangleSets[i];
				for(int j = 0; j < triangles.Length; j += 3)
				{
					int index1 = triangles[j].VertexIndex;
					int index2 = triangles[j + 1].VertexIndex;
					int index3 = triangles[j + 2].VertexIndex;

					if(index1 == index2
						|| index2 == index3
						|| index3 == index1)
					{
						degenerates.Add(j);
					}
				}

				changed |= degenerates.Count > 0;

				if(degenerates.Count == triangles.Length / 3)
				{
					continue;
				}

				newMaterials.Add(Materials[i]);

				if(degenerates.Count == 0)
				{
					newTriangleSets.Add(triangles);
				}
				else
				{
					List<BufferCorner> newTriangles = new();
					for(int j = 0; j < triangles.Length; j += 3)
					{
						if(degenerates.Count > 0 && degenerates[0] == j)
						{
							degenerates.RemoveAt(0);
							continue;
						}

						newTriangles.Add(triangles[j]);
						newTriangles.Add(triangles[j + 1]);
						newTriangles.Add(triangles[j + 2]);
					}

					newTriangleSets.Add(newTriangles.ToArray());
				}
			}

			if(changed)
			{
				TriangleSets = newTriangleSets.ToArray();
				Materials = newMaterials.ToArray();
			}

			return changed;
		}

		/// <summary>
		/// Checks every mesh for degenerate triangles and removes them.
		/// </summary>
		/// <param name="meshes">The meshes to correct.</param>
		public static void EnsurePolygonsValid(ref WeightedMesh[] meshes)
		{
			List<WeightedMesh> newWBAs = new();
			bool changed = false;

			foreach(WeightedMesh wba in meshes)
			{
				if(wba.EnsurePolygonsValid() && wba.TriangleSets.Length == 0)
				{
					changed = true;
					continue;
				}

				newWBAs.Add(wba);
			}

			if(changed)
			{
				meshes = newWBAs.ToArray();
			}
		}

		/// <summary>
		/// Combines vertex and polygon data of multiple weighted meshes together.
		/// </summary>
		/// <param name="meshes">The weighted meshes to merge.</param>
		/// <returns>The merged mesh.</returns>
		/// <exception cref="ArgumentException"></exception>
		public static WeightedMesh MergeWeightedMeshes(IEnumerable<WeightedMesh> meshes)
		{
			int count = meshes.Count();
			if(count == 0)
			{
				throw new ArgumentException("No attaches in enumerable", nameof(meshes));
			}
			else if(count == 1)
			{
				return meshes.First();
			}

			List<WeightedVertex> vertices = new();
			List<BufferCorner[]> corners = new();
			List<BufferMaterial> materials = new();
			SortedSet<int> dependingNodes = new();
			bool hasColors = false;

			int maxNode = meshes.Max(x => x.DependingNodeIndices.Max);
			int weightNum = maxNode + 1;

			foreach(WeightedMesh mesh in meshes)
			{
				materials.AddRange(mesh.Materials);
				hasColors |= mesh.HasColors;

				if(vertices.Count > 0)
				{
					int vertexOffset = vertices.Count;
					foreach(BufferCorner[] wbaTriangles in mesh.TriangleSets)
					{
						BufferCorner[] outTriangles = new BufferCorner[wbaTriangles.Length];
						for(int i = 0; i < wbaTriangles.Length; i++)
						{
							outTriangles[i].VertexIndex = (ushort)(wbaTriangles[i].VertexIndex + vertexOffset);
						}

						corners.Add(outTriangles);
					}
				}
				else
				{
					corners.AddRange(mesh.TriangleSets.ContentClone());
				}

				if(maxNode == 0)
				{
					vertices.AddRange(mesh.Vertices);
				}
				else if(maxNode == mesh.DependingNodeIndices.Max)
				{
					vertices.AddRange(mesh.Vertices.ContentClone());
					dependingNodes.UnionWith(mesh.DependingNodeIndices);
				}
				else if(mesh.DependingNodeIndices.Count == 0)
				{
					for(int i = 0; i < mesh.Vertices.Length; i++)
					{
						WeightedVertex vert = mesh.Vertices[i];
						WeightedVertex newVert = new(vert.Position, vert.Normal, weightNum);
						newVert.Weights![0] = 1;
						vertices.Add(newVert);
					}

					dependingNodes.Add(0);
				}
				else
				{
					for(int i = 0; i < mesh.Vertices.Length; i++)
					{
						WeightedVertex vert = mesh.Vertices[i];
						WeightedVertex newVert = new(vert.Position, vert.Normal, weightNum);

						Array.Copy(vert.Weights!, newVert.Weights!, vert.Weights!.Length);

						vertices.Add(newVert);
					}

					dependingNodes.UnionWith(mesh.DependingNodeIndices);
				}
			}

			return new(
				vertices.ToArray(),
				corners.ToArray(),
				materials.ToArray(),
				new(),
				dependingNodes,
				hasColors);
		}

		/// <summary>
		/// Merges weighted meshes at their specified roots.
		/// </summary>
		/// <param name="meshes">The meshes to merge.</param>
		/// <returns>The merged meshes.</returns>
		public static WeightedMesh[] MergeAtRoots(WeightedMesh[] meshes)
		{
			if(meshes.Length == 0)
			{
				return meshes;
			}

			List<WeightedMesh>?[] sharedRoots = new List<WeightedMesh>[
				meshes.Max(x => x.RootIndices.Count == 0 ? 0 : x.RootIndices.Max()) + 1];

			foreach(WeightedMesh mesh in meshes)
			{
				foreach(int rootIndex in mesh.RootIndices)
				{
					List<WeightedMesh>? list = sharedRoots[rootIndex];

					if(list == null)
					{
						list = new();
						sharedRoots[rootIndex] = list;
					}

					list.Add(mesh);
				}
			}

			if(!sharedRoots.Any(x => x?.Count > 1))
			{
				return meshes;
			}

			List<WeightedMesh> result = new();

			for(int i = 0; i < sharedRoots.Length; i++)
			{
				List<WeightedMesh>? item = sharedRoots[i];
				if(item == null)
				{
					continue;
				}

				if(item.Count == 1)
				{
					if(item[0].RootIndices.Count == 1)
					{
						result.Add(item[0]);
					}
					else
					{
						// TODO this should probably be improved for edge cases where weighted meshes thrice, of which 2 instances can be reused.
						WeightedMesh clonedMesh = item[0].Clone();
						clonedMesh.RootIndices.Clear();
						clonedMesh.RootIndices.Add(i);
						result.Add(clonedMesh);
					}

					continue;
				}

				WeightedMesh merged = MergeWeightedMeshes(item);
				merged.RootIndices.Add(i);
				result.Add(merged);
			}

			return result.ToArray();
		}


		object ICloneable.Clone()
		{
			return Clone();
		}

		/// <summary>
		/// Creates a deep clone of the mesh.
		/// </summary>
		/// <returns>The cloned mesh.</returns>
		public WeightedMesh Clone()
		{
			return new(
				Vertices.ContentClone(),
				TriangleSets.ContentClone(),
				Materials.ToArray(),
				new(RootIndices),
				new(DependingNodeIndices),
				HasColors);
		}

		#endregion
	}
}
