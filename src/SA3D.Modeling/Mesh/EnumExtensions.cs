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
		public static Format ToFormat(this AttachFormat attachFormat)
		{
			return attachFormat switch
			{
				AttachFormat.Basic => Format.Basic,
				AttachFormat.Chunk => Format.Chunk,
				AttachFormat.Ginja => Format.Ginja,
				_ => throw new InvalidOperationException(),
			};
		}
	}
}
