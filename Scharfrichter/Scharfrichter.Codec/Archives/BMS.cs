using Scharfrichter.Codec.Charts;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Archives
{
	// Be-Music Source File.

	public class BMS : Archive
	{
		private enum ValueCoding
		{
			BME,
			Hex,
			Decimal,
			BPMTable
		}

		private const string alphabetBME = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		private const string alphabetHex = "0123456789ABCDEF";
		private const string alphabetDecimal = "0123456789";

		private Chart chart = null;

		public override Chart[] Charts
		{
			get
			{
				if (chart != null)
					return new Chart[] { chart };
				else
					return base.Charts;
			}
			set
			{
				if (value != null && value.Length > 0)
					chart = value[0];
			}
		}

		public override int ChartCount
		{
			get
			{
				return (chart != null) ? 1 : 0;
			}
		}

		static public BMS Read(Stream source)
		{
			List<KeyValuePair<string, string>> noteTags = new List<KeyValuePair<string, string>>();

			BMS result = new BMS();
			Chart chart = new Chart();

			using (StreamReader reader = new StreamReader(source))
			{
				while (!reader.EndOfStream)
				{
					string currentLine = reader.ReadLine();

					if (currentLine.StartsWith("#"))
					{
						currentLine = currentLine.Substring(1);
						currentLine = currentLine.Replace("\t", " ");

						if (currentLine.Contains(" "))
						{
							int separatorOffset = currentLine.IndexOf(" ");
							string val = currentLine.Substring(separatorOffset + 1).Trim();
							string tag = currentLine.Substring(0, separatorOffset).Trim().ToUpper();
							if (tag != "")
								chart.Tags[tag] = val;
						}
						else if (currentLine.Contains(":"))
						{
							int separatorOffset = currentLine.IndexOf(":");
							string val = currentLine.Substring(separatorOffset + 1).Trim();
							string tag = currentLine.Substring(0, separatorOffset).Trim().ToUpper();
							if (tag != "")
								noteTags.Add(new KeyValuePair<string, string>(tag, val));
						}
					}
				}
			}

			if (chart.Tags.ContainsKey("BPM"))
			{
				chart.DefaultBPM = Fraction.Rationalize(Convert.ToDouble(chart.Tags["BPM"]));
			}

			foreach (KeyValuePair<string, string> tag in noteTags)
			{
				if (tag.Key.Length == 5)
				{
					string measure = tag.Key.Substring(0, 3);
					string lane = tag.Key.Substring(3, 2);
					ValueCoding coding = ValueCoding.BME;

					int currentColumn;
					int currentMeasure;
					int currentPlayer;
					EntryType currentType;

					if (lane == "02")
					{
						chart.MeasureLengths[Convert.ToInt32(measure)] = Fraction.Rationalize(Convert.ToDouble(tag.Value));
					}
					else
					{
						currentMeasure = Convert.ToInt32(measure);
						currentColumn = 0;

						switch (lane)
						{
							case "01": currentPlayer = 0; currentType = EntryType.Marker; currentColumn = 0; break;
							case "03": currentPlayer = 0; currentType = EntryType.Tempo; coding = ValueCoding.Hex; break;
							case "04": currentPlayer = 0; currentType = EntryType.BGA; currentColumn = 0; break;
							case "05": currentPlayer = 0; currentType = EntryType.BGA; currentColumn = 1; break;
							case "06": currentPlayer = 0; currentType = EntryType.BGA; currentColumn = 2; break;
							case "07": currentPlayer = 0; currentType = EntryType.BGA; currentColumn = 3; break;
							case "08": currentPlayer = 0; currentType = EntryType.Tempo; coding = ValueCoding.BPMTable; break;
							case "11": currentPlayer = 1; currentType = EntryType.Marker; currentColumn = 0; break;
							case "12": currentPlayer = 1; currentType = EntryType.Marker; currentColumn = 1; break;
							case "13": currentPlayer = 1; currentType = EntryType.Marker; currentColumn = 2; break;
							case "14": currentPlayer = 1; currentType = EntryType.Marker; currentColumn = 3; break;
							case "15": currentPlayer = 1; currentType = EntryType.Marker; currentColumn = 4; break;
							case "16": currentPlayer = 1; currentType = EntryType.Marker; currentColumn = 5; break;
							case "17": currentPlayer = 1; currentType = EntryType.Marker; currentColumn = 8; break;
							case "18": currentPlayer = 1; currentType = EntryType.Marker; currentColumn = 6; break;
							case "19": currentPlayer = 1; currentType = EntryType.Marker; currentColumn = 7; break;
							case "21": currentPlayer = 2; currentType = EntryType.Marker; currentColumn = 0; break;
							case "22": currentPlayer = 2; currentType = EntryType.Marker; currentColumn = 1; break;
							case "23": currentPlayer = 2; currentType = EntryType.Marker; currentColumn = 2; break;
							case "24": currentPlayer = 2; currentType = EntryType.Marker; currentColumn = 3; break;
							case "25": currentPlayer = 2; currentType = EntryType.Marker; currentColumn = 4; break;
							case "26": currentPlayer = 2; currentType = EntryType.Marker; currentColumn = 5; break;
							case "27": currentPlayer = 2; currentType = EntryType.Marker; currentColumn = 8; break;
							case "28": currentPlayer = 2; currentType = EntryType.Marker; currentColumn = 6; break;
							case "29": currentPlayer = 2; currentType = EntryType.Marker; currentColumn = 7; break;
							case "31": currentPlayer = 1; currentType = EntryType.Sample; currentColumn = 0; break;
							case "32": currentPlayer = 1; currentType = EntryType.Sample; currentColumn = 1; break;
							case "33": currentPlayer = 1; currentType = EntryType.Sample; currentColumn = 2; break;
							case "34": currentPlayer = 1; currentType = EntryType.Sample; currentColumn = 3; break;
							case "35": currentPlayer = 1; currentType = EntryType.Sample; currentColumn = 4; break;
							case "36": currentPlayer = 1; currentType = EntryType.Sample; currentColumn = 5; break;
							case "37": currentPlayer = 1; currentType = EntryType.Sample; currentColumn = 8; break;
							case "38": currentPlayer = 1; currentType = EntryType.Sample; currentColumn = 6; break;
							case "39": currentPlayer = 1; currentType = EntryType.Sample; currentColumn = 7; break;
							case "41": currentPlayer = 2; currentType = EntryType.Sample; currentColumn = 0; break;
							case "42": currentPlayer = 2; currentType = EntryType.Sample; currentColumn = 1; break;
							case "43": currentPlayer = 2; currentType = EntryType.Sample; currentColumn = 2; break;
							case "44": currentPlayer = 2; currentType = EntryType.Sample; currentColumn = 3; break;
							case "45": currentPlayer = 2; currentType = EntryType.Sample; currentColumn = 4; break;
							case "46": currentPlayer = 2; currentType = EntryType.Sample; currentColumn = 5; break;
							case "47": currentPlayer = 2; currentType = EntryType.Sample; currentColumn = 8; break;
							case "48": currentPlayer = 2; currentType = EntryType.Sample; currentColumn = 6; break;
							case "49": currentPlayer = 2; currentType = EntryType.Sample; currentColumn = 7; break;
							default: chart.Tags[tag.Key + ":" + tag.Value] = ""; continue; // a little hack to preserve unknown lines
						}
						
						// determine the alphabet used to decode this line
						string alphabet;
						int alphabetLength;
						switch (coding)
						{
							case ValueCoding.Hex: alphabet = alphabetHex; break;
							case ValueCoding.Decimal: alphabet = alphabetDecimal; break;
							default: alphabet = alphabetBME; break;
						}
						alphabetLength = alphabet.Length;

						// decode the line
						int valueLength = (tag.Value.Length | 1) ^ 1; // make an even number
						for (int i = 0; i < valueLength; i += 2)
						{
							string pair = tag.Value.Substring(i, 2);
							int index0 = alphabet.IndexOf(pair.Substring(0, 1));
							int index1 = alphabet.IndexOf(pair.Substring(1, 1));
							int val = 0;

							if (index0 > 0)
								val += (index0 * alphabetLength);
							if (index1 > 0)
								val += index1;

							if (val > 0)
							{
								Entry entry = new Entry();
								entry.Column = currentColumn;
								entry.Player = currentPlayer;
								entry.MetricMeasure = currentMeasure;
								entry.Type = currentType;
								entry.MetricOffset = new Fraction(i, valueLength);

								if (coding == ValueCoding.BPMTable)
								{
									if (chart.Tags.ContainsKey("BPM" + pair))
									{
										string bpmValue = chart.Tags["BPM" + pair];
										entry.Value = Fraction.Rationalize(Convert.ToDouble(bpmValue));
									}
									else
									{
										entry.Type = EntryType.Invalid;
									}
								}
								else
								{
									entry.Value = new Fraction(val, 1);
								}

								if (entry.Type != EntryType.Invalid)
									chart.Entries.Add(entry);
							}
						}
					}
				}
			}

			chart.AddMeasureLines();
			chart.AddJudgements();
			chart.CalculateLinearOffsets();

			result.chart = chart;
			return result;
		}

		public void Write(Stream target)
		{
		}
	}
}
