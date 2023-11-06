using SA3D.Common.IO;
namespace SA3D.Modeling.Mesh.Gamecube.Parameters
{
	/// <summary>
	/// Extension methods for <see cref="IGCParameter"/>
	/// </summary>
	public static class GCParameterExtensions
	{
		/// <summary>
		/// Writes a GC parameter to an endian stack writer.
		/// </summary>
		/// <param name="parameter">The parameter to write</param>
		/// <param name="writer">The writer to write to</param>
		public static void Write(this IGCParameter parameter, EndianStackWriter writer)
		{
			writer.WriteByte((byte)parameter.Type);
			writer.WriteEmpty(3);
			writer.WriteUInt(parameter.Data);
		}

	}
}
