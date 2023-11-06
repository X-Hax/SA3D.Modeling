using SA3D.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Modeling.Strippify
{
	/// <summary>
	/// Calculates triangle strips from a triangle list.
	/// </summary>
	public class TriangleStrippifier
	{
		/// <summary>
		/// Globally available and used triangle strippifier. 
		/// <br/> Used for every natively used strippifier in this library. 
		/// <br/> Does not raise Topo Error by default.
		/// </summary>
		public static readonly TriangleStrippifier Global = new(false);

		/// <summary>
		/// Whether to raise an exception when a topology anomaly occurs.
		/// </summary>
		public bool RaiseTopoError { get; set; }

		/// <summary>
		/// Creates a new triangle strippifier.
		/// </summary>
		/// <param name="raiseTopoError"></param>
		public TriangleStrippifier(bool raiseTopoError = false)
		{
			RaiseTopoError = raiseTopoError;
		}


		/// <summary>
		/// Strippifies a list of triangle elements and returns a 2D array, where each array is a single strip.
		/// </summary>
		/// <typeparam name="T">Type of triangle elements.</typeparam>
		/// <param name="triangleList">The triangle elements to strippify.</param>
		/// <returns>Triangle strips.</returns>
		/// <exception cref="ArgumentException"></exception>
		public T[][] Strippify<T>(T[] triangleList) where T : IEquatable<T>
		{
			int[][] strips = Strippify(triangleList, out DistinctMap<T> distinctMap);

			T[][] result = new T[strips.Length][];

			for(int i = 0; i < strips.Length; i++)
			{
				int[] strip = strips[i];
				T[] resultStrip = new T[strip.Length];

				for(int j = 0; j < strip.Length; j++)
				{
					resultStrip[j] = distinctMap.Values[strip[j]];
				}

				result[i] = resultStrip;
			}

			return result;
		}

		/// <summary>
		/// Strippifies a list of triangle elements and returns a 2D array, where each array is a single strip.
		/// <br/> Degenerate elements at the beginning of strips are removed and instead marked in the reversedStrips array.
		/// </summary>
		/// <typeparam name="T">Type of triangle elements</typeparam>
		/// <param name="triangleList">The triangle elements to strippify.</param>
		/// <param name="reversedStrips">An array indicating which strips have a reversed forward direction.</param>
		/// <returns>Triangle strips.</returns>
		public T[][] StrippifyNoDegen<T>(T[] triangleList, out bool[] reversedStrips) where T : IEquatable<T>
		{
			int[][] strips = Strippify(triangleList, out DistinctMap<T> distinctMap);

			T[][] result = new T[strips.Length][];
			reversedStrips = new bool[strips.Length];

			for(int i = 0; i < strips.Length; i++)
			{
				int[] strip = strips[i];

				bool reversed = strip[0] == strip[1];
				reversedStrips[i] = reversed;

				int src = reversed ? 1 : 0;

				T[] resultStrip = new T[strip.Length - src];

				for(int dst = 0; src < strip.Length; src++, dst++)
				{
					resultStrip[dst] = distinctMap.Values[strip[src]];
				}

				result[i] = resultStrip;
			}

			return result;
		}

		/// <summary>
		/// Strippifies a list of triangle elements and returns a 2D array of indices to the distinct values, where each array is a single strip.
		/// </summary>
		/// <typeparam name="T">Type of triangle elements</typeparam>
		/// <param name="triangleList">The triangle elements to strippify.</param>
		/// <param name="distinctMap">The index mapping with distinct values.</param>
		/// <returns>Triangle strips consisting of indices to distinct triangle elements.</returns>
		/// <exception cref="ArgumentException"></exception>
		public int[][] Strippify<T>(T[] triangleList, out DistinctMap<T> distinctMap) where T : IEquatable<T>
		{
			if(triangleList.Length % 3 != 0)
			{
				throw new ArgumentException("Triangle list size needs to be a multiple of 3!", nameof(triangleList));
			}

			int[] triangleIndices;

			if(!DistinctMap<T>.TryCreateDistinctMap(triangleList, out distinctMap))
			{
				triangleIndices = new int[triangleList.Length];

				for(int i = 0; i < triangleIndices.Length; i++)
				{
					triangleIndices[i] = i;
				}
			}
			else
			{
				triangleIndices = distinctMap.Map!;
			}

			return Strippify(triangleIndices);
		}

		/// <summary>
		/// Strippifies a list of triangle indices and returns a 2D array, where each array is a single strip.
		/// </summary>
		/// <param name="triangleList">Input triangle list</param>
		/// <returns>Triangle strips.</returns>
		public int[][] Strippify(int[] triangleList)
		{
			/* based on the paper written by David Kronmann:
             https://pdfs.semanticscholar.org/9749/331d92f865282c3f5a19b73b25c4f0ac02bc.pdf
             The code has been written and modified by Justin113D,
             and also optimized the strips by handling the priority list different */

			if(triangleList.Length % 3 != 0)
			{
				throw new ArgumentException("Triangle list size needs to be a multiple of 3!", nameof(triangleList));
			}

			Mesh mesh = new(triangleList, RaiseTopoError);   // reading the index data into a virtual mesh
			int written = 0;            // amount of written triangles
			List<int[]> strips = new(); // the result list

			int triCount = mesh.Triangles.Length;

			// creates a strip from a triangle with no (free) neighbours
			void AddZTriangle(Triangle tri)
			{
				Vertex[] verts = tri.Vertices;
				strips.Add(new int[] { verts[0].Index, verts[1].Index, verts[2].Index });
				written++;
				tri.Used = true;
			}

			Triangle? getFirstTri()
			{
				Triangle? resultTri = null;

				foreach(Triangle t in mesh.Triangles)
				{
					if(t.Used)
					{
						continue;
					}

					int tnCount = t.AvailableNeighbours.Length;
					if(tnCount == 0)
					{
						AddZTriangle(t);
						continue;
					}

					if(resultTri == null)
					{
						resultTri = t;
						continue;
					}

					if(tnCount < resultTri.AvailableNeighbours.Length)
					{
						if(tnCount == 1)
						{
							return t;
						}

						resultTri = t;
					}
				}

				return resultTri;
			}

			Triangle? firstTri = getFirstTri();

			// as long as some triangles remain to be written, keep the loop running
			while(written != triCount)
			{
				// when looking for the first triangle, we also filter out some
				// single triangles, which means that it will alter the written
				// count. thats why we have to call it before the loop starts
				// and before the end of the loop, instead of once at the start

				// the first thing we gotta do is determine the
				// first (max) 3 triangles to write
				Triangle currentTri = firstTri ?? throw new NullReferenceException("First triangle is null!");
				currentTri.Used = true;

				Triangle newTri = currentTri.NextTriangleS() ?? throw new TopologyException("First tri somehow has no usable neighbours");

				// If the two triangles have a broken cull flow, then dont continue
				// the strip (well ok, there is a chance it could continue on
				// another tri, but its not worth looking for such a triangle)
				if(currentTri.HasBrokenCullFlow(newTri))
				{
					AddZTriangle(currentTri);
					// since we are wrapping back around, we have
					// to set the first tri too
					firstTri = getFirstTri();
					continue;
				}

				newTri.Used = true; // confirming that we are using it now

				// get the starting vert
				// (the one which is not connected with the new tri)
				Vertex[] sharedVerts = currentTri.GetSharedEdge(newTri)?.Vertices ?? throw new TopologyException("Triangles are somehow not connected");
				Vertex prevVert = currentTri.GetThirdVertex(sharedVerts[0], sharedVerts[1]) ?? throw new TopologyException("Triangles are somehow not connected");

				// get the vertex which wouldnt be connected to
				// the tri afterwards, to prevent swapping 
				Triangle? secNewTri = newTri.NextTriangleS();
				Vertex currentVert;
				Vertex nextVert;

				// if the third tri isnt valid, just end the strip;
				// now you might be thinking:
				// "but justin, what if the strip can be reversed and continued on the other end?"
				// good point, but! This only occurs if the second triangle has no free neighbours.
				// this can only happen if the first triangle also has no other free neighbours,
				// otherwise the second triangle would have been chosen as the first one.
				// Thus we have two triangles in the same strip
				if(secNewTri == null)
				{
					// gotta get the correct second vertex to keep the vertex flow
					currentVert = currentTri.GetNextVertexByFlow(prevVert) ?? throw new TopologyException("Vertex somehow not part of the vertices");
					nextVert = currentVert == sharedVerts[0] ? sharedVerts[1] : sharedVerts[0];

					int thirdVertex = newTri.GetThirdVertex(currentVert, nextVert)?.Index ?? throw new TopologyException("Triangles are somehow not connected");

					strips.Add(new int[] { prevVert.Index, currentVert.Index, nextVert.Index, thirdVertex });
					written += 2;

					// since we are wrapping back around,
					// we have to set the first tri too
					firstTri = getFirstTri();
					continue;
				}
				else if(secNewTri.HasVertex(sharedVerts[0]))
				{
					currentVert = sharedVerts[1];
					nextVert = sharedVerts[0];
				}
				else
				{
					currentVert = sharedVerts[0];
					nextVert = sharedVerts[1];
				}

				// initializing the strip base
				int[] strip = StripLoop(
					firstTri, ref written,
					newTri, secNewTri,
					prevVert, currentVert, nextVert);

				strips.Add(strip);
				firstTri = getFirstTri();
			}

			return strips.ToArray();
		}


		private int[] StripLoop(Triangle firstTriangle, ref int written, Triangle tri2, Triangle tri3, Vertex vert1, Vertex vert2, Vertex vert3)
		{
			List<int> strip = new() { vert1.Index, vert2.Index, vert3.Index };
			written++;

			// shift verts two forward
			Vertex prevVert = vert3;
			Vertex currentVert = tri2.GetThirdVertex(vert2, vert3) ?? throw new TopologyException("Third vertex somehow null");

			// shift triangles one forward
			Triangle currentTri = tri2;
			Triangle? newTri = currentTri.HasBrokenCullFlow(tri3) ? null : tri3;

			// creating the strip
			bool reachedEnd = false;
			bool reversedList = false;
			while(!reachedEnd)
			{
				// writing the next index
				strip.Add(currentVert.Index);
				written++;

				// ending or reversing the loop when the current
				// tri is None (end of the strip)
				if(newTri == null)
				{
					if(!reversedList && firstTriangle.AvailableNeighbours.Length > 0)
					{
						reversedList = true;

						prevVert = vert2;
						currentVert = vert1;

						newTri = firstTriangle.NextTriangle(prevVert, currentVert);
						if(newTri == null)
						{
							reachedEnd = true;
							continue;
						}

						strip.Reverse();

						(currentTri, firstTriangle) = (firstTriangle, currentTri);
					}
					else
					{
						reachedEnd = true;
						continue;
					}
				}

				// getting the next vertex to write
				Vertex? nextVert = newTri.GetThirdVertex(prevVert, currentVert);

				if(nextVert == null)
				{
					reachedEnd = true;
					continue;
				}

				prevVert = currentVert;
				currentVert = nextVert;

				Triangle oldTri = currentTri;
				currentTri = newTri;
				currentTri.Used = true;

				newTri = oldTri.HasBrokenCullFlow(currentTri)
					? null
					: currentTri.NextTriangle(prevVert, currentVert);
			}

			// checking if the triangle is reversed
			FlipStrip(strip, firstTriangle);
			return strip.ToArray();
		}

		private void FlipStrip(List<int> strip, Triangle firstTriangle)
		{
			for(int i = 0; i < 3; i++)
			{
				if(strip[i] == firstTriangle.Vertices[0].Index)
				{
					if(strip[(i + 1) % 3] != firstTriangle.Vertices[1].Index)
					{
						if(strip.Count % 2 == 1)
						{
							strip.Reverse();
						}
						else
						{
							strip.Insert(0, strip[0]);
						}
					}

					return;
				}
			}
		}


		/// <summary>
		/// Returns an enumerable with the same sequence as <see cref="JoinStrips"/> but without allocating an array.
		/// <br/> For concatenating multiple strips together using degenerate triangles.
		/// </summary>
		/// <typeparam name="T">Strip corner type.</typeparam>
		/// <param name="strips">Strips to concatenate.</param>
		/// <param name="reversed">Indicates which strips are reversed.</param>
		/// <returns>The joined strip.</returns>
		public static IEnumerable<T> JoinedStripEnumerator<T>(T[][] strips, bool[]? reversed)
		{
			bool realRev = false;

			for(int i = 0; i < strips.Length; i++)
			{
				T[] strip = strips[i];

				if(i > 0)
				{
					yield return strip[0];
					realRev = !realRev;
				}

				if(realRev != (reversed?[i] == true))
				{
					yield return strip[0];
					realRev = !realRev;
				}

				foreach(T item in strip)
				{
					yield return item;
					realRev = !realRev;
				}


				if(i < strips.Length - 1)
				{
					yield return strip[^1];
					realRev = !realRev;
				}
			}
		}

		/// <summary>
		/// Concatenates multiple strips together using degenerate triangles.
		/// </summary>
		/// <typeparam name="T">Strip corner type.</typeparam>
		/// <param name="strips">Strips to concatenate.</param>
		/// <param name="reversed">Indicates which strips are reversed.</param>
		/// <returns>The joined strip.</returns>
		public static T[] JoinStrips<T>(T[][] strips, bool[]? reversed)
		{
			return JoinedStripEnumerator(strips, reversed).ToArray();
		}
	}
}
