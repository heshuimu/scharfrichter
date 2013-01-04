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
			BinaryReader reader = new BinaryReader(source);
			BemaniIFS result = new BemaniIFS();

			// header length is 0x28 bytes
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();
			Int32 headerLength = SwapEndian(reader.ReadInt32());
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();
			reader.ReadInt32();

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
			int headerPadding = headerLength - (0x28 + 4 + tableALength + 4 + tableBLength);
			if (headerPadding > 0)
				reader.ReadBytes(headerPadding);

			// a bit of a hack to get the info we need (it's probably not accurate)
			BinaryReaderEx tableAReader = new BinaryReaderEx(tableAMem);
			BinaryReaderEx tableBReader = new BinaryReaderEx(tableBMem);

			tableAReader.BaseStream.Position = 0x18;
			tableBReader.BaseStream.Position = 0x14;
			int dataLength = tableBReader.ReadInt32S();

			// process tables
			int chunkIndex = 0;
			bool processTable = true;
			while (processTable)
			{
				byte chunkType = tableAReader.ReadByte();
				switch (chunkType)
				{
					case 0x06: // directory
						tableAReader.ReadByte();
						tableBReader.ReadInt32S(); // modified date?
						break;
					case 0x1E: // file
						tableAReader.ReadByte();
						tableBReader.ReadInt32S(); // offset
						tableBReader.ReadInt32S(); // length
						tableBReader.ReadInt32S(); // modified date?
						break;
					case 0x3C: // unknown (music files)
						tableAReader.ReadBytes(2);
						break;
					case 0x4F: // unknown (music files)
						tableAReader.ReadByte();
						break;
					case 0x65: // unknown (music files)
					case 0xA5:
						tableAReader.ReadBytes(3);
						break;
					case 0x94: // filename
						tableAReader.ReadInt32S();
						break;
					case 0xFE: // end of entry
						chunkIndex++;
						continue;
					case 0xFF: // end of list
						processTable = false;
						break;
					default:
						break;
				}
			}

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
