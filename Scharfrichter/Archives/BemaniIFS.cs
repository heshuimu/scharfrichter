using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Archives
{
	public class BemaniIFS : Archive
	{

		private List<byte[]> files = new List<byte[]>();

		public override byte[][] RawData
		{
			get
			{
				return files.ToArray();
			}
			set
			{
				files.Clear();
				files.AddRange(value);
			}
		}

		public override int RawDataCount
		{
			get
			{
				return files.Count;
			}
		}

		static public BemaniIFS Read(Stream source)
		{
			List<byte[]> dataList = new List<byte[]>();
			BinaryReaderEx reader = new BinaryReaderEx(source);
			BemaniIFS result = new BemaniIFS();

			// header length is 0x28 bytes
			reader.ReadInt32(); // identifier
			Int16 headerMetaLength = reader.ReadInt16S(); // header meta amount?
			reader.ReadInt16S(); // bitwise xor 0xFFFF of previously read value
			reader.ReadInt32();
			reader.ReadInt32();
			Int32 headerLength = SwapEndian(reader.ReadInt32());
			reader.ReadInt32();

			for (int i = 1; i < headerMetaLength; i++)
			{
				reader.ReadInt32();
				reader.ReadInt32();
			}

			Console.WriteLine("Header length: " + headerLength.ToString());

			// read table A
			Int32 tableALength = SwapEndian(reader.ReadInt32());
			Console.WriteLine("Table A length: " + tableALength.ToString());
			MemoryStream tableAMem = new MemoryStream(reader.ReadBytes(tableALength));

			// read table B
			Int32 tableBLength = SwapEndian(reader.ReadInt32());
			Console.WriteLine("Table B length: " + tableBLength.ToString());
			MemoryStream tableBMem = new MemoryStream(reader.ReadBytes(tableBLength));

			// read padding
			int headerPadding = headerLength - (0x10 + (headerMetaLength * 8) + 4 + tableALength + 4 + tableBLength);
			if (headerPadding > 0)
				reader.ReadBytes(headerPadding);

			// a bit of a hack to get the info we need (it's probably not accurate)
			BinaryReaderEx tableAReader = new BinaryReaderEx(tableAMem);
			BinaryReaderEx tableBReader = new BinaryReaderEx(tableBMem);

			tableAReader.BaseStream.Position = 0x18;
			tableBReader.BaseStream.Position = 0x14;
			int dataLength = tableBReader.ReadInt32S();
			MemoryStream dataChunk = new MemoryStream(reader.ReadBytes(dataLength));
			BinaryReaderEx dataReader = new BinaryReaderEx(dataChunk);

			// process tables
			int chunkIndex = 0;
			bool processTable = true;
			while (processTable)
			{
				Console.Write("A:" + Util.ConvertToHexString((int)tableAReader.BaseStream.Position, 8) + " B:" + Util.ConvertToHexString((int)tableBReader.BaseStream.Position, 8) + " ");
				byte chunkType = tableAReader.ReadByte();
				Console.Write("Op:" + Util.ConvertToHexString(chunkType, 2) + "  ");
				switch (chunkType)
				{
					case 0x06: // directory
						{
							byte subType = tableAReader.ReadByte();
							switch (subType)
							{
								case 0x03:
									tableAReader.ReadBytes(3);
									break;
								case 0x06:
									break;
								default:
									break;
							}
							Int32 fileModified = tableBReader.ReadInt32S(); // modified date?
							Console.WriteLine("*" + Util.ConvertToHexString(fileModified, 8));
						}
						continue;
					case 0x1E: // file
						tableAReader.ReadByte();
						{
							Int32 fileOffset = tableBReader.ReadInt32S(); // offset
							Int32 fileLength = tableBReader.ReadInt32S(); // length
							Int32 fileModified = tableBReader.ReadInt32S(); // modified date?
							Console.WriteLine(Util.ConvertToHexString(fileOffset, 8) + ":" + Util.ConvertToHexString(fileLength, 8) + ", *" + Util.ConvertToHexString(fileModified, 8));
							dataReader.BaseStream.Position = fileOffset;
							dataList.Add(dataReader.ReadBytes(fileLength));
						}
						break;
					case 0x94: // filename
						Console.WriteLine("FileID: " + Util.ConvertToHexString(tableAReader.ReadInt32S(), 8));
						continue;
					case 0xFE: // end of entry
						Console.WriteLine("End of entry.");
						break;
					case 0xFF: // end of list
						processTable = false;
						Console.WriteLine("End of list.");
						continue;
					default:
						// for types we don't know, skip the whole line for now
						Console.WriteLine("UNKNOWN.");
						break;
				}
				while (chunkType != 0xFE)
				{
					chunkType = tableAReader.ReadByte();
				}
				chunkIndex++;
			}

			result.files = dataList;
			return result;
		}

		static private Int32 SwapEndian(Int32 operand)
		{
			Int32 a = (operand >> 0) & (0xFF);
			Int32 b = (operand >> 8) & (0xFF);
			Int32 c = (operand >> 16) & (0xFF);
			Int32 d = (operand >> 24) & (0xFF);
			Int32 result = a;
			result <<= 8;
			result |= b;
			result <<= 8;
			result |= c;
			result <<= 8;
			result |= d;
			return result;
		}
	}
}
