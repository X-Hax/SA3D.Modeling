using System;

namespace SA3D.Modeling.ObjectData.Enums
{
	/// <summary>
	/// Annotates a Surface attribute as a collision attribute
	/// </summary>
	[AttributeUsage(AttributeTargets.Field)]
	public class IsCollisionAttributeAttribute : Attribute
	{
	}
}
