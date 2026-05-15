using Amicitia.IO.Binary;
using System.Collections;
using System.Collections.Generic;

namespace SA3D.Modeling.Mesh.Basic.Polygon
{
	/// <summary>
	/// A BASIC polygon containing a variable number of indices.
	/// </summary>
	public struct BasicMultiPolygon : IBasicPolygon
	{
		/// <summary>
		/// Polygon corner information
		/// </summary>
		public ushort[] Indices { get; set; }

		/// <summary>
		/// Whether the backface culling direction is flipped.
		/// </summary>
		public bool Reversed { get; set; }

		/// <inheritdoc/>
		public readonly uint Size => (uint)((1 + Indices.Length) * sizeof(ushort));

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
		/// <param name="corners">Indices of the polygon.</param>
		/// <param name="reversed">Whether the polygons backface culling direction is flipped.</param>
		public BasicMultiPolygon(ushort[] corners, bool reversed)
		{
			Indices = corners;
			Reversed = reversed;
		}

		/// <summary>
		/// Creates a new empty multi polygon.
		/// </summary>
		/// <param name="size">Number of indices the polygon holds.</param>
		/// <param name="reversed">Whether the polygons backface culling direction is flipped.</param>
		public BasicMultiPolygon(uint size, bool reversed)
			: this(new ushort[size], reversed) { }


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader)
		{
			ushort header = reader.ReadUInt16();
			Indices = new ushort[header & 0x7FFF];
			Reversed = (header & 0x8000) != 0;
			for(int i = 0; i < Indices.Length; i++)
			{
				Indices[i] = reader.ReadUInt16();
			}
		}

		/// <inheritdoc/>
		public readonly void Write(BinaryObjectWriter writer)
		{
			writer.WriteUInt16((ushort)((Indices.Length & 0x7FFF) | (Reversed ? 0x8000 : 0)));
			for(int i = 0; i < Indices.Length; i++)
			{
				writer.WriteUInt16(Indices[i]);
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
			return new BasicMultiPolygon((ushort[])Indices.Clone(), Reversed);
		}

		/// <inheritdoc/>
		public override readonly string ToString()
		{
			return $"Multi: {Reversed} - {Indices.Length}";
		}

	}
}
