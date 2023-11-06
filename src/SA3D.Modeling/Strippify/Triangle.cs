using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SA3D.Modeling.Strippify
{
	/// <summary>
	/// Triangle between three vertices
	/// </summary>
	internal class Triangle
	{
		/// <summary>
		/// The three vertices that the triangle consists of
		/// </summary>
		public Vertex[] Vertices { get; }

		/// <summary>
		/// The three edges that the triangle consists of
		/// </summary>
		public Edge[] Edges { get; }

		/// <summary>
		/// The triangles that the triangle borders through the edges
		/// </summary>
		public List<Triangle> Neighbours { get; }

		/// <summary>
		/// Whether the triangle was already used in the strip
		/// </summary>
		public bool Used { get; set; }

		/// <summary>
		/// Neighbouring triangles that havent been used yet
		/// </summary>
		public Triangle[] AvailableNeighbours
			=> Neighbours.Where(x => !x.Used).ToArray();

		public int AvailableNeighbourCount
			=> Neighbours.Count(x => !x.Used);

		/// <summary>
		/// Creates a new triangle from an index and three vertices, and adds newly created edges to an output list
		/// </summary>
		/// <param name="vertices">Vertices (must be 3)</param>
		/// <param name="outEdges">Edge list of the entire mesh, to add new edges to</param>
		/// <param name="raiseTopoError">Whether to raise a topo error when encountered.</param>
		public Triangle(Vertex[] vertices, List<Edge> outEdges, bool raiseTopoError)
		{
			Vertices = vertices;
			Edges = new Edge[3];
			Neighbours = new List<Triangle>();
			Used = false;

			Vertex prevVert = vertices[2];
			int i = 0;
			foreach(Vertex v in vertices)
			{
				v.Triangles.Add(this);
				Edges[i] = AddEdge(v, prevVert, outEdges, raiseTopoError);
				prevVert = v;

				i++;
			}

		}

		/// <summary>
		/// Creates a new edge for the triangle.
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <param name="edges"></param>
		/// <param name="raiseTopoError"></param>
		/// <returns></returns>
		private Edge AddEdge(Vertex v1, Vertex v2, List<Edge> edges, bool raiseTopoError)
		{
			if(!v1.IsConnectedWith(v2, out Edge? e))
			{
				e = v1.Connect(v2);
				edges.Add(e);
			}
			else if(raiseTopoError && e.Triangles.Count > 1)
			{
				throw new TopologyException("Some edge has more than 2 faces! Can't strippify!");
			}

			e.AddTriangle(this);

			return e;
		}

		/// <summary>
		/// Whether a vertex is contained in the triangle
		/// </summary>
		/// <param name="vert"></param>
		/// <returns></returns>
		public bool HasVertex(Vertex? vert)
		{
			return Vertices.Contains(vert);
		}

		/// <summary>
		/// Gets the third vertex in a triangle. <br/>
		/// If any of the two vertices are not part of the triangle, then the method returns null
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns></returns>
		public Vertex? GetThirdVertex(Vertex v1, Vertex v2)
		{
			if(Vertices.Contains(v1) && Vertices.Contains(v2))
			{
				foreach(Vertex v in Vertices)
				{
					if(v != v1 && v != v2)
					{
						return v;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Reurns a shared edge two triangles
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Edge? GetSharedEdge(Triangle other)
		{
			foreach(Edge e in Edges)
			{
				if(other.Edges.Contains(e))
				{
					return e;
				}
			}

			return null;
		}


		/// <summary>
		/// Gets the next triangle in a strip sequence (with swapping)
		/// </summary>
		/// <returns></returns>
		/// <exception cref="NullReferenceException"></exception>
		public Triangle? NextTriangleS()
		{
			Triangle[] trisToUse = AvailableNeighbours;

			if(trisToUse.Length == 0)
			{
				return null;
			}

			if(trisToUse.Length == 1)
			{
				return trisToUse[0];
			}

			int[] weights = new int[trisToUse.Length];
			int[] vConnection = new int[trisToUse.Length];
			int biggestConnection = 0;

			for(int i = 0; i < trisToUse.Length; i++)
			{
				Triangle t = trisToUse[i];

				if(t.AvailableNeighbourCount == 0)
				{
					return t;
				}

				weights[i] = t.AvailableNeighbourCount;
				Vertex[] eVerts = t.GetSharedEdge(this)?.Vertices ?? throw new NullReferenceException("No shared edge found");
				vConnection[i] = eVerts[0].AvailableTris + eVerts[1].AvailableTris;

				if(vConnection[i] > biggestConnection)
				{
					biggestConnection = vConnection[i];
				}
			}

			for(int i = 0; i < vConnection.Length; i++)
			{
				weights[i] += vConnection[i] < biggestConnection ? -1 : 1;
			}

			int index = 0;
			for(int j = 1; j < trisToUse.Length; j++)
			{
				if(weights[j] < weights[index])
				{
					index = j;
				}
			}

			return trisToUse[index];
		}

		/// <summary>
		/// Gets the next triangle in a strip sequence (no swapping)
		/// </summary>
		/// <param name="v1"></param>
		/// <param name="v2"></param>
		/// <returns></returns>
		public Triangle? NextTriangle(Vertex v1, Vertex v2)
		{
			if(!v1.IsConnectedWith(v2, out Edge? e))
			{
				throw new InvalidOperationException("Vertices are not connected");
			}

			return e.Triangles.FirstOrDefault(x => x != this && !x.Used);
		}


		/// <summary>
		/// Whether the culling directions between two triangles differ
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool HasBrokenCullFlow(Triangle other)
		{
			int t = 0;
			foreach(Vertex v in Vertices)
			{
				if(other.Vertices.Contains(v))
				{
					int tt = Array.IndexOf(other.Vertices, v);
					return Vertices[(t + 1) % 3] == other.Vertices[(tt + 1) % 3];
				}

				t++;
			}

			return false;
		}

		public Vertex? GetNextVertexByFlow(Vertex vert)
		{
			// get the index
			for(int i = 0; i < 3; i++)
			{
				if(Vertices[i] == vert)
				{
					return Vertices[(i + 1) % 3];
				}
			}

			return null;
		}

		public override string ToString()
		{
			return $"{Vertices[0].Index}-{Vertices[1].Index}-{Vertices[2].Index}";
		}
	}
}
