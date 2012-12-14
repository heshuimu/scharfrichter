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
			BinaryReader reader = new BinaryReader(source);

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
						BinaryReader memReader = new BinaryReader(mem);
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

			return result;
		}

		public void Write(Stream target, long unitNumerator, long unitDenominator)
		{
			Fraction unit = new Fraction(unitDenominator, unitNumerator);
			long baseOffset = target.Position;
			int[] offset = new int[12];
			int[] length = new int[12];

			using (MemoryStream mem = new MemoryStream())
			{
				// generate the data block
				using (BinaryWriter writer = new BinaryWriter(mem))
				{
					for (int i = 0; i < 12; i++)
					{
						if (charts[i] != null)
						{
							baseOffset = mem.Position;
							offset[i] = (int)baseOffset;

							// I don't know if these are needed, but they are
							// parameters for note count you would typically find
							// in such a chart, so we will include them

							writer.Write((Int32)0);
							writer.Write((byte)0x10);	// notecount ID
							writer.Write((byte)0x00);	// player#
							writer.Write((Int16)charts[i].NoteCount(1));
							writer.Write((Int32)0);
							writer.Write((byte)0x10);	// notecount ID
							writer.Write((byte)0x01);	// player#
							writer.Write((Int16)charts[i].NoteCount(2));

							foreach (Entry entry in charts[i].Entries)
							{
								long num;
								long den;
								Int32 entryOffset = (Int32)(entry.LinearOffset * unit);
								byte entryType = 0xFF;
								byte entryParameter = (byte)(entry.Parameter & 0xFF);
								Int16 entryValue = 0;

								switch (entry.Type)
								{
									case EntryType.EndOfSong:
										entryType = 0x06;
										entryParameter = 0;
										entryValue = 0;
										break;
									case EntryType.Judgement:
										entryType = 0x08;
										entryParameter = (byte)entry.Parameter;
										entryValue = (Int16)entry.Value;
										break;
									case EntryType.Marker:
										if (entry.Player < 1)
										{
											entryType = 0x07;
											entryValue = (Int16)entry.Value;
										}
										else
										{
											entryType = (byte)(entry.Player - 1);
											entryValue = 0;
											entryParameter = (byte)entry.Column;
										}
										break;
									case EntryType.Measure:
										entryType = 0x0C;
										entryParameter = (byte)(entry.Player - 1);
										break;
									case EntryType.Sample:
										if (entry.Player > 0)
										{
											entryType = (byte)(entry.Player + 1);
											entryValue = (Int16)entry.Value;
											entryParameter = (byte)entry.Column;
										}
										break;
									case EntryType.Tempo:
										num = entry.Value.Numerator;
										den = entry.Value.Denominator;
										while ((num > 32767) || (den > 255))
										{
											num /= 2;
											den /= 2;
										}
										entryValue = (Int16)num;
										entryParameter = (byte)den;
										entryType = 0x04;
										break;
									default:
										continue;
								}
								if (entryType == 0xFF)
									continue;

								writer.Write(entryOffset);
								writer.Write(entryType);
								writer.Write(entryParameter);
								writer.Write(entryValue);
							}
							writer.Write((Int32)0x7FFFFFFF);
							writer.Write((Int32)0);
							length[i] = (int)(mem.Position - baseOffset);
						}
					}
					writer.Flush();
				}

				BinaryWriter outputWriter = new BinaryWriter(target);
				// write the offsets and data block
				for (int i = 0; i < 12; i++)
				{
					if (length[i] > 0)
					{
						outputWriter.Write((Int32)(offset[i] + 0x60));
						outputWriter.Write((Int32)length[i]);
					}
					else
					{
						outputWriter.Write((Int32)0);
						outputWriter.Write((Int32)0);
					}
				}
				outputWriter.Write(mem.ToArray());
				outputWriter.Flush();
			}

			target.Flush();
		}
	}
}
