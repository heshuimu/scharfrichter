using Scharfrichter.Codec.Charts;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Archives
{
	public class Bemani1 : Archive
	{
		private Chart[] charts = new Chart[12];

		public override Chart[] Charts
		{
			get
			{
				return charts;
			}
			set
			{
				if (value.Length == 12)
					charts = value;
			}
		}

		public override int ChartCount
		{
			get
			{
				return 12;
			}
		}

		public static Bemani1 Read(Stream source, long unitNumerator, long unitDenominator)
		{
			Bemani1 result = new Bemani1();
			long offsetBase = source.Position;

			using (BinaryReader reader = new BinaryReader(source))
			{
				int[] offset = new int[12];
				int[] length = new int[12];

				for (int i = 0; i < 12; i++)
				{
					offset[i] = reader.ReadInt32();
					length[i] = reader.ReadInt32();
				}

				for (int i = 0; i < 12; i++)
				{
					if (length[i] > 0 && offset[i] >= 0x60)
					{
						Chart chart = new Chart();
						source.Position = offsetBase + offset[i];

						byte[] chartData = reader.ReadBytes(length[i]);

						using (MemoryStream mem = new MemoryStream(chartData))
						{
							using (BinaryReader memReader = new BinaryReader(mem))
							{
								while (mem.Position < mem.Length)
								{
									Entry entry = new Entry();
									long eventOffset = memReader.ReadInt32();

									if (eventOffset >= 0x7FFFFFFF)
										break;

									entry.LinearOffset = new Fraction(eventOffset * unitNumerator, unitDenominator);
									entry.Value = new Fraction(0, 1);

									int eventType = memReader.ReadByte();
									int eventParameter = memReader.ReadByte();
									int eventValue = memReader.ReadInt16();

									// unhandled parameter types:
									//  0x05: measure length
									//  0x08: judgement
									//  0x10: note count
									switch (eventType)
									{
										case 0x00: entry.Type = EntryType.Marker; entry.Player = 1; entry.Column = eventParameter; break;
										case 0x01: entry.Type = EntryType.Marker; entry.Player = 2; entry.Column = eventParameter; break;
										case 0x02: entry.Type = EntryType.Sample; entry.Player = 1; entry.Column = eventParameter; entry.Value = new Fraction(eventValue, 1); break;
										case 0x03: entry.Type = EntryType.Sample; entry.Player = 2; entry.Column = eventParameter; entry.Value = new Fraction(eventValue, 1); break;
										case 0x04: entry.Type = EntryType.Tempo; entry.Value = new Fraction(eventValue, eventParameter); break;
										case 0x06: entry.Type = EntryType.EndOfSong; break;
										case 0x07: entry.Type = EntryType.Marker; entry.Player = 0; entry.Value = new Fraction(eventValue, 1); break;
										case 0x0C: entry.Type = (eventParameter == 0 ? EntryType.Measure : EntryType.Invalid); break;
										default: entry.Type = EntryType.Invalid; break;
									}

									if (entry.Type != EntryType.Invalid)
										chart.Entries.Add(entry);
								}
								chart.Entries.Sort();
							}
						}

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

						if (chart.Entries.Count > 0)
							result.charts[i] = chart;
						else
							result.charts[i] = null;
					}
				}
			}

			return result;
		}

		public void Write(Stream target)
		{
		}
	}
}
