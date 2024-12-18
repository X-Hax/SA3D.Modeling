﻿using SA3D.Common;
using SA3D.Common.Lookup;
using SA3D.Modeling.Mesh.Buffer;
using SA3D.Modeling.Mesh.Converters;
using SA3D.Modeling.Mesh.Gamecube;
using SA3D.Modeling.Mesh.Gamecube.Enums;
using SA3D.Modeling.Mesh.Gamecube.Parameters;
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
		/// Whether the model has normals (even if all normals point in the same direction).
		/// </summary>
		public bool HasNormals { get; }

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

		/// <summary>
		/// Texture coordinate precision level. Every level higher allows for double the previous precision, but also reduces range by half.
		/// <br/> Supported:
		/// <br/> - Basic: 0
		/// <br/> - Chunk: 0 or 2
		/// <br/> - Gamecube: 0-7
		/// <br/> - Buffer: Unaffected
		/// <br/>
		/// <br/> When converting, the lowest supported precision is used.
		/// </summary>
		public byte TexcoordPrecisionLevel { get; set; }

		/// <summary>
		/// Resulting mesh data will have no bounds calculated.
		/// </summary>
		public bool NoBounds { get; set; }

		internal WeightedMesh(
			WeightedVertex[] vertices,
			BufferCorner[][] triangleSets,
			BufferMaterial[] materials,
			HashSet<int> rootIndices,
			SortedSet<int> dependingNodeIndices,
			bool hasColors,
			bool hasNormals)
		{
			Vertices = vertices;
			TriangleSets = triangleSets;
			Materials = materials;
			RootIndices = rootIndices;
			DependingNodeIndices = dependingNodeIndices;
			HasColors = hasColors;
			HasNormals = hasNormals;
		}

		/// <summary>
		/// Creates a new weighted mesh from model data.
		/// </summary>
		/// <param name="vertices">Vertex data.</param>
		/// <param name="triangleSets">Triangle sets.</param>
		/// <param name="materials">Materials for each triangle set.</param>
		/// <param name="hasColors">Whether the model contains colors.</param>
		/// <param name="hasNormals">Whether the model contains normals.</param>
		/// <returns>The created weighted mesh.</returns>
		public static WeightedMesh Create(
			WeightedVertex[] vertices,
			BufferCorner[][] triangleSets,
			BufferMaterial[] materials,
			bool hasColors,
			bool hasNormals)
		{
			SortedSet<int> dependingNodes = [];

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
				[],
				dependingNodes,
				hasColors,
				hasNormals);
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
		/// <param name="bufferMode">How to handle buffered mesh data of the model (if needed for conversion).</param>
		/// <returns>The converted weighted meshes.</returns>
		public static WeightedMesh[] FromModel(Node model, BufferMode bufferMode)
		{
			AttachFormat? attachFormat = model.GetAttachFormat();
			if(attachFormat == null)
			{
				return Array.Empty<WeightedMesh>();
			}

			bool hasWelding = model.GetTreeNodeEnumerable().Any(x => x.Welding != null);

			WeightedMesh[] result;

			if(attachFormat == AttachFormat.BASIC && hasWelding)
			{
				Node[][] weldingGroups = model.GetTreeWeldingGroups(false);
				result = FromWeldedBasicConverter.CreateWeightedFromWeldedBasicModel(model, weldingGroups, bufferMode);
			}
			else
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

				result = ToWeightedConverter.ConvertToWeighted(model);
				GetTexcoordPrecisionLevel(model, result, attachFormat.Value);
			}

			EnsurePolygonsValid(ref result);
			return result;
		}

		private static void GetTexcoordPrecisionLevel(Node model, WeightedMesh[] meshes, AttachFormat format)
		{
			Node[] nodes = model.GetTreeNodes();

			switch(format)
			{
				case AttachFormat.GC:
					foreach(WeightedMesh mesh in meshes)
					{
						GCAttach atc = (GCAttach)nodes[mesh.RootIndices.First()].Attach!;

						byte texcoordPrecision = byte.MaxValue;
						if(atc.OpaqueMeshes.Length != 0)
						{
							GCVertexFormatParameter[] vtxParams = atc.OpaqueMeshes
								.SelectMany(x => x.Parameters)
								.Select(x => x)
								.OfType<GCVertexFormatParameter>()
								.Where(x => x.VertexType == GCVertexType.TexCoord0)
								.ToArray();

							if(vtxParams.Length != 1)
							{
								continue;
							}

							texcoordPrecision = (vtxParams[0].Attributes & 0x8) != 0
								? (byte)(vtxParams[0].Attributes & 0x7)
								: (byte)0;
						}

						if(atc.TransparentMeshes.Length != 0)
						{
							GCVertexFormatParameter[] vtxParams = atc.TransparentMeshes
								.SelectMany(x => x.Parameters)
								.Select(x => x)
								.OfType<GCVertexFormatParameter>()
								.Where(x => x.VertexType == GCVertexType.TexCoord0)
								.ToArray();

							if(vtxParams.Length != 1)
							{
								continue;
							}

							byte newPrecision = (vtxParams[0].Attributes & 0x8) != 0
								? (byte)(vtxParams[0].Attributes & 0x7)
								: (byte)0;

							if(texcoordPrecision == byte.MaxValue)
							{
								texcoordPrecision = newPrecision;
							}
							else if(texcoordPrecision != newPrecision)
							{
								continue;
							}
						}

						mesh.TexcoordPrecisionLevel = texcoordPrecision;
					}

					break;
				case AttachFormat.CHUNK: // theoretically you can do it for chunk, but its very annoying
				case AttachFormat.Buffer:
				case AttachFormat.BASIC:
				default:
					break;
			}

		}

		/// <summary>
		/// Attempts to convert the weighted mesh to a standalone attach.
		/// </summary>
		/// <param name="format">Attach format to convert to.</param>
		/// <param name="optimize">Whether to optimize the mesh info.</param>
		/// <param name="vertexMapping">Vertex index mapping for formats, where vertices may rearrange. 
		/// <br/> If a meshes vertices have been rearranged (weighted meshes excluded), then the mapping is set up like: 
		/// <code>vertexMapping[mesh_index][new_vertex_index] = old_vertex_index</code></param>
		/// <returns>The converted attach.</returns>
		/// <exception cref="InvalidOperationException"></exception>
		public Attach ToAttach(AttachFormat format, bool optimize, out int[]?[] vertexMapping)
		{
			HashSet<int> backup = new(RootIndices);
			RootIndices.Clear();
			RootIndices.Add(0);

			Node dummy = new();
			ToModel(dummy, [this], format, optimize, out vertexMapping);

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
		/// <param name="vertexMapping">Vertex index mapping for formats, where vertices may rearrange. 
		/// <br/> If a meshes vertices have been rearranged (weighted meshes excluded), then the mapping is set up like: 
		/// <code>vertexMapping[mesh_index][new_vertex_index] = old_vertex_index</code></param>
		public static void ToModel(Node model, WeightedMesh[] meshes, AttachFormat format, bool optimize, out int[]?[] vertexMapping)
		{
			EnsurePolygonsValid(ref meshes);

			switch(format)
			{
				case AttachFormat.Buffer:
					FromWeightedConverter.Convert(model, meshes, optimize, out vertexMapping);
					break;
				case AttachFormat.BASIC:
					BasicConverter.ConvertWeightedToBasic(model, meshes, optimize);
					vertexMapping = new int[]?[meshes.Length];
					break;
				case AttachFormat.CHUNK:
					ChunkConverter.ConvertWeightedToChunk(model, meshes, optimize, out vertexMapping);
					break;
				case AttachFormat.GC:
					GCConverter.ConvertWeightedToGC(model, meshes, optimize);
					vertexMapping = new int[]?[meshes.Length];
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
			List<BufferCorner[]> newTriangleSets = [];
			List<BufferMaterial> newMaterials = [];
			bool changed = false;

			for(int i = 0; i < TriangleSets.Length; i++)
			{
				List<int> degenerates = [];

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
					List<BufferCorner> newTriangles = [];
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
			List<WeightedMesh> newWBAs = [];
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

			List<WeightedVertex> vertices = [];
			List<BufferCorner[]> corners = [];
			List<BufferMaterial> materials = [];
			SortedSet<int> dependingNodes = [];
			bool hasColors = false;
			bool hasNormals = false;

			int maxNode = meshes.Max(x => x.DependingNodeIndices.Max);
			int weightNum = maxNode + 1;

			foreach(WeightedMesh mesh in meshes)
			{
				materials.AddRange(mesh.Materials);
				hasColors |= mesh.HasColors;
				hasNormals |= mesh.HasNormals;

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
				[],
				dependingNodes,
				hasColors,
				hasNormals);
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
						list = [];
						sharedRoots[rootIndex] = list;
					}

					list.Add(mesh);
				}
			}

			if(!sharedRoots.Any(x => x?.Count > 1))
			{
				return meshes;
			}

			List<WeightedMesh> result = [];

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
				HasColors,
				HasNormals);
		}

		#endregion
	}
}
