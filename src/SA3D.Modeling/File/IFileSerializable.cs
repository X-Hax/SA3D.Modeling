using Amicitia.IO.Binary;

namespace SA3D.Modeling.File
{
	/// <summary>
	/// Base file interface
	/// </summary>
	public interface IFileSerializable : IBinarySerializable
	{
		/// <summary>
		/// Check whether the data behind a reader can be read as the file
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		public bool Check(BinaryObjectReader reader);
	}

	/// <summary>
	/// Base file interface (with a context)
	/// </summary>
	public interface IFileSerializable<T> : IBinarySerializable<T>, IFileSerializable { }
}
