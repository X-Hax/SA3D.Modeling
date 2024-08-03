using System.Collections.Generic;
using System.Linq;

namespace SA3D.Modeling.Strippify
{
	/// <summary>
	/// A mesh made of connections between triangles, egdes and vertices
	/// </summary>
	internal class Mesh
	{
		/// <summary>
		/// All triangles in the mesh
		/// </summary>
		public Triangle[] Triangles { get; }

		/// <summary>
		/// all vertices in the mesh
		/// </summary>
		public Vertex[] Vertices { get; }

		/// <summary>
		/// Creates a new mesh from a triangle list
		/// </summary>
		/// <param name="triangleList"></param>
		/// <param name="raiseTopoError"></param>
		public Mesh(int[] triangleList, bool raiseTopoError)
		{
			int vertCount = triangleList.Max(x => x) + 1;

			Vertices = new Vertex[vertCount];

			for(int i = 0; i < vertCount; i++)
			{
				Vertices[i] = new Vertex(i);
			}

			List<Edge> edges = [];
			List<Triangle> triangles = [];

			for(int i = 0; i < triangleList.Length; i += 3)
			{
				if(triangleList[i] == triangleList[i + 1]
					|| triangleList[i + 1] == triangleList[i + 2]
					|| triangleList[i + 2] == triangleList[i])
				{
					continue;
				}

				triangles.Add(new(
					new Vertex[] {
							Vertices[triangleList[i]],
							Vertices[triangleList[i+1]],
							Vertices[triangleList[i+2]]
					},
					edges, raiseTopoError));
			}

			int triEdgeCount = edges.Count(x => x.Triangles.Count > 2);
			//if (triEdgeCount > 0)
			//{
			//    Console.WriteLine("Tripple edges: " + triEdgeCount);
			//}

			Triangles = triangles.ToArray();
		}
	}
}
