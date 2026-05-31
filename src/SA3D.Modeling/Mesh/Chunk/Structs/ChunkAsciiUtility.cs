using SA3D.Common.Ascii;

namespace SA3D.Modeling.Mesh.Chunk.Structs
{
	internal static class ChunkAsciiUtility
	{
		public static void WritePolygonUserflags(this AsciiWriter writer, int count, ushort first, ushort second, ushort third, bool asColor)
		{
			switch(count)
			{
				case 1:
					writer.Write($"\tUF1( 0x{first:x4} ),");
					break;
				case 2:
					if(asColor)
					{
						byte alpha = (byte)(second >> 8);
						byte red = (byte)(second & 0xFF);
						byte green = (byte)(first >> 8);
						byte blue = (byte)(first & 0xFF);

						writer.Write($"\tMDiff( {alpha}, {red}, {green}, {blue} ),");
					}
					else
					{
						writer.Write($"\tUF2( 0x{first:x4}, 0x{second:x4} ),");
					}

					break;
				case 3:
					writer.Write($"\tUF3( 0x{first:x4}, 0x{second:x4}, 0x{third:x4} ),");
					break;
			}
		}
	}
}
