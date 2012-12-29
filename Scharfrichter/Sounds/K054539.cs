using NAudio;
using NAudio.Wave;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Sounds
{
	static public class K054539
	{
		// tail markers used for determining the end of a sample
		private const byte[] tailMarker4Bit = { 0x88, 0x88, 0x88, 0x88 };
		private const byte[] tailMarker8Bit = { 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80, 0x80 };
		private const byte[] tailMarker16Bit = { 0x00, 0x80, 0x00, 0x80, 0x00, 0x80, 0x00, 0x80, 0x00, 0x80, 0x00, 0x80, 0x00, 0x80, 0x00, 0x80 };

		// amplitude table for 4-bit DPCM
		private const byte[] ampTable4Bit = { 0x00, 0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x00, 0xC0, 0xE0, 0xF0, 0xF8, 0xFC, 0xFE, 0xFF };

		// sample volume table
		static private float[] volTab;

		static private float[] VolumeTable
		{
			get
			{
				if (volTab == null)
				{
					volTab = new float[256];
					for (int i = 0; i < 256; i++)
						volTab[i] = (float)Math.Pow(10.0f, (-36.0f * i / 64f) / 20.0f);
				}
				return volTab;
			}
		}

		public struct Properties
		{
			public byte Channel;
			public byte Flags;
			public UInt16 Frequency;
			public UInt32 Offset;
			public byte Panning;
			public byte ReverbVolume;
			public byte SampleType;
			public byte Volume;

			static public Properties Read(Stream source)
			{
				BinaryReaderEx reader = new BinaryReaderEx(source);
				Properties result = new Properties();

				result.Channel = reader.ReadByte();
				result.Frequency = reader.ReadUInt16();
				result.ReverbVolume = reader.ReadByte();
				result.Volume = reader.ReadByte();
				result.Panning = reader.ReadByte();
				result.Offset = reader.ReadUInt24();
				result.SampleType = reader.ReadByte();
				result.Flags = reader.ReadByte();

				return result;
			}

			public void Write(Stream target)
			{
				BinaryWriterEx writer = new BinaryWriterEx(target);
				writer.Write(Channel);
				writer.Write(Frequency);
				writer.Write(ReverbVolume);
				writer.Write(Volume);
				writer.Write(Panning);
				writer.Write24(Offset);
				writer.Write(SampleType);
				writer.Write(Flags);
			}
		}

		// decode mono 4-bit sample to stereo 16-bit sample
		static private byte[] Decode4(byte[] source)
		{
			using (MemoryStream mem = new MemoryStream())
			{
				int length = source.Length;
				int currentSample = 0;

				for (int i = 0; i < length; i++)
				{
					int raw = source[i];
					for (int j = 0; j < 2; j++)
					{
						int tableValue = ampTable4Bit[raw & 0xF];
						currentSample = (currentSample + tableValue) & 0xFF;
						mem.WriteByte(0x00);
						mem.WriteByte((byte)currentSample);
						mem.WriteByte(0x00);
						mem.WriteByte((byte)currentSample);
						raw >>= 4;
					}
				}
				return mem.ToArray();
			}
		}

		// decode 8-bit mono sample to 16-bit stereo sample
		static private byte[] Decode8(byte[] source)
		{
			using (MemoryStream mem = new MemoryStream())
			{
				int length = source.Length;
				for (int i = 0; i < length; i++)
				{
					byte high = source[i];
					mem.WriteByte(0x00);
					mem.WriteByte(high);
					mem.WriteByte(0x00);
					mem.WriteByte(high);
				}
				return mem.ToArray();
			}
		}

		// decode 16-bit mono sample to 16-bit stereo sample
		static private byte[] Decode16(byte[] source)
		{
			using (MemoryStream mem = new MemoryStream())
			{
				int length = source.Length - 1; // - 1 ensures we use an even number
				for (int i = 0; i < length;)
				{
					byte low = source[i++];
					byte high = source[i++];
					mem.WriteByte(low);
					mem.WriteByte(high);
					mem.WriteByte(low);
					mem.WriteByte(high);
				}
				return mem.ToArray();
			}
		}

		// encode 16-bit stereo sample to 4-bit mono sample
		static private byte[] Encode4(byte[] source)
		{
			using (MemoryStream mem = new MemoryStream())
			{
				int length = source.Length;
				for (int i = 0; i < length; i++)
				{
				}
				return mem.ToArray();
			}
		}

		// encode 16-bit stereo sample to 8-bit mono sample
		static private byte[] Encode8(byte[] source)
		{
			using (MemoryStream mem = new MemoryStream())
			{
				int length = source.Length;
				for (int i = 0; i < length; i++)
				{
				}
				return mem.ToArray();
			}
		}

		// encode 16-bit stereo sample to 16-bit mono sample
		static private byte[] Encode16(byte[] source)
		{
			using (MemoryStream mem = new MemoryStream())
			{
				int length = source.Length;
				for (int i = 0; i < length; i++)
				{
				}
				return mem.ToArray();
			}
		}

		// note: I/O functions do not use the Offset field in Properties

		static public Sound Read(Stream source, Properties prop)
		{
			Sound result = new Sound();
			byte[] data = ReadRaw(source, prop);

			// get the true sample rate
			int sampleRate = ((int)prop.Frequency * 44100) / 60126;

			// decode the sound and quantize to 16 bits stereo
			switch (prop.Flags & 0xC)
			{
				case 0x0:
					data = Decode8(data);
					break;
				case 0x4:
					data = Decode16(data);
					break;
				case 0x8:
					data = Decode4(data);
					break;
			}

			// 16-bit stereo format
			result.Format = WaveFormat.CreateCustomFormat(WaveFormatEncoding.Pcm, sampleRate, 2, sampleRate * 4, 4, 16);
			result.Data = data;
			
			// determine volume
			result.Volume = VolumeTable[prop.Volume];
			
			// determine panning
			int panValue = prop.Panning & 0xF;
			if (panValue < 1) 
				panValue = 8;
			if (panValue > 15) 
				panValue = 8;
			result.Panning = (float)(panValue - 1) / 15f;
			
			// return the final result
			return result;
		}

		static public byte[] ReadRaw(Stream source, Properties prop)
		{
			BinaryReaderEx reader = new BinaryReaderEx(source);
			byte[] tailMarker = { };
			int bytesPerSample = 1;

			switch (prop.Flags & 0xC)
			{
				case 0x0:
					tailMarker = tailMarker8Bit;
					break;
				case 0x4:
					tailMarker = tailMarker16Bit;
					bytesPerSample = 2;
					break;
				case 0x8:
					tailMarker = tailMarker4Bit;
					break;
				default:
					throw new Exception("Unable to determine sample type.");
			}

			using (MemoryStream mem = new MemoryStream())
			{
				int tailMarkerLength = tailMarker.Length;
				byte[] buffer = new byte[tailMarkerLength];
				bool finished = false;
				int bufferPad = tailMarkerLength - 1;

				while (!finished)
				{
					// read in the next sample
					for (int j = 0; j < bytesPerSample; j++)
					{
						if (bufferPad > 0)
							bufferPad--;
						else
							mem.WriteByte(buffer[0]);

						for (int i = 1; i < tailMarkerLength; i++)
							buffer[i - 1] = buffer[i];

						int inByte = source.ReadByte();
						if (inByte < 0 || inByte > 255)
							break;

						buffer[tailMarkerLength - 1] = (byte)inByte;
					}

					// check the buffer against the tail marker
					for (int i = 0; i <= tailMarkerLength; i++)
					{
						if (i == tailMarkerLength)
						{
							finished = true;
							break;
						}
						if (buffer[i] != tailMarker[i])
							break;
					}
				}

				return mem.ToArray();
			}
		}

		// returns the number of bytes written
		static public int Write(Sound source, Stream target, Properties prop)
		{
			BinaryWriterEx writer = new BinaryWriterEx(target);
			return 0;
		}

		static public int WriteRaw(byte[] source, Stream target, Properties prop)
		{
			BinaryWriterEx writer = new BinaryWriterEx(target);
			return 0;
		}
	}
}
