using System;
using System.Numerics;

namespace SA3D.Modeling.Structs
{
	internal struct PositionNormal : IEquatable<PositionNormal>
	{
		public Vector3 position;

		public Vector3 normal;

		public PositionNormal(Vector3 position, Vector3 normal)
		{
			this.position = position;
			this.normal = normal;
		}

		public override readonly bool Equals(object? obj)
		{
			return obj is PositionNormal normal &&
				   position.Equals(normal.position) &&
				   this.normal.Equals(normal.normal);
		}

		public readonly bool Equals(PositionNormal other)
		{
			return Equals((object?)other);
		}

		public override readonly int GetHashCode()
		{
			return HashCode.Combine(position, normal);
		}
	}
}
