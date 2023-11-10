using SA3D.Common.IO;

namespace SA3D.Modeling.Mesh.Chunk.PolyChunks
{
	/// <summary>
	/// Polychunk with unknown usage.
	/// </summary>
	public class MaterialBumpChunk : SizedChunk
	{
		/// <inheritdoc/>
		public override ushort Size => 16;

		/// <summary>
		/// DX.
		/// </summary>
		public ushort DX { get; set; }

		/// <summary>
		/// DY.
		/// </summary>
		public ushort DY { get; set; }

		/// <summary>
		/// DZ.
		/// </summary>
		public ushort DZ { get; set; }

		/// <summary>
		/// UX.
		/// </summary>
		public ushort UX { get; set; }

		/// <summary>
		/// UY.
		/// </summary>
		public ushort UY { get; set; }

		/// <summary>
		/// UZ.
		/// </summary>
		public ushort UZ { get; set; }

		/// <summary>
		/// Creates a new material bump chunk.
		/// </summary>
		public MaterialBumpChunk() : base(PolyChunkType.Material_Bump) { }

		internal static MaterialBumpChunk Read(EndianStackReader reader, uint address)
		{
			ushort header = reader.ReadUShort(address);
			byte attrib = (byte)(header >> 8);
			// skipping size
			address += 4;

			return new MaterialBumpChunk()
			{
				Attributes = attrib,
				DX = reader.ReadUShort(address),
				DY = reader.ReadUShort(address += 2),
				DZ = reader.ReadUShort(address += 2),
				UX = reader.ReadUShort(address += 2),
				UY = reader.ReadUShort(address += 2),
				UZ = reader.ReadUShort(address += 2),
			};
		}

		/// <inheritdoc/>
		protected override void InternalWrite(EndianStackWriter writer)
		{
			base.InternalWrite(writer);
			writer.WriteUShort(DX);
			writer.WriteUShort(DY);
			writer.WriteUShort(DZ);
			writer.WriteUShort(UX);
			writer.WriteUShort(UY);
			writer.WriteUShort(UZ);
		}
	}
}
