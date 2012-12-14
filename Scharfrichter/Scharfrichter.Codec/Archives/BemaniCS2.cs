using Scharfrichter.Codec.Charts;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Archives
{
	public class BemaniCS2 : Archive
	{
		private Chart chart;

		public override Chart[] Charts
		{
			get
			{
				return new Chart[] { chart };
			}
			set
			{
				chart = value[0];
			}
		}

		public override int ChartCount
		{
			get
			{
				return 1;
			}
		}

		public static BemaniCS2 Read(Stream source)
		{
			BemaniCS2 result = new BemaniCS2();
			BinaryReader reader = new BinaryReader(source);
			Chart chart = new Chart();

			if (reader.ReadInt32() != 0x00000008)
				throw new Exception("Can't load this CS2 file: invalid signature.");

			long granularity = reader.ReadInt32();

			while (true)
			{
				Entry entry = new Entry();
				long eventOffset = reader.ReadInt16();

				if (eventOffset >= 0x7FFF)
					break;

				entry.LinearOffset = new Fraction(eventOffset * granularity, 1000000);
				entry.Value = new Fraction(0, 1);

				int eventType = reader.ReadByte();
				int eventValue = reader.ReadByte();
				int eventParameter;

				eventParameter = eventType >> 4;
				eventType &= 0xF;

				// unhandled parameter types:
				//  0x05: measure length
				//  also does not interpret note count 100% (it's a carryover from older bm formats)

				switch (eventType)
				{
					case 0x00: if (eventOffset > 0) { entry.Type = EntryType.Marker; entry.Player = 1; entry.Column = eventParameter; } break;
					case 0x01: if (eventOffset > 0) { entry.Type = EntryType.Marker; entry.Player = 2; entry.Column = eventParameter; } break;
					case 0x02: entry.Type = EntryType.Sample; entry.Player = 1; entry.Column = eventParameter; entry.Value = new Fraction(eventValue, 1); break;
					case 0x03: entry.Type = EntryType.Sample; entry.Player = 2; entry.Column = eventParameter; entry.Value = new Fraction(eventValue, 1); break;
					case 0x04: entry.Type = EntryType.Tempo; entry.Value = new Fraction(eventValue + (eventParameter * 256), 1); break;
					case 0x06: entry.Type = EntryType.EndOfSong; entry.Player = eventParameter + 1; break;
					case 0x07: entry.Type = EntryType.Marker; entry.Player = 0; entry.Value = new Fraction(eventValue, 1); entry.Parameter = eventParameter; break;
					case 0x08: entry.Type = EntryType.Judgement; entry.Player = 0; entry.Value = new Fraction(eventValue, 1); entry.Parameter = eventParameter; break;
					case 0x0C: entry.Type = (eventParameter == 0 ? EntryType.Measure : EntryType.Invalid); entry.Player = eventParameter + 1; break;
					default: entry.Type = EntryType.Invalid; break;
				}

				if (entry.Type != EntryType.Invalid)
					chart.Entries.Add(entry);
			}
			chart.Entries.Sort();

			// find the default bpm
			foreach (Entry entry in chart.Entries)
			{
				if (entry.Type == EntryType.Tempo)
				{
					chart.DefaultBPM = entry.Value;
					break;
				}
			}

			// fill in the metric offsets
			chart.CalculateMetricOffsets();
			
			result.chart = chart;
			return result;
		}

		public void Write(Stream target, long unitNumerator, long unitDenominator)
		{
		}
	}
}
