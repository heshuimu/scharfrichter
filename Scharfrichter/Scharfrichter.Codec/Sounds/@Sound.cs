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
	public class Sound
	{
		public byte[] Data;
		public WaveFormat Format;

		public void Write(Stream target)
		{
			using (MemoryStream mem = new MemoryStream())
			{
				RawSourceWaveStream wave = new RawSourceWaveStream(new MemoryStream(Data), Format);
				VolumeWaveProvider16 vol = new VolumeWaveProvider16(wave);
				vol.Volume = 0.5f;

				using (WaveWriter writer = new WaveWriter(mem, Format))
				{
					byte[] finalData = new byte[Data.Length];
					vol.Read(finalData, 0, finalData.Length);
					writer.Write(finalData, 0, finalData.Length);
					writer.Update();
					target.Write(mem.ToArray(), 0, (int)mem.Length);
				}
			}
		}
	}
}
