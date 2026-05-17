using Amicitia.IO.Binary;
using SA3D.Common.IO;
using SA3D.Modeling.ObjectData.Enums;
using SA3D.Modeling.ObjectData.Events;
using SA3D.Modeling.Structs;
using System.Numerics;

namespace SA3D.Modeling.ObjectData
{
	/// <summary>
	/// Stage Geometry
	/// </summary>
	public class LevelModel : IBinarySerializable<IOContext>
	{
		private Node _model;

		/// <summary>
		/// Model behind the landentry.
		/// </summary>
		public Node Model
		{
			get => _model;
			set
			{
				_model.OnTransformsUpdated -= OnTransformsUpdated;
				_model.OnMeshDataUpdated -= OnMeshDataUpdated;

				_model = value;

				_model.OnTransformsUpdated += OnTransformsUpdated;
				_model.OnMeshDataUpdated += OnMeshDataUpdated;
			}
		}

		/// <summary>
		/// World space bounds.
		/// <br/> Get automatically updated when the transforms change.
		/// </summary>
		public Bounds ModelBounds { get; set; }

		/// <summary>
		/// Geometry behavior attributes.
		/// </summary>
		public SurfaceAttributes SurfaceAttributes { get; set; }

		/// <summary>
		/// Block mapping bits
		/// </summary>
		public uint BlockBit { get; set; }

		/// <summary>
		/// No idea what this does at all, might be unused
		/// </summary>
		public uint Unknown { get; set; }

		/// <summary>
		/// Creates a new, empty land entry
		/// </summary>
		public LevelModel() : this(new()) { }

		/// <summary>
		/// Creates a new landentry object.
		/// </summary>
		/// <param name="node">Model behind the landentry.</param>
		public LevelModel(Node node)
		{
			_model = node;
			_model.OnTransformsUpdated += OnTransformsUpdated;
			_model.OnMeshDataUpdated += OnMeshDataUpdated;

			UpdateBounds();
		}


		private void OnMeshDataUpdated(Node node, MeshDataUpdatedEventArgs args)
		{
			UpdateBounds();
		}

		private void OnTransformsUpdated(Node node, TransformsUpdatedEventArgs args)
		{
			UpdateBounds();
		}

		/// <summary>
		/// Copies the Meshdata-bounds and applies the landentries transform matrix to them
		/// </summary>
		public void UpdateBounds()
		{
			if(Model.MeshData == null)
			{
				ModelBounds = default;
				return;
			}

			Vector3 position = Vector3.Transform(Model.MeshData.MeshBounds.Position, Model.QuaternionRotation) + Model.Position;
			float radius = Model.MeshData.MeshBounds.Radius * Model.Scale.GreatestValue();
			ModelBounds = new(position, radius);
		}


		/// <inheritdoc/>
		public void Read(BinaryObjectReader reader, IOContext context)
		{
			ModelBounds = reader.ReadObject<Bounds>();

			if(context.LevelFormat is Format.Basic or Format.BasicDX)
			{
				reader.Skip(sizeof(float) * 2); // SA1 has unused radius y and radius z values
			}

			Model = reader.ReadObjectOffset<Node, IOContext>(context, context.PointerLUT)
				?? throw reader.ReadNullReference(nameof(LevelModel), nameof(Model));

			if(context.LevelFormat >= Format.Chunk)
			{
				Unknown = reader.ReadUInt32();
				BlockBit = reader.ReadUInt32();
				SurfaceAttributes = ((SA2SurfaceAttributes)reader.ReadUInt32()).ToUniversal();
			}
			else
			{
				BlockBit = reader.ReadUInt32();
				SurfaceAttributes = ((SA1SurfaceAttributes)reader.ReadUInt32()).ToUniversal();
			}
		}

		/// <inheritdoc/>
		public void Write(BinaryObjectWriter writer, IOContext context)
		{
			writer.WriteObject(ModelBounds);

			if(context.LevelFormat is Format.Basic or Format.BasicDX)
			{
				writer.Skip(sizeof(float) * 2); // SA1 has unused radius y and radius z values
			}

			writer.WriteObjectOffset(Model, context, context.PointerLUT);

			if(context.LevelFormat >= Format.Chunk)
			{
				writer.WriteUInt32(Unknown);
				writer.WriteUInt32(BlockBit);
				writer.WriteUInt32((uint)SurfaceAttributes.ToSA2());
			}
			else
			{
				writer.WriteUInt32(BlockBit);
				writer.WriteUInt32((uint)SurfaceAttributes.ToSA1());
			}
		}



		/// <summary>
		/// Creates a copy of the landentry copies the node tree but reuses attaches.
		/// </summary>
		/// <returns></returns>
		public LevelModel Copy()
		{
			return new(Model.DeepSimpleCopy())
			{
				SurfaceAttributes = SurfaceAttributes,
				BlockBit = BlockBit,
				Unknown = Unknown,
				ModelBounds = ModelBounds
			};
		}

		/// <inheritdoc/>
		public override string ToString()
		{
			return Model.ToString();
		}

	}
}
