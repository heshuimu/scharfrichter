using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec
{
	public class BinaryReaderEx : BinaryReader
	{
		public BinaryReaderEx(Stream source)
			: base(source)
		{
		}

		public byte[] ReadBytesS(int count)
		{
			byte[] input = ReadBytes(count);
			byte[] result = new byte[count];
			for (int i = 0, j = count - 1; i < count; i++)
				result[i] = input[j--];
			return result;
		}

		public Int16 ReadInt16S()
		{
			byte[] input = ReadBytes(2);
			Int16 result = input[0];
			result <<= 8;
			result |= (Int16)input[1];
			return result;
		}

		public Int32 ReadInt24()
		{
			byte[] input = ReadBytes(3);
			Int32 result = input[2];
			result <<= 8;
			result |= (Int32)input[1];
			result <<= 8;
			result |= (Int32)input[0];
			return result;
		}

		public Int32 ReadInt24S()
		{
			byte[] input = ReadBytes(3);
			Int32 result = input[0];
			result <<= 8;
			result |= (Int32)input[1];
			result <<= 8;
			result |= (Int32)input[2];
			return result;
		}

		public Int32 ReadInt32S()
		{
			byte[] input = ReadBytes(4);
			Int32 result = input[0];
			result <<= 8;
			result |= (Int32)input[1];
			result <<= 8;
			result |= (Int32)input[2];
			result <<= 8;
			result |= (Int32)input[3];
			return result;
		}

		public Int64 ReadInt64S()
		{
			byte[] input = ReadBytes(8);
			Int64 result = input[0];
			result <<= 8;
			result |= (Int64)input[1];
			result <<= 8;
			result |= (Int64)input[2];
			result <<= 8;
			result |= (Int64)input[3];
			result <<= 8;
			result |= (Int64)input[4];
			result <<= 8;
			result |= (Int64)input[5];
			result <<= 8;
			result |= (Int64)input[6];
			result <<= 8;
			result |= (Int64)input[7];
			return result;
		}

		public byte[] ReadMD5()
		{
			return ReadBytes(16);
		}

		public byte[] ReadMD5S()
		{
			return ReadBytesS(16);
		}

		public byte[] ReadSHA1()
		{
			return ReadBytes(20);
		}

		public byte[] ReadSHA1S()
		{
			return ReadBytesS(20);
		}

		public UInt16 ReadUInt16S()
		{
			byte[] input = ReadBytes(2);
			UInt16 result = input[0];
			result <<= 8;
			result |= input[1];
			return result;
		}

		public UInt32 ReadUInt24()
		{
			byte[] input = ReadBytes(3);
			UInt32 result = input[2];
			result <<= 8;
			result |= (UInt32)input[1];
			result <<= 8;
			result |= (UInt32)input[0];
			return result;
		}

		public UInt32 ReadUInt24S()
		{
			byte[] input = ReadBytes(3);
			UInt32 result = input[0];
			result <<= 8;
			result |= (UInt32)input[1];
			result <<= 8;
			result |= (UInt32)input[2];
			return result;
		}

		public UInt32 ReadUInt32S()
		{
			byte[] input = ReadBytes(4);
			UInt32 result = input[0];
			result <<= 8;
			result |= input[1];
			result <<= 8;
			result |= input[2];
			result <<= 8;
			result |= input[3];
			return result;
		}

		public UInt64 ReadUInt64S()
		{
			byte[] input = ReadBytes(8);
			UInt64 result = input[0];
			result <<= 8;
			result |= input[1];
			result <<= 8;
			result |= input[2];
			result <<= 8;
			result |= input[3];
			result <<= 8;
			result |= input[4];
			result <<= 8;
			result |= input[5];
			result <<= 8;
			result |= input[6];
			result <<= 8;
			result |= input[7];
			return result;
		}
	}

	public class BinaryWriterEx : BinaryWriter
	{
		public BinaryWriterEx(Stream target)
			: base(target)
		{
		}

		public void Write24(Int32 value)
		{
			Write((byte)((value) & 0xFF));
			Write((byte)((value >> 8) & 0xFF));
			Write((byte)((value >> 16) & 0xFF));
		}

		public void Write24(UInt32 value)
		{
			Write((byte)((value) & 0xFF));
			Write((byte)((value >> 8) & 0xFF));
			Write((byte)((value >> 16) & 0xFF));
		}

		public void Write24S(Int32 value)
		{
			Write((byte)((value >> 16) & 0xFF));
			Write((byte)((value >> 8) & 0xFF));
			Write((byte)((value) & 0xFF));
		}

		public void Write24S(UInt32 value)
		{
			Write((byte)((value >> 16) & 0xFF));
			Write((byte)((value >> 8) & 0xFF));
			Write((byte)((value) & 0xFF));
		}

		public void WriteS(Int16 value)
		{
			Write((byte)((value >> 8) & 0xFF));
			Write((byte)((value ) & 0xFF));
		}

		public void WriteS(Int32 value)
		{
			Write((byte)((value >> 24) & 0xFF));
			Write((byte)((value >> 16) & 0xFF));
			Write((byte)((value >> 8) & 0xFF));
			Write((byte)((value) & 0xFF));
		}

		public void WriteS(Int64 value)
		{
			Write((byte)((value >> 56) & 0xFF));
			Write((byte)((value >> 48) & 0xFF));
			Write((byte)((value >> 40) & 0xFF));
			Write((byte)((value >> 32) & 0xFF));
			Write((byte)((value >> 24) & 0xFF));
			Write((byte)((value >> 16) & 0xFF));
			Write((byte)((value >> 8) & 0xFF));
			Write((byte)((value) & 0xFF));
		}

		public void WriteS(UInt16 value)
		{
			Write((byte)((value >> 8) & 0xFF));
			Write((byte)((value) & 0xFF));
		}

		public void WriteS(UInt32 value)
		{
			Write((byte)((value >> 24) & 0xFF));
			Write((byte)((value >> 16) & 0xFF));
			Write((byte)((value >> 8) & 0xFF));
			Write((byte)((value) & 0xFF));
		}

		public void WriteS(UInt64 value)
		{
			Write((byte)((value >> 56) & 0xFF));
			Write((byte)((value >> 48) & 0xFF));
			Write((byte)((value >> 40) & 0xFF));
			Write((byte)((value >> 32) & 0xFF));
			Write((byte)((value >> 24) & 0xFF));
			Write((byte)((value >> 16) & 0xFF));
			Write((byte)((value >> 8) & 0xFF));
			Write((byte)((value) & 0xFF));
		}

		public void WriteS(byte[] value)
		{
			for (int i = value.Length - 1; i >= 0; i--)
				Write(value[i]);
		}
	}
}
