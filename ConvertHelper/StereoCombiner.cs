using NAudio.Wave;
using Scharfrichter.Codec.Charts;
using Scharfrichter.Codec.Sounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ConvertHelper
{
    static public class StereoCombiner
    {
        static public void Process(Sound[] sounds, Chart[] charts)
        {
            List<int> keysoundsUsed = new List<int>();
            List<int> bgmKeysounds = new List<int>();

            foreach (Chart chart in charts)
            {
                foreach (Entry entry in chart.Entries)
                {
                    if (entry.Type == EntryType.Sample || entry.Type == EntryType.Marker)
                    {
                        if (entry.Value.Denominator == 1 && entry.Value.Numerator > 0)
                        {
                            int noteValue = (int)entry.Value.Numerator;
                            if (!keysoundsUsed.Contains(noteValue))
                            {
                                keysoundsUsed.Add(noteValue);
                                if (!bgmKeysounds.Contains(noteValue))
                                {
                                    bgmKeysounds.Add(noteValue);
                                }
                            }
                            if (entry.Player != 0)
                            {
                                bgmKeysounds.Remove(noteValue);
                            }
                        }
                    }
                }
            }

            int count = sounds.Length;
            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    if (bgmKeysounds.Contains(i + 1) && bgmKeysounds.Contains(j + 1))
                    {
                        if (sounds[i].Data.Length == sounds[j].Data.Length && Math.Abs(sounds[i].Panning - sounds[j].Panning) == 1)
                        {
                            byte[] render0 = sounds[i].Render(1.0f);
                            byte[] render1 = sounds[j].Render(1.0f);
                            int renderLength = render0.Length;
                            byte[] output = new byte[renderLength];
                            for (int k = 0; k < renderLength; k++)
                            {
                                output[k] = (byte)(render0[k] | render1[k]);
                            }
                            sounds[i].SetSound(output, sounds[i].Format);
                            sounds[j].SetSound(new byte[] { }, sounds[j].Format);
                            sounds[i].Panning = 0.5f;
                            sounds[i].Volume = 1.0f;
                        }
                    }
                }
            }
        }
    }
}
