using SA3D.Common.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace SA3D.Modeling.Mesh.Basic.Polygon
{
	/// <summary>
	/// A BASIC polygon containing a variable number of corners.
	/// </summary>
	public struct BasicMultiPolygon : IBasicPolygon
	{
		/// <summary>
		/// Indices of the polygon.
		/// </summary>
		public ushort[] Indices { get; set; }

		/// <summary>
		/// Whether the backface culling direction is flipped.
		/// </summary>
		public bool Reversed { get; set; }

		/// <inheritdoc/>
		public readonly uint Size => (uint)(2 + (Indices.Length * 2));

		/// <inheritdoc/>
		public readonly int NumIndices => Indices.Length;


		/// <inheritdoc/>
		public readonly ushort this[int index]
		{
			get => Indices[index];
			set => Indices[index] = value;
		}

		/// <summary>
		/// Creates a new multi polygon.
		/// </summary>
		/// <param name="indices">Indices of the polygon.</param>
		/// <param name="reversed">Whether the polygons backface culling direction is flipped.</param>
		public BasicMultiPolygon(ushort[] indices, bool reversed)
		{
			Indices = indices;
			Reversed = reversed;
		}

		/// <summary>
		/// Creates a new empty multi polygon.
		/// </summary>
		/// <param name="size">Number of indices the polygon holds.</param>
		/// <param name="reversed">Whether the polygons backface culling direction is flipped.</param>
		public BasicMultiPolygon(uint size, bool reversed)
			: this(new ushort[size], reversed) { }


		/// <summary>
		/// Reads a basic multi polygon off of an endian stack reader. Advances the address by the number of bytes read.
		/// </summary>
		/// <param name="data">The reader to read from.</param>
		/// <param name="address">Address at which the polygon is located.</param>
		/// <returns>The polygon that was read.</returns>
		public static BasicMultiPolygon Read(EndianStackReader data, ref uint address)
		{
			ushort header = data.ReadUShort(address);
			ushort[] indices = new ushort[header & 0x7FFF];
			bool reversed = (header & 0x8000) != 0;
			address += 2;
			for(int i = 0; i < indices.Length; i++)
			{
				indices[i] = data.ReadUShort(address);
				address += 2;
			}

			return new BasicMultiPolygon(indices, reversed);
		}

		/// <inheritdoc/>
		public readonly void Write(EndianStackWriter writer)
		{
			writer.WriteUShort((ushort)((Indices.Length & 0x7FFF) | (Reversed ? 0x8000 : 0)));
			for(int i = 0; i < Indices.Length; i++)
			{
				writer.WriteUShort(Indices[i]);
			}
		}


		/// <inheritdoc/>
		public readonly IEnumerator<ushort> GetEnumerator()
		{
			return ((IEnumerable<ushort>)Indices).GetEnumerator();
		}

		/// <inheritdoc/>
		readonly IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}


		/// <inheritdoc/>
		public readonly object Clone()
		{
			return new BasicMultiPolygon(Indices.ToArray(), Reversed);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Multi: {Reversed} - {Indices.Length}";
		}


	}
}
