using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Media
{
	public partial class CHD : Stream
	{
		public static CHD Load(Stream source)
		{
			CHD result = new CHD();
			BinaryReaderEx reader = new BinaryReaderEx(source);

			if (new string(reader.ReadChars(8)) != "MComprHD")
				return null;

			UInt32 headerLength = reader.ReadUInt32S();
			UInt32 version = reader.ReadUInt32S();

			switch (version)
			{
				case 1:
					result.ReadHeaderV1(reader);
					break;
				case 2:
					result.ReadHeaderV2(reader);
					break;
				case 3:
					result.ReadHeaderV3(reader);
					break;
				case 4:
					result.ReadHeaderV4(reader);
					break;
				case 5:
					result.ReadHeaderV5(reader);
					break;
				default:
					return null;
			}

			return result;
		}

		private void ReadHeaderV1(BinaryReaderEx reader)
		{
			UInt32 flags = reader.ReadUInt32S();
			UInt32 compression = reader.ReadUInt32S();
			UInt32 hunkSize = reader.ReadUInt32S();
			UInt32 totalHunks = reader.ReadUInt32S();
			UInt32 cylinders = reader.ReadUInt32S();
			UInt32 heads = reader.ReadUInt32S();
			UInt32 sectors = reader.ReadUInt32S();
			byte[] md5 = reader.ReadBytesS(16);
			byte[] parentmd5 = reader.ReadBytesS(16);
		}

		private void ReadHeaderV2(BinaryReaderEx reader)
		{
			UInt32 flags = reader.ReadUInt32S();
			UInt32 compression = reader.ReadUInt32S();
			UInt32 hunkSize = reader.ReadUInt32S();
			UInt32 totalHunks = reader.ReadUInt32S();
			UInt32 cylinders = reader.ReadUInt32S();
			UInt32 heads = reader.ReadUInt32S();
			UInt32 sectors = reader.ReadUInt32S();
			byte[] md5 = reader.ReadBytesS(16);
			byte[] parentmd5 = reader.ReadBytesS(16);
			UInt32 seclen = reader.ReadUInt32S();
		}

		private void ReadHeaderV3(BinaryReaderEx reader)
		{
			UInt32 flags = reader.ReadUInt32S();
			UInt32 compression = reader.ReadUInt32S();
			UInt32 totalHunks = reader.ReadUInt32S();
			UInt64 logicalBytes = reader.ReadUInt64S();
			UInt64 metaOffset = reader.ReadUInt64S();
		}

		private void ReadHeaderV4(BinaryReaderEx reader)
		{
		}

		private void ReadHeaderV5(BinaryReaderEx reader)
		{
		}
	}
}
