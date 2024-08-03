using System.Collections.Generic;

namespace SA3D.Modeling.Strippify
{
	/// <summary>
	/// An edge between two <see cref="Vertex"/>
	/// </summary>
	internal class Edge
	{
		/// <summary>
		/// The two vertices that the edge connects
		/// </summary>
		public Vertex[] Vertices { get; }

		/// <summary>
		/// Triangles that the edge is part of
		/// </summary>
		public List<Triangle> Triangles { get; }

		/// <summary>
		/// Creates a new edge between two vertices
		/// </summary>
		public Edge(Vertex v1, Vertex v2)
		{
			Vertices = new Vertex[] { v1, v2 };
			Triangles = [];
		}

		/// <summary>
		/// Adds a triangle to the edge
		/// </summary>
		public void AddTriangle(Triangle tri)
		{
			foreach(Triangle t in Triangles)
			{
				t.Neighbours.Add(tri);
				tri.Neighbours.Add(t);
			}

			Triangles.Add(tri);
		}
	}
}
