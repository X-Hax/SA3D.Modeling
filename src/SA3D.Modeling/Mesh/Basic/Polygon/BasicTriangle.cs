using Amicitia.IO.Binary;
using System;
using System.Collections;
using System.Collections.Generic;

namespace SA3D.Modeling.Mesh.Basic.Polygon
{
	/// <summary>
	/// A polygon with three index.
	/// </summary>
	public struct BasicTriangle : IBasicPolygon
	{
		/// <inheritdoc/>
		public readonly uint Size => 6;

		/// <inheritdoc/>
		public readonly int NumIndices => 3;


		/// <summary>
		/// First vertex index.
		/// </summary>
		public ushort Index1 { get; set; }

		/// <summary>
		/// Second vertex index.
		/// </summary>
		public ushort Index2 { get; set; }

		/// <summary>
		/// Third vertex index.
		/// </summary>
		public ushort Index3 { get; set; }


		/// <inheritdoc/>
		public ushort this[int index]
		{
			readonly get => index switch
			{
				0 => Index1,
				1 => Index2,
				2 => Index3,
				_ => throw new IndexOutOfRangeException(),
			};
			set
			{
				switch(index)
				{
					case 0:
						Index1 = value;
						break;
					case 1:
						Index2 = value;
						break;
					case 2:
						Index3 = value;
						break;
					default:
						throw new IndexOutOfRangeException();
				}
			}
		}

		/// <summary>
		/// Creates a new populated basic quad.
		/// </summary>
		/// <param name="index1">First vertex index.</param>
		/// <param name="index2">Second vertex index.</param>
		/// <param name="index3">Third vertex index.</param>
		public BasicTriangle(ushort index1, ushort index2, ushort index3)
		{
			Index1 = index1;
			Index2 = index2;
			Index3 = index3;
		}


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			Index1 = reader.ReadUInt16();
			Index2 = reader.ReadUInt16();
			Index3 = reader.ReadUInt16();
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer)
		{
			writer.WriteUInt16(Index1);
			writer.WriteUInt16(Index2);
			writer.WriteUInt16(Index3);
		}


		/// <inheritdoc/>
		public readonly IEnumerator<ushort> GetEnumerator()
		{
			yield return Index1;
			yield return Index2;
			yield return Index3;
		}

		readonly IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		/// <inheritdoc/>
		public readonly object Clone()
		{
			return this;
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Triangle: [{Index1}, {Index2}, {Index3}]";
		}
	}
}
