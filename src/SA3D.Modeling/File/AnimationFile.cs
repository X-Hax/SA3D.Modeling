using SA3D.Common.IO;
using SA3D.Modeling.Animation;
using SA3D.Modeling.Structs;
using System;
using System.IO;
using static SA3D.Modeling.File.FileHeaders;
using SA3D.Common;

namespace SA3D.Modeling.File
{
	/// <summary>
	/// Animation file contents.
	/// </summary>
	public class AnimationFile
	{
		/// <summary>
		/// Animation of the file.
		/// </summary>
		public Motion Animation { get; }

		/// <summary>
		/// Metadata in the file.
		/// </summary>
		public MetaData MetaData { get; }


		/// <summary>
		/// Creates a new animation file
		/// </summary>
		/// <param name="animation">Animation of the file.</param>
		/// <param name="metaData">Metadata in the file.</param>
		public AnimationFile(Motion animation, MetaData metaData)
		{
			Animation = animation;
			MetaData = metaData;
		}


		/// <summary>
		/// Checks whether data is formatted as a animation file.
		/// </summary>
		/// <param name="data">The data to check.</param>
		public static bool CheckIsAnimationFile(byte[] data)
		{
			return CheckIsAnimationFile(data, 0);
		}

		/// <summary>
		/// Checks whether data is formatted as a animation file.
		/// </summary>
		/// <param name="data">The data to check.</param>
		/// <param name="address">Address at which to check.</param>
		public static bool CheckIsAnimationFile(byte[] data, uint address)
		{
			return CheckIsAnimationFile(new EndianStackReader(data), address);
		}

		/// <summary>
		/// Checks whether data is formatted as a animation file.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		public static bool CheckIsAnimationFile(EndianStackReader reader)
		{
			return CheckIsAnimationFile(reader, 0);
		}

		/// <summary>
		/// Checks whether data is formatted as a animation file.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to check.</param>
		public static bool CheckIsAnimationFile(EndianStackReader reader, uint address)
		{
			if(CheckIsSAAnimFile(reader, address))
			{
				return true;
			}

			return CheckIsNJAnimFile(reader, address);
		}

		private static bool CheckIsSAAnimFile(EndianStackReader reader, uint address)
		{
			reader.PushBigEndian(false);
			bool result = (reader.ReadULong(address) & HeaderMask) == SAANIM;
			reader.PopEndian();
			return result;
		}

		private static bool CheckIsNJAnimFile(EndianStackReader reader, uint address)
		{
			return NJBlockUtility.FindBlockAddress(reader, address, AnimationBlockHeaders, out _);
		}




		/// <summary>
		/// Reads a animation file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		/// <returns>The animation file that was read.</returns>
		public static AnimationFile ReadFromFile(string filepath)
		{
			return ReadFromBytes(System.IO.File.ReadAllBytes(filepath), 0);
		}

		/// <summary>
		/// Reads a animation file.
		/// </summary>
		/// <param name="filepath">Path to the file to read.</param>
		/// <param name="nodeCount">Number of nodes in the targeted model node tree. <br/> Only acts as fallback, in case the file does not contain the value.</param>
		/// <param name="shortRot">Whether euler rotations are stored in 16-bit instead of 32-bit. <br/> Only acts as fallback, in case the file does not contain the value.</param>
		/// <returns>The animation file that was read.</returns>
		public static AnimationFile ReadFromFile(string filepath, uint? nodeCount, bool shortRot)
		{
			return ReadFromBytes(System.IO.File.ReadAllBytes(filepath), 0, nodeCount, shortRot);
		}

		/// <summary>
		/// Reads a animation file off byte data.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <returns>The animation file that was read.</returns>
		public static AnimationFile ReadFromBytes(byte[] data)
		{
			return ReadFromBytes(data, 0, null, false);
		}

		/// <summary>
		/// Reads a animation file off byte data.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <returns>The animation file that was read.</returns>
		public static AnimationFile ReadFromBytes(byte[] data, uint address)
		{
			return ReadFromBytes(data, address, null, false);
		}

		/// <summary>
		/// Reads a animation file off byte data.
		/// </summary>
		/// <param name="data">The data to read.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="nodeCount">Number of nodes in the targeted model node tree. <br/> Only acts as fallback, in case the file does not contain the value.</param>
		/// <param name="shortRot">Whether euler rotations are stored in 16-bit instead of 32-bit. <br/> Only acts as fallback, in case the file does not contain the value.</param>
		/// <returns>The animation file that was read.</returns>
		public static AnimationFile ReadFromBytes(byte[] data, uint address, uint? nodeCount, bool shortRot)
		{
			using(EndianStackReader reader = new(data))
			{
				return Read(reader, address, nodeCount, shortRot);
			}
		}

		/// <summary>
		/// Reads a animation file off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <returns>The animation file that was read.</returns>
		public static AnimationFile Read(EndianStackReader reader)
		{
			return Read(reader, 0, null, false);
		}

		/// <summary>
		/// Reads a animation file off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <returns>The animation file that was read.</returns>
		public static AnimationFile Read(EndianStackReader reader, uint address)
		{
			return Read(reader, address, null, false);
		}

		/// <summary>
		/// Reads a animation file off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from.</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="nodeCount">Number of nodes in the targeted model node tree. <br/> Only acts as fallback, in case the file does not contain the value.</param>
		/// <param name="shortRot">Whether euler rotations are stored in 16-bit instead of 32-bit. <br/> Only acts as fallback, in case the file does not contain the value.</param>
		/// <returns>The animation file that was read.</returns>
		public static AnimationFile Read(EndianStackReader reader, uint address, uint? nodeCount, bool shortRot)
		{
			if(CheckIsSAAnimFile(reader, address))
			{
				try
				{
					reader.PushBigEndian(false);
					return ReadSA(reader, address, nodeCount, shortRot);
				}
				finally
				{
					reader.PopEndian();
				}
			}

			uint prevImageBase = reader.ImageBase;
			if(CheckIsNJAnimFile(reader, address))
			{
				try
				{
					reader.PushBigEndian(reader.CheckBigEndian32(address + 4));
					return ReadNJ(reader, address, nodeCount, shortRot);
				}
				finally
				{
					reader.ImageBase = prevImageBase;
					reader.PopEndian();
				}
			}

			throw new FormatException("File is not an animation file");
		}

		private static AnimationFile ReadNJ(EndianStackReader reader, uint address, uint? nodeCount, bool shortrot)
		{
			if(nodeCount == null)
			{
				throw new ArgumentException("Cannot read NJ animations without providing node count!");
			}

			NJBlockUtility.FindBlockAddress(reader, address, AnimationBlockHeaders, out uint? blockAddress);

			uint dataAddress = blockAddress!.Value + 8;
			reader.ImageBase = unchecked((uint)-dataAddress);
			Motion motion = Motion.Read(reader, dataAddress, nodeCount.Value, new(), shortrot);
			return new(motion, new());
		}

		private static AnimationFile ReadSA(EndianStackReader reader, uint address, uint? nodeCount, bool shortRot)
		{
			byte version = reader[7];
			if(version > CurrentAnimVersion)
			{
				throw new FormatException("Not a valid SAANIM file.");
			}

			uint motionAddress = reader.ReadUInt(address + 8);

			MetaData metaData = new();
			if(version >= 2)
			{
				// motion v2 uses metadata v3
				metaData = MetaData.Read(reader, address + 0xC, 3, false);
			}
			else if(reader.TryReadPointer(address + 0xC, out uint labelAddr))
			{
				metaData.Labels.Add(motionAddress, reader.ReadNullterminatedString(labelAddr));
			}

			if(version > 0)
			{
				const uint shortRotMask = (uint)Flag32.B31;
				uint fileNodeCount = reader.ReadUInt(0x10);
				shortRot = (fileNodeCount & shortRotMask) != 0;
				nodeCount = fileNodeCount & ~shortRotMask;
			}
			else if(nodeCount == null)
			{
				throw new ArgumentException("Cannot open version 0 animations without providing node count!");
			}

			PointerLUT lut = new(metaData.Labels);
			Motion motion = Motion.Read(reader, motionAddress, nodeCount.Value, lut, shortRot);

			return new(motion, metaData);
		}


		/// <summary>
		/// Write the animation file to a file. Previous labels may get lost.
		/// </summary>
		/// <param name="filepath">Path to the file to write to.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public void WriteToFile(string filepath)
		{
			WriteToFile(filepath, Animation, MetaData);
		}

		/// <summary>
		/// Writes the animation file to a byte array. Previous labels may get lost.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public byte[] WriteToBytes()
		{
			return WriteToBytes(Animation, MetaData);
		}

		/// <summary>
		/// Writes the animation file to an endian stack writer. Previous labels may get lost.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public void Write(EndianStackWriter writer)
		{
			Write(writer, Animation, MetaData);
		}


		/// <summary>
		/// Write a animation file to a file.
		/// </summary>
		/// <param name="filepath">Path to the file to write to.</param>
		/// <param name="animation">The animation to write.</param>
		/// <param name="metaData">The metadata to include.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public static void WriteToFile(string filepath, Motion animation, MetaData? metaData = null)
		{
			using(FileStream stream = System.IO.File.Create(filepath))
			{
				EndianStackWriter writer = new(stream);
				Write(writer, animation, metaData);
			}
		}

		/// <summary>
		/// Writes a animation file to a byte array.
		/// </summary>
		/// <param name="animation">The animation to write.</param>
		/// <param name="metaData">The metadata to include.</param>
		/// <returns>The written byte data.</returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static byte[] WriteToBytes(Motion animation, MetaData? metaData = null)
		{
			using(MemoryStream stream = new())
			{
				EndianStackWriter writer = new(stream);
				Write(writer, animation, metaData);
				return stream.ToArray();
			}
		}

		/// <summary>
		/// Writes a animation file to an endian stack writer.
		/// </summary>
		/// <param name="writer">The writer to write to.</param>
		/// <param name="animation">The animation to write.</param>
		/// <param name="metaData">The metadata to include.</param>
		/// <exception cref="InvalidOperationException"></exception>
		public static void Write(EndianStackWriter writer, Motion animation, MetaData? metaData = null)
		{
			writer.WriteULong(SAANIMVer);

			uint placeholderAddr = writer.Position;
			// 4 bytes: motion address placeholder
			// 4 bytes: metadata placeholder
			writer.WriteEmpty(8);

			uint animFileInfo = animation.NodeCount;
			if(animation.ShortRot)
			{
				animFileInfo |= (uint)Flag32.B31;
			}

			writer.WriteUInt(animFileInfo);

			PointerLUT lut = new();

			uint motionAddress = animation.Write(writer, lut);

			metaData ??= new();
			metaData.Labels = lut.Labels.GetDictFrom();
			uint metaDataAddress = metaData.Write(writer);

			uint end = writer.Position;
			writer.Seek(placeholderAddr, SeekOrigin.Begin);
			writer.WriteUInt(motionAddress);
			writer.WriteUInt(metaDataAddress);
			writer.Seek(end, SeekOrigin.Begin);
		}
	}
}
