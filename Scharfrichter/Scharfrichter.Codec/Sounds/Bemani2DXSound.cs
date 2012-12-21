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
							byte[] rawWaveData = new byte[wavConvertStream.Length];
							wavConvertStream.Read(rawWaveData, 0, (int)wavConvertStream.Length);
							result.Format = wavConvertStream.WaveFormat;
							result.Data = rawWaveData;
						}
					}
				}
			}

			return result;
		}
	}
}
