using System;

namespace SA3D.Modeling.Strippify
{
	/// <summary>
	/// Gets raised when certain 3D topology rules are ignored and/or not processable
	/// </summary>
	public class TopologyException : Exception
	{
		/// <summary>
		/// Creates a new topology exception.
		/// </summary>
		/// <param name="msg">The message to pass along.</param>
		public TopologyException(string msg) : base(msg) { }
	}
}
