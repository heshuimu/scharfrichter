using NAudio;
using NAudio.Codecs;
using NAudio.Wave;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Sounds
{
	static public class Bemani2DXSound
	{
		// sample volume table
		// TODO: determine correctness. I borrowed the formula from bmIII.
		static private float[] volTab;
		static private float[] VolumeTable
		{
			get
			{
				if (volTab == null)
				{
					volTab = new float[256];
					for (int i = 0; i < 256; i++)
						volTab[i] = (float)Math.Pow(10.0f, (-36.0f * i / 144f) / 20.0f);
				}
				return volTab;
			}
		}

		static public Sound Read(Stream source)
		{
			Sound result = new Sound();
			BinaryReader reader = new BinaryReader(source);
			if (new string(reader.ReadChars(4)) == "2DX9")
			{
				int infoLength = reader.ReadInt32();
				int dataLength = reader.ReadInt32();
				reader.ReadInt32();
				int panning = reader.ReadInt16();
				int volume = reader.ReadInt16();
				reader.ReadBytes(infoLength - 20);

				byte[] wavData = reader.ReadBytes(dataLength);
				using (MemoryStream wavDataMem = new MemoryStream(wavData))
				{
					using (WaveStream wavStream = new WaveFileReader(wavDataMem))
					{
						using (WaveStream wavConvertStream = WaveFormatConversionStream.CreatePcmStream(wavStream))
						{
							IWaveProvider sourceProvider;
							int bytesToRead;

							// using a mux, we force all sounds to be 2 channels
							sourceProvider = new MultiplexingWaveProvider(new IWaveProvider[] { wavConvertStream }, 2);
							bytesToRead = (int)((wavConvertStream.Length * 2) / wavConvertStream.WaveFormat.Channels);

							byte[] rawWaveData = new byte[bytesToRead];
							int bytesRead = sourceProvider.Read(rawWaveData, 0, bytesToRead);
							result.Data = rawWaveData;
							result.Format = sourceProvider.WaveFormat;

							// calculate output panning
							if (panning > 0x7F || panning < 0x01)
								panning = 0x40;
							result.Panning = ((float)panning - 1.0f) / 126.0f;

							// calculate output volume
							if (volume < 0x01)
								volume = 0x01;
							else if (volume > 0xFF)
								volume = 0xFF;
							result.Volume = VolumeTable[volume];
						}
					}
				}
			}

			return result;
		}
	}
}
