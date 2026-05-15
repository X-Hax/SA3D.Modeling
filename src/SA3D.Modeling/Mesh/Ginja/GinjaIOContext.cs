using SA3D.Modeling.Mesh.Ginja.Enums;
using SA3D.Modeling.Structs;

namespace SA3D.Modeling.Mesh.Ginja
{
	/// <summary>
	/// Ginja specific context
	/// </summary>
	public class GinjaIOContext
	{
		/// <summary>
		/// Base context
		/// </summary>
		public IOContext BaseContext { get; }

		/// <summary>
		/// Active index format to be serialized with
		/// </summary>
		public GinjaIndexFormat IndexFormat { get; set; }

		/// <summary>
		/// Creates a new ginja IO context
		/// </summary>
		/// <param name="baseContext"></param>
		public GinjaIOContext(IOContext baseContext)
		{
			BaseContext = baseContext;
			IndexFormat = GinjaIndexFormat.HasPosition;
		}
	}
}
