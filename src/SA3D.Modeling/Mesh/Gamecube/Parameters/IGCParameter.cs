using SA3D.Common.IO;
using SA3D.Modeling.Mesh.Gamecube.Enums;
using System;

namespace SA3D.Modeling.Mesh.Gamecube.Parameters
{
	/// <summary>
	/// Base interface for all GC parameter types. 
	/// <br/> Used to store geometry information (like materials).
	/// </summary>
	public interface IGCParameter
	{
		/// <summary>
		/// The type of parameter.
		/// </summary>
		public GCParameterType Type { get; }

		/// <summary>
		/// All parameter data is stored in these 4 bytes.
		/// </summary>
		public uint Data { get; set; }

		/// <summary>
		/// Reads a parameter from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which the parameter is located</param>
		/// <returns>The parameter that was read.</returns>
		public static IGCParameter Read(EndianStackReader reader, uint address)
		{
			GCParameterType paramType = (GCParameterType)reader[address];

			IGCParameter result = paramType switch
			{
				GCParameterType.VertexFormat => new GCVertexFormatParameter(),
				GCParameterType.IndexFormat => new GCIndexFormatParameter(),
				GCParameterType.Lighting => new GCLightingParameter(),
				GCParameterType.BlendAlpha => new GCBlendAlphaParameter(),
				GCParameterType.AmbientColor => new GCAmbientColorParameter(),
				GCParameterType.DiffuseColor => new GCDiffuseColorParameter(),
				GCParameterType.SpecularColor => new GCSpecularColorParameter(),
				GCParameterType.Texture => new GCTextureParameter(),
				GCParameterType.Unknown => new GCUnknownParameter(),
				GCParameterType.Texcoord => new GCTexCoordParameter(),
				_ => throw new NotSupportedException($"GC parameter type {paramType} not supported.")
			};

			result.Data = reader.ReadUInt(address + 4);

			return result;
		}
	}
}
