using Amicitia.IO.Binary;
using SA3D.Modeling.Mesh.Ginja.Enums;
using System;

namespace SA3D.Modeling.Mesh.Ginja.Parameters
{
	/// <summary>
	/// Base interface for all GC parameter types. 
	/// <br/> Used to store geometry information (like materials).
	/// </summary>
	public interface IGinjaParameter : IBinarySerializable
	{
		/// <summary>
		/// The type of parameter.
		/// </summary>
		public GinjaParameterType Type { get; }

		/// <summary>
		/// All parameter data is stored in these 4 bytes.
		/// </summary>
		public uint Data { get; set; }

		/// <inheritdoc/>
		void IBinarySerializable.Read(BinaryObjectReader reader)
		{
			reader.Skip(4);
			Data = reader.ReadUInt32();
		}

		/// <inheritdoc/>
		void IBinarySerializable.Write(BinaryObjectWriter writer)
		{
			writer.WriteByte((byte)Type);
			writer.WriteBytes([0, 0, 0]);
			writer.WriteUInt32(Data);
		}

		/// <summary>
		/// Reads a parameter from an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <returns>The parameter that was read.</returns>
		public static IGinjaParameter ReadParameter(BinaryObjectReader reader)
		{
			GinjaParameterType paramType = (GinjaParameterType)reader.ReadByte();
			reader.Skip(3);

			IGinjaParameter result = paramType switch
			{
				GinjaParameterType.VertexFormat => new GinjaVertexFormatParameter(),
				GinjaParameterType.IndexFormat => new GinjaIndexFormatParameter(),
				GinjaParameterType.StripFlags => new GinjaStripFlagsParameter(),
				GinjaParameterType.BlendAlpha => new GinjaBlendAlphaParameter(),
				GinjaParameterType.DiffuseColor => new GinjaDiffuseColorParameter(),
				GinjaParameterType.AmbientColor => new GinjaAmbientColorParameter(),
				GinjaParameterType.SpecularColor => new GinjaSpecularColorParameter(),
				GinjaParameterType.Texture => new GinjaTextureParameter(),
				GinjaParameterType.TevStage => new GinjaTevStageParameter(),
				GinjaParameterType.TexGen => new GinjaTexGenParameter(),
				_ => throw new NotSupportedException($"GC parameter type {paramType} not supported.")
			};

			result.Data = reader.ReadUInt32();

			return result;
		}
	}
}
