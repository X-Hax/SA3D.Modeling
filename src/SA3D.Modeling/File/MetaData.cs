using SA3D.Common.IO;
using SA3D.Modeling.File.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;


namespace SA3D.Modeling.File
{
	/// <summary>
	/// Meta data storage for files.
	/// </summary>
	public class MetaData
	{
		/// <summary>
		/// Author of the file.
		/// </summary>
		public string? Author { get; set; }

		/// <summary>
		/// Description of the files contents.
		/// </summary>
		public string? Description { get; set; }

		/// <summary>
		/// Action name.
		/// </summary>
		public string? ActionName { get; set; }

		/// <summary>
		/// Object name.
		/// </summary>
		public string? ObjectName { get; set; }

		/// <summary>
		/// C struct labels (only for reading).
		/// </summary>
		public Dictionary<uint, string> Labels { get; set; }

		/// <summary>
		/// Animation file paths.
		/// </summary>
		public List<string> AnimFiles { get; }

		/// <summary>
		/// Morph animation file paths.
		/// </summary>
		public List<string> MorphFiles { get; }

		/// <summary>
		/// Metadata weights for advanced BASIC welding.
		/// </summary>
		public List<MetaWeightNode> MetaWeights { get; }

		/// <summary>
		/// Other chunk blocks that have no implementation.
		/// </summary>
		public Dictionary<uint, byte[]> Other { get; set; }


		/// <summary>
		/// Creates a new empty set of meta data.
		/// </summary>
		public MetaData()
		{
			AnimFiles = [];
			MorphFiles = [];
			Other = [];
			Labels = [];
			MetaWeights = [];
		}


		private void ReadVersion0(EndianStackReader reader, uint address)
		{
			// reading animation locations
			if(reader.TryReadPointer(address, out uint dataAddr))
			{
				uint pathAddr = reader.ReadPointer(dataAddr);
				while(pathAddr != uint.MaxValue)
				{
					AnimFiles.Add(reader.ReadNullterminatedString(pathAddr));
					pathAddr = reader.ReadPointer(dataAddr += 4);
				}
			}

			// reading morph locations
			if(reader.TryReadPointer(address + 4, out dataAddr))
			{
				uint pathAddr = reader.ReadPointer(dataAddr);
				while(pathAddr != uint.MaxValue)
				{
					MorphFiles.Add(reader.ReadNullterminatedString(pathAddr));
					pathAddr = reader.ReadPointer(dataAddr += 4);
				}
			}
		}

		private void ReadVersion1(EndianStackReader reader, uint address, bool hasAnimMorphFiles)
		{
			if(hasAnimMorphFiles)
			{
				ReadVersion0(reader, address);
				address += 8;
			}

			if(!reader.TryReadPointer(address, out uint labelsAddress))
			{
				return;
			}

			uint labelPointer = reader.ReadUInt(labelsAddress);
			while(labelPointer != uint.MaxValue)
			{
				uint labelTextPointer = reader.ReadUInt(labelsAddress + 4);
				string labelText = reader.ReadNullterminatedString(labelTextPointer);
				Labels.Add(labelPointer, labelText);

				labelsAddress += 8;
				labelPointer = reader.ReadUInt(labelsAddress);
			}
		}

		private bool ReadMetaBlockType(EndianStackReader reader, ref uint address, MetaBlockType type)
		{
			switch(type)
			{
				case MetaBlockType.Label:
					while(reader.ReadULong(address) != ulong.MaxValue)
					{
						uint labelAddress = reader.ReadUInt(address);
						string label = reader.ReadNullterminatedString(reader.ReadPointer(address += 4));
						if(!Labels.TryAdd(labelAddress, label))
						{
							Labels[labelAddress] = label;
						}

						address += 4;
					}

					break;
				case MetaBlockType.Animation:
					while(reader.ReadUInt(address) != uint.MaxValue)
					{
						AnimFiles.Add(
						reader.ReadNullterminatedString(
								reader.ReadPointer(address)));
						address += 4;
					}

					break;
				case MetaBlockType.Morph:
					while(reader.ReadUInt(address) != uint.MaxValue)
					{
						MorphFiles.Add(
						reader.ReadNullterminatedString(
								reader.ReadPointer(address)));
						address += 4;
					}

					break;
				case MetaBlockType.Author:
					Author = reader.ReadNullterminatedString(address);
					break;
				case MetaBlockType.Description:
					Description = reader.ReadNullterminatedString(address);
					break;
				case MetaBlockType.ActionName:
					ActionName = reader.ReadNullterminatedString(address);
					break;
				case MetaBlockType.ObjectName:
					ObjectName = reader.ReadNullterminatedString(address);
					break;
				case MetaBlockType.Tool:
					break;
				case MetaBlockType.Texture:
					break;
				case MetaBlockType.Weight:
					while(reader.ReadUInt(address) != uint.MaxValue)
					{
						MetaWeights.Add(MetaWeightNode.Read(reader, ref address));
					}

					break;
				case MetaBlockType.End:
					break;
				default:
					return false;
			}

			return true;
		}

		private void ReadVersion2(EndianStackReader reader, uint address)
		{
			if(!reader.TryReadPointer(address, out uint tmpAddr))
			{
				return;
			}

			MetaBlockType type = (MetaBlockType)reader.ReadUInt(tmpAddr);

			while(type != MetaBlockType.End)
			{
				uint blockSize = reader.ReadUInt(tmpAddr + 4);
				uint blockStart = tmpAddr + 8;
				tmpAddr = blockStart + blockSize;

				ReadMetaBlockType(reader, ref blockStart, type);

				type = (MetaBlockType)reader.ReadUInt(tmpAddr);
			}
		}

		private void ReadVersion3(EndianStackReader reader, uint address)
		{
			if(!reader.TryReadPointer(address, out uint metaAddr))
			{
				return;
			}

			MetaBlockType type = (MetaBlockType)reader.ReadUInt(metaAddr);

			while(type != MetaBlockType.End)
			{
				uint blockSize = reader.ReadUInt(metaAddr += 4);
				metaAddr += 4;

				reader.ImageBase = ~metaAddr + 1;
				uint blockAddr = metaAddr;

				if(!ReadMetaBlockType(reader, ref blockAddr, type))
				{
					byte[] block = reader.Slice((int)blockAddr, (int)blockSize).ToArray();
					Other.Add((uint)type, block);
				}

				metaAddr += blockSize;
				type = (MetaBlockType)reader.ReadUInt(metaAddr);
			}
		}

		/// <summary>
		/// Reads meta data off an endian stack reader.
		/// </summary>
		/// <param name="reader">The reader to read from</param>
		/// <param name="address">Address at which to start reading.</param>
		/// <param name="version">File version.</param>
		/// <param name="hasAnimMorphFiles">Whether the file contains animation and morph animation file paths (only applicable to Version 1)</param>
		/// <returns></returns>
		public static MetaData Read(EndianStackReader reader, uint address, int version, bool hasAnimMorphFiles)
		{
			MetaData result = new();
			uint prevImageBase = reader.ImageBase;
			reader.ImageBase = 0;

			switch(version)
			{
				case 0:
					result.ReadVersion0(reader, address);
					break;
				case 1:
					result.ReadVersion1(reader, address, hasAnimMorphFiles);
					break;
				case 2:
					result.ReadVersion2(reader, address);
					break;
				case 3:
					result.ReadVersion3(reader, address);
					break;
				default:
					break;
			}

			reader.ImageBase = prevImageBase;
			return result;
		}


		/// <summary>
		/// Writes the meta data to a stream
		/// </summary>
		/// <param name="writer">Output stream</param>
		public uint Write(EndianStackWriter writer)
		{
			uint result = writer.PointerPosition;

			if(Labels.Count > 0)
			{
				WriteBlock(writer, MetaBlockType.Label, () =>
				{
					uint straddr = (uint)((Labels.Count * 8) + 8);

					foreach(KeyValuePair<uint, string> label in Labels)
					{
						writer.WriteUInt(label.Key);
						writer.WriteUInt(straddr);
						straddr += CalcStringLength(label.Value);
					}

					writer.WriteLong(-1L);

					foreach(KeyValuePair<uint, string> label in Labels)
					{
						WriteString(writer, label.Value);
					}
				});
			}

			if(AnimFiles?.Count > 0)
			{
				WriteStringList(writer, MetaBlockType.Animation, AnimFiles);
			}

			if(MorphFiles?.Count > 0)
			{
				WriteStringList(writer, MetaBlockType.Morph, MorphFiles);
			}

			if(MetaWeights.Count > 0)
			{
				WriteBlock(writer, MetaBlockType.Weight, () =>
				{
					foreach(MetaWeightNode node in MetaWeights)
					{
						node.Write(writer);
					}

					writer.WriteUInt(uint.MaxValue);
				});
			}

			WriteStringBlock(writer, MetaBlockType.Author, Author);
			WriteStringBlock(writer, MetaBlockType.Description, Description);
			WriteStringBlock(writer, MetaBlockType.ActionName, ActionName);
			WriteStringBlock(writer, MetaBlockType.ObjectName, ObjectName);

			foreach(KeyValuePair<uint, byte[]> item in Other)
			{
				writer.WriteUInt(item.Key);
				writer.WriteUInt((uint)item.Value.Length);
				writer.Write(item.Value);
			}

			writer.WriteUInt((uint)MetaBlockType.End);
			writer.WriteUInt(0);

			return result;
		}

		#region Utility methods

		private static void WriteBlock(EndianStackWriter writer, MetaBlockType type, Action write)
		{
			uint start = writer.Position;

			writer.WriteUInt((uint)type);
			writer.WriteEmpty(4);

			write();

			uint bytesWritten = writer.Position - start - 8;

			uint prevPos = writer.Position;
			writer.Seek(start + 4, SeekOrigin.Begin);
			writer.WriteUInt(bytesWritten);
			writer.Seek(prevPos, SeekOrigin.Begin);
		}

		private static void WriteString(EndianStackWriter writer, string value)
		{
			byte[] bytes = Encoding.UTF8.GetBytes(value + "\0");
			uint start = writer.Position;
			writer.Write(bytes);
			writer.AlignFrom(4, start);
		}

		private static uint CalcStringLength(string value)
		{
			uint length = (uint)Encoding.UTF8.GetBytes(value).Length + 1;
			uint padding = length % 4;
			if(padding > 0)
			{
				length += 4 - padding;
			}

			return length;
		}

		private static void WriteStringBlock(EndianStackWriter writer, MetaBlockType type, string? value)
		{
			if(string.IsNullOrEmpty(value))
			{
				return;
			}

			WriteBlock(writer, type, () => WriteString(writer, value));
		}

		private static void WriteStringList(EndianStackWriter writer, MetaBlockType type, List<string> values)
		{
			WriteBlock(writer, type, () =>
			{
				uint straddr = (uint)((values.Count + 1) * 4);

				foreach(string value in values)
				{
					writer.WriteUInt(straddr);
					straddr += CalcStringLength(value);
				}

				writer.WriteUInt(uint.MaxValue);

				foreach(string value in values)
				{
					WriteString(writer, value);
				}
			});
		}

		#endregion
	}
}
