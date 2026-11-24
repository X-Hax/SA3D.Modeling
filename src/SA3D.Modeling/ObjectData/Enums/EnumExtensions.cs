using System;
using System.Linq;
using System.Reflection;

namespace SA3D.Modeling.ObjectData.Enums
{
	/// <summary>
	/// Extension methods for object data enums.
	/// </summary>
	public static class EnumExtensions
	{
		private static readonly SA1SurfaceAttributes[] _sa1DisplayAttributes = [];
		private static readonly SA1SurfaceAttributes[] _sa1CollisionAttributes = [];
		private static readonly SA2SurfaceAttributes[] _sa2DisplayAttributes = [];
		private static readonly SA2SurfaceAttributes[] _sa2CollisionAttributes = [];

		static EnumExtensions()
		{
			Type displayAttributType = typeof(IsDisplayAttributeAttribute);
			Type collisionAttributType = typeof(IsCollisionAttributeAttribute);

			(T[] displayAttributes, T[] collisionAttributes) GetAttributes<T>() where T : struct, Enum
			{
				FieldInfo[] values = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static);

				return (
					[.. values.Where(x => x.IsDefined(displayAttributType, false)).Select(x => (T)x.GetValue(null)!)],
					[.. values.Where(x => x.IsDefined(collisionAttributType, false)).Select(x => (T)x.GetValue(null)!)]
				);
			}

			(_sa1DisplayAttributes, _sa1CollisionAttributes) = GetAttributes<SA1SurfaceAttributes>();
			(_sa2DisplayAttributes, _sa2CollisionAttributes) = GetAttributes<SA2SurfaceAttributes>();
		}

		/// <summary>
		/// Checks if any of the attributes in <paramref name="attribute"/> is annotated with <see cref="IsDisplayAttributeAttribute"/>
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public static bool CheckIsDisplayAttribute(this SA1SurfaceAttributes attribute)
		{
			return _sa1DisplayAttributes.Any(x => attribute.HasFlag(x));
		}

		/// <summary>
		/// Checks if any of the attributes in <paramref name="attribute"/> is annotated with <see cref="IsCollisionAttributeAttribute"/>
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public static bool CheckIsCollisionAttribute(this SA1SurfaceAttributes attribute)
		{
			return _sa1CollisionAttributes.Any(x => attribute.HasFlag(x));
		}

		/// <summary>
		/// Checks if any of the attributes in <paramref name="attribute"/> is annotated with <see cref="IsDisplayAttributeAttribute"/>
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public static bool CheckIsDisplayAttribute(this SA2SurfaceAttributes attribute)
		{
			return _sa2DisplayAttributes.Any(x => attribute.HasFlag(x));
		}

		/// <summary>
		/// Checks if any of the attributes in <paramref name="attribute"/> is annotated with <see cref="IsCollisionAttributeAttribute"/>
		/// </summary>
		/// <param name="attribute"></param>
		/// <returns></returns>
		public static bool CheckIsCollisionAttribute(this SA2SurfaceAttributes attribute)
		{
			return _sa2CollisionAttributes.Any(x => attribute.HasFlag(x));
		}

	}
}
