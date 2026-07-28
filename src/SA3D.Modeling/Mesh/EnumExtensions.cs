using System;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.Mesh
{
	/// <summary>
	/// Enum extensions
	/// </summary>
	public static class EnumExtensions
	{
		/// <summary>
		/// Converts from an attach format to a file format
		/// </summary>
		/// <param name="attachFormat"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static Format ToFormat(this MeshFormat attachFormat)
		{
			return attachFormat switch
			{
				MeshFormat.Basic => Format.Basic,
				MeshFormat.Chunk => Format.Chunk,
				MeshFormat.Ginja => Format.Ginja,
				_ => throw new InvalidOperationException(),
			};
		}
	}
}
