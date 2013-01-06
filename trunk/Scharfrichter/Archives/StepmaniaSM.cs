using Scharfrichter.Codec.Charts;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Archives
{
	public class StepmaniaSM : Archive
	{
		public Dictionary<string, string> Tags = new Dictionary<string, string>();

		public void CreateStepTag(Entry[] entries, string gameType, string description, string difficulty, string playLevel, string grooveRadar, int panelCount, int quantize)
		{
			string tagName = "NOTES:" + gameType + ":" + description + ":" + difficulty + ":" + playLevel + ":" + grooveRadar;
			int count = entries.Length;
			int highestMeasure = entries[count - 1].MetricMeasure + 2;
			double quantDouble = (double)quantize;

			int[, ,] notes = new int[highestMeasure, quantize, panelCount];
			int[,] lastNoteData = new int[panelCount, 2];

			foreach (Entry entry in entries)
			{
				int noteData = 1;
				if (entry.Column < panelCount)
				{
					if (entry.Freeze)
					{
						noteData = 3;
						int test = notes[lastNoteData[entry.Column, 0], lastNoteData[entry.Column, 1], entry.Column];
						notes[lastNoteData[entry.Column, 0], lastNoteData[entry.Column, 1], entry.Column] = 2;
					}
					else if (entry.Type == EntryType.Mine)
					{
						noteData = -1;
					}
					int offset = (int)Math.Truncate(quantDouble * (double)entry.MetricOffset);
					notes[entry.MetricMeasure, offset, entry.Column] = noteData;
					lastNoteData[entry.Column, 0] = entry.MetricMeasure;
					lastNoteData[entry.Column, 1] = offset;
				}
			}

			StringBuilder builder = new StringBuilder();
			builder.AppendLine();
			for (int measure = 0; measure < highestMeasure; measure++)
			{
				if (measure > 0)
					builder.AppendLine(",");

				for (int offset = 0; offset < quantize; offset++)
				{
					for (int column = 0; column < panelCount; column++)
					{
						switch (notes[measure, offset, column])
						{
							case -1: builder.Append("M"); break;
							case 1: builder.Append("1"); break;
							case 2: builder.Append("2"); break;
							case 3: builder.Append("3"); break;
							default: builder.Append("0"); break;
						}
					}
					builder.AppendLine();
				}
			}

			Tags[tagName] = builder.ToString();
		}

		public void CreateTempoTags(Entry[] entries)
		{
			// build the BPMS and STOPS tags
			string bpmTag = "";
			string stopTag = "";
			int bpmCount = entries.Length;
			for (int i = 0; i < bpmCount; i++)
			{
				Entry entry = entries[i];
				double offset = Math.Round(((double)entry.MetricOffset + (double)entry.MetricMeasure) * 4f, 3);
				double value = Math.Round((double)entry.Value, 3);

				if (value > 0)
				{
					if (bpmTag.Length > 0)
						bpmTag += ",";
					bpmTag += offset.ToString();
					bpmTag += "=";
					bpmTag += value.ToString();
				}
				else if (i < (bpmCount - 1))
				{
					double stopLength = Math.Abs(Math.Round((double)(entries[i + 1].LinearOffset - entries[i].LinearOffset), 3));
					if (stopTag.Length > 0)
						stopTag += ",";
					stopTag += offset.ToString();
					stopTag += "=";
					stopTag += stopLength.ToString();
				}
			}

			Tags["BPMs"] = bpmTag;
			Tags["Stops"] = stopTag;
		}

		public void Write(Stream target)
		{
			StreamWriter writer = new StreamWriter(target);
			foreach (KeyValuePair<string, string> tag in Tags)
			{
				string val = "#" + tag.Key.ToUpper() + ":" + tag.Value + ";";
				writer.WriteLine(val);
			}
			writer.Flush();
		}

		public void WriteFile(string filename)
		{
			using (MemoryStream mem = new MemoryStream())
			{
				Write(mem);
				File.WriteAllBytes(filename, mem.ToArray());
			}
		}
	}
}
