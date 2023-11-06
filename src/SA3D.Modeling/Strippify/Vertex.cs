using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace SA3D.Modeling.Strippify
{
	/// <summary>
	/// A single point in 3D space, used to create polygons
	/// </summary>
	internal class Vertex
	{
		/// <summary>
		/// The index of this vertex
		/// </summary>
		public int Index { get; }

		/// <summary>
		/// The edges connected to this vertex
		/// </summary>
		public Dictionary<Vertex, Edge> Edges { get; }

		/// <summary>
		/// The triangles of this vertex
		/// </summary>
		public List<Triangle> Triangles { get; }

		/// <summary>
		/// Returns the amount of triangles connected to this vertex that havent already been used
		/// </summary>
		public int AvailableTris => Triangles.Count(x => !x.Used);

		/// <summary>
		/// Creates a new Vertex by index
		/// </summary>
		/// <param name="index"></param>
		public Vertex(int index)
		{
			Index = index;
			Edges = new Dictionary<Vertex, Edge>();
			Triangles = new List<Triangle>();
		}

		/// <summary>
		/// Returns the edge between two vertices. If the edge doesnt exist, null is returned
		/// </summary>
		/// <param name="other">Vertex to check connection with</param>
		/// <param name="edge"></param>
		/// <returns></returns>
		public bool IsConnectedWith(Vertex other, [MaybeNullWhen(false)] out Edge edge)
		{
			return Edges.TryGetValue(other, out edge);
		}

		/// <summary>
		/// Connects a vertex with another and returns the connected edge
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public Edge Connect(Vertex other)
		{
			Edge e = new(this, other);
			Edges.Add(other, e);
			other.Edges.Add(this, e);
			return e;
		}

		public override string ToString()
		{
			return $"{Index} - {Triangles.Count}/{AvailableTris}";
		}
	}
}
