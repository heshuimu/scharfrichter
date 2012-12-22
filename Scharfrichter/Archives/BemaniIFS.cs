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

		private struct Entry
		{
			public Int32 Offset;
			public Int32 Length;
			public Int32 Meta;
		}

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
			List<Entry> entries = new List<Entry>();
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
			using (MemoryStream mem = new MemoryStream(reader.ReadBytes(tableALength)))
			{
				BinaryReader tableReader = new BinaryReader(mem);
			}

			// read table B
			Int32 totalDataLength = 0;
			Int32 tableBLength = SwapEndian(reader.ReadInt32());
			Console.WriteLine("Table B length: " + tableBLength.ToString());
			using (MemoryStream mem = new MemoryStream(reader.ReadBytes(tableBLength)))
			{
				BinaryReader tableReader = new BinaryReader(mem);
				Int32 metaDataLength = SwapEndian(tableReader.ReadInt32());
				byte[] metaData = tableReader.ReadBytes(metaDataLength);
				totalDataLength = SwapEndian(tableReader.ReadInt32());
				tableReader.ReadInt32();
				Int32 tableCount = (tableBLength - (4 + metaDataLength + 8)) / 0xC;
				Console.WriteLine("Table B entries: " + tableCount.ToString());
				for (int i = 0; i < tableCount; i++)
				{
					Entry entry = new Entry();
					entry.Offset = SwapEndian(tableReader.ReadInt32());
					entry.Length = SwapEndian(tableReader.ReadInt32());
					entry.Meta = SwapEndian(tableReader.ReadInt32());
					entries.Add(entry);
				}
			}

			int headerPadding = headerLength - (0x28 + 4 + tableALength + 4 + tableBLength);
			if (headerPadding > 0)
				reader.ReadBytes(headerPadding);

			// read data chunk
			using (MemoryStream mem = new MemoryStream(reader.ReadBytes(totalDataLength)))
			{
				for (int i = 0; i < entries.Count; i++)
				{
					byte[] data;
					Entry entry = entries[i];

					mem.Position = entry.Offset;
					data = new byte[entry.Length];
					mem.Read(data, 0, data.Length);
					result.files.Add(data);
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
