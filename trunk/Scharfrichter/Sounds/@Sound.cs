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
		public float Panning = 0.5f;
		public float Volume = 0.5f;

		public void Write(Stream target, float masterVolume)
		{
			if (Data != null && Data.Length > 0)
			{
				using (MemoryStream mem = new MemoryStream())
				{
					MemoryStream sourceLeft = new MemoryStream(Data);
					MemoryStream sourceRight = new MemoryStream(Data);
					RawSourceWaveStream waveLeft = new RawSourceWaveStream(sourceLeft, Format);
					RawSourceWaveStream waveRight = new RawSourceWaveStream(sourceRight, Format);

					// step 1: separate the stereo stream
					MultiplexingWaveProvider demuxLeft = new MultiplexingWaveProvider(new IWaveProvider[] { waveLeft }, 1);
					MultiplexingWaveProvider demuxRight = new MultiplexingWaveProvider(new IWaveProvider[] { waveRight }, 1);
					demuxLeft.ConnectInputToOutput(0, 0);
					demuxRight.ConnectInputToOutput(1, 0);

					// step 2: adjust the volume of a stereo stream
					VolumeWaveProvider16 volLeft = new VolumeWaveProvider16(demuxLeft);
					VolumeWaveProvider16 volRight = new VolumeWaveProvider16(demuxRight);

					// note: use logarithmic scale
#if (true)
					// log scale is applied to each operation
					float volumeValueLeft = (float)Math.Pow(1.0f - Panning, 0.5f);
					float volumeValueRight = (float)Math.Pow(Panning, 0.5f);
					volumeValueLeft *= (float)Math.Pow(Volume, 0.5f);
					volumeValueRight *= (float)Math.Pow(Volume, 0.5f);
					volumeValueLeft = Math.Min(Math.Max(volumeValueLeft, 0.0f), 1.0f);
					volumeValueRight = Math.Min(Math.Max(volumeValueRight, 0.0f), 1.0f);
#else
					// log scale is applied to the result of the operations
					float volumeValueLeft = (float)Math.Pow(1.0f - Panning, 0.5f);
					float volumeValueRight = (float)Math.Pow(Panning, 0.5f);
					volumeValueLeft *= Volume;
					volumeValueRight *= Volume;
					volumeValueLeft = (float)Math.Pow(volumeValueLeft, 0.5f);
					volumeValueRight = (float)Math.Pow(volumeValueRight, 0.5f);
					volumeValueLeft = Math.Min(Math.Max(volumeValueLeft, 0.0f), 1.0f);
					volumeValueRight = Math.Min(Math.Max(volumeValueRight, 0.0f), 1.0f);
#endif
					// use linear scale for master volume
					volLeft.Volume = volumeValueLeft * masterVolume;
					volRight.Volume = volumeValueRight * masterVolume;

					// step 3: combine them again
					IWaveProvider[] tracks = new IWaveProvider[] { volLeft, volRight };
					MultiplexingWaveProvider mux = new MultiplexingWaveProvider(tracks, 2);

					// step 4: export them to a byte array
					byte[] finalData = new byte[Data.Length];
					mux.Read(finalData, 0, finalData.Length);

					using (WaveWriter writer = new WaveWriter(mem, Format))
					{
						writer.Write(finalData, 0, finalData.Length);
						writer.Update();
						target.Write(mem.ToArray(), 0, (int)mem.Length);
					}
				}
			}
		}
	}
}
