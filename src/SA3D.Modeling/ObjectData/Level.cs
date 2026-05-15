using Amicitia.IO.Binary;
using SA3D.Common.IO;
using SA3D.Common.Lookup;
using SA3D.Modeling.AnimationData;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.Structs;
using System;
using System.Linq;
using static SA3D.Common.StringExtensions;

namespace SA3D.Modeling.ObjectData
{
	/// <summary>
	/// Stage geometry information
	/// </summary>
	public class Level : ILabel, IBinarySerializable<IOContext>
	{
		#region Properties

		/// <summary>
		/// Label prefix for <see cref="Models"/>
		/// </summary>
		public const string ModelsLabelPrefix = "models_";

		/// <summary>
		/// Label prefix for <see cref="ModelAnimations"/>
		/// </summary>
		public const string ModelAnimationsLabelPrefix = "modelAnimations_";

		/// <inheritdoc/>
		public string LabelPrefix => "level_";

		/// <inheritdoc/>
		public string Label { get; set; }

		/// <summary>
		/// Level geometry
		/// </summary>
		public LabeledArray<LevelModel> Models { get; set; }

		/// <summary>
		/// Geometry animations (sa1)
		/// </summary>
		public LabeledArray<LevelModelAnimation>? ModelAnimations { get; set; }

		/// <summary>
		/// Landtable attributes
		/// </summary>
		public LevelAttributes Attributes { get; set; }

		/// <summary>
		/// Draw distance
		/// </summary>
		public float DrawDistance { get; set; }

		/// <summary>
		/// Texture file name
		/// </summary>
		public string? TextureFileName { get; set; }

		/// <summary>
		/// Texture list pointer
		/// </summary>
		public uint TexListPtr { get; set; }

		/// <summary>
		/// Format of the landtable
		/// </summary>
		public Format Format { get; private set; }

		#endregion

		/// <summary>
		/// Creates a new, empty level
		/// </summary>
		public Level()
		{
			string identifier = GenerateIdentifier();

			Label = LabelPrefix + identifier;
			Models = new(ModelsLabelPrefix + identifier, 0);
		}

		/// <summary>
		/// Sorts land entries to be viable for SA2 / SA2B export.
		/// </summary>
		public void SortLandEntries()
		{
			if(Format is not Format.Chunk and not Format.Ginja)
			{
				return;
			}

			LevelModel[] newOrder = [.. Models.OrderByDescending(x => x.SurfaceAttributes.HasFlag(SurfaceAttributes.Visible))];

			for(int i = 0; i < newOrder.Length; i++)
			{
				Models[i] = newOrder[i];
			}
		}

		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, IOContext context)
		{
			short modelCount = reader.ReadInt16();
			short displayCount = 0;

			if(context.LevelFormat is Format.Chunk or Format.Ginja)
			{
				displayCount = reader.ReadInt16();
				// "direct-display model count" (runtime field)
				reader.Skip(sizeof(short));
			}

			short modelAnimationCount = reader.ReadInt16();

			Attributes = (LevelAttributes)reader.ReadInt16();
			reader.Skip(sizeof(short)); // "is loaded" (runtime field)

			DrawDistance = reader.ReadSingle();

			long modelsOffset = reader.ReadOffsetValue();
			if(modelsOffset == 0)
			{
				throw reader.ReadNullReference(nameof(LevelModel), nameof(Models));
			}

			reader.ReadAtOffset(modelsOffset, () =>
			{
				short baseCount = context.LevelFormat is Format.Chunk or Format.Ginja ? displayCount : modelCount;
				Models = reader.ReadLabeledObjectArray<LevelModel, IOContext>(baseCount, ModelsLabelPrefix, context, context.PointerLUT);

				if(context.LevelFormat is Format.Chunk or Format.Ginja)
				{
					int collisionCount = modelCount - displayCount;

					IOContext collisionModelContext = new()
					{
						LevelFormat = context.LevelFormat,
						MeshFormat = Format.Basic,
						PointerLUT = context.PointerLUT
					};

					LevelModel[] collisionModels = reader.ReadObjectArray<LevelModel, IOContext>(collisionCount, collisionModelContext);
					Models.Array = [.. Models.Array, .. collisionModels];
				}
			});

			ModelAnimations = reader.ReadLabeledObjectArrayOffset<LevelModelAnimation, IOContext>(modelAnimationCount, ModelAnimationsLabelPrefix, context, context.PointerLUT);

			TextureFileName = reader.ReadStringOffset();
			TexListPtr = reader.ReadUInt32();
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, IOContext context)
		{
			if(Format is Format.Chunk or Format.Ginja)
			{
				bool visiblesFinished = false;
				foreach(LevelModel model in Models)
				{
					bool isVisible = model.SurfaceAttributes.HasFlag(SurfaceAttributes.Visible);

					if(!visiblesFinished)
					{
						visiblesFinished = !isVisible;
					}
					else if(isVisible)
					{
						throw new FormatException("Level models are not ordered propertly! Visual models need to come before collision models.");
					}
				}
			}

			writer.WriteInt16((short)Models.Length);

			if(Format is Format.Chunk or Format.Ginja)
			{
				writer.WriteInt16((short)Models.Count(x => x.SurfaceAttributes.HasFlag(SurfaceAttributes.Visible)));
				writer.WriteInt16(0); // direct display models (runtime field)
			}

			writer.WriteInt16((short)(ModelAnimations?.Length ?? 0));
			writer.WriteInt16((short)Attributes);
			writer.WriteInt16(0); // "is loaded" (runtime field)
			writer.WriteSingle(DrawDistance);

			writer.WriteObjectArrayOffset(Models, context, context.PointerLUT);
			writer.WriteObjectArrayOffset(ModelAnimations, context, context.PointerLUT);

			writer.WriteStringOffset(StringBinaryFormat.NullTerminated, TextureFileName);
			writer.WriteUInt32(TexListPtr);

		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return $"{Format} LandTable";
		}
	}
}
