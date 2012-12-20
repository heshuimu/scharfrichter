using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Charts
{
	public class Chart
	{
		private List<Entry> entries = new List<Entry>();
		private Dictionary<int, Fraction> lengths = new Dictionary<int, Fraction>();
		private Dictionary<string, string> tags = new Dictionary<string, string>();

		public Fraction DefaultBPM = new Fraction(0, 1);
		public Fraction TickRate = new Fraction(0, 1);

		// IIDX: F0, FA, FF, 03, 08, 12
		// 5key: F4, FC, FF, 03, 06, 0E

		public void AddJudgements()
		{
			int[] judgementValues = new int[] { 0xF0, 0xFA, 0xFF, 0x03, 0x08, 0x12 };
			int judgementCount = judgementValues.Length;
			int playerCount = Players;

			for (int j = 0; j < playerCount; j++)
			{
				for (int i = 0; i < judgementCount; i++)
				{
					Entry entry = new Entry();
					entry.Column = 0;
					entry.LinearOffset = new Fraction(0, 1);
					entry.MetricMeasure = 0;
					entry.MetricOffset = new Fraction(0, 1);
					entry.Parameter = i;
					entry.Player = j + 1;
					entry.Type = EntryType.Judgement;
					entry.Value = new Fraction(judgementValues[i], 1);
					entries.Add(entry);
				}
			}
		}

		public void AddMeasureLines()
		{
			int measureCount = -1;
			int playerCount = Players;

			// verify all required metric info is present
			foreach (Entry entry in entries)
				if (!entry.MetricOffsetInitialized)
					throw new Exception("Measure lines can't be added because at least one entry is missing Metric offset information.");

			// clear up existing ones
			RemoveMeasureLines();

			// find the highest measure index
			foreach (Entry entry in entries)
			{
				if (entry.MetricMeasure >= measureCount)
					measureCount = entry.MetricMeasure + 1;
			}

			// add measure lines for each measure
			for (int i = 0; i < measureCount; i++)
			{
				for (int j = 0; j < playerCount; j++)
				{
					Entry entry = new Entry();
					entry.Column = 0;
					entry.MetricMeasure = i;
					entry.MetricOffset = new Fraction(0, 1);
					entry.Player = j + 1;
					entry.Type = EntryType.Measure;
					entry.Value = new Fraction(0, 1);
					entries.Add(entry);
				}
			}

			// add end of song marker
			if (measureCount >= 0)
			{
				for (int j = 0; j < playerCount; j++)
				{
					Entry entry = new Entry();
					entry.Column = 0;
					entry.MetricMeasure = measureCount;
					entry.MetricOffset = new Fraction(0, 1);
					entry.Player = j + 1;
					entry.Type = EntryType.EndOfSong;
					entry.Value = new Fraction(0, 1);
					entries.Add(entry);
				}
			}
		}

		public void CalculateLinearOffsets()
		{
			// verify all required metric info is present
			foreach (Entry entry in entries)
				if (!entry.MetricOffsetInitialized)
					throw new Exception("Linear offsets can't be calculated because at least one entry is missing Metric offset information.");

			// make sure everything is sorted before we begin
			entries.Sort();

			// initialization
			Fraction baseLinear = new Fraction(0, 1);
			Fraction bpm = DefaultBPM;
			Fraction lastMetric = new Fraction(0, 1);
			Fraction length = new Fraction(0, 1);
			int measure = -1;
			Fraction measureRate = new Fraction(0, 1);
			Fraction rate = new Fraction(0, 1);
			
			// BPM into seconds per measure
			measureRate = Util.CalculateMeasureRate(bpm);

			foreach (Entry entry in entries)
			{
				if (entry.Type == EntryType.Measure)
				{
					baseLinear += rate;
					measure = entry.MetricMeasure;

					if (lengths.ContainsKey(measure))
					{
						length = lengths[measure];
						rate = length * measureRate;
					}
					else
					{
						length = new Fraction(1, 1);
						rate = measureRate;
					}
					lastMetric = entry.MetricOffset;
				}

				Fraction entryOffset = entry.MetricOffset;
				entryOffset -= lastMetric;
				entryOffset *= rate;
				entryOffset += baseLinear;
				entry.LinearOffset = entryOffset;

				if (entry.Type == EntryType.Tempo)
				{
					measureRate = Util.CalculateMeasureRate(bpm);
					rate = length * measureRate;
					lastMetric = entry.MetricOffset;
				}
			}
		}

		public void CalculateMetricOffsets()
		{
			// verify all required linear info is present
			foreach (Entry entry in entries)
			{
				if (!entry.LinearOffsetInitialized)
					throw new Exception("Metric offsets can't be calculated because at least one entry is missing Linear offset information.");
			}

			// make sure everything is sorted before we begin
			entries.Sort();

			// we keep going through the list until we hit a measure line. when we do,
			// we take a look at how long the measure is, how long a measure should be,
			// and determine its length from there.
			// if we encounter a tempo change mid-measure, that's where things get tricky.
			// in order to convert losslessly, we have to multiply both the pre-change and
			// post-change granularities, and the number will get exponentially larger as
			// more tempo changes are included (however with the built-in fraction reduction
			// this effect should be minimized).

			// initialization
			Fraction bpm = DefaultBPM;
			Fraction lastMeasureOffset = new Fraction(0, 1);
			Fraction lastTempoOffset = new Fraction(0, 1);
			Fraction length = new Fraction(0, 1);
			Dictionary<int, Fraction> lengthList = new Dictionary<int, Fraction>();
			int measure = 0;
			Fraction measureLength = new Fraction(0, 1);
			Fraction metricBase = new Fraction(0, 1);
			Fraction rate = Util.CalculateMeasureRate(bpm);
			bool tempoChanged = false;
			lengths.Clear();

			// get measure lengths for non-tempo-changing measures
			foreach (Entry entry in entries)
			{
				if (entry.Type == EntryType.Measure || entry.Type == EntryType.EndOfSong)
				{
					if (entry.LinearOffset != lastMeasureOffset)
					{
						if (!tempoChanged)
						{
							lengthList.Add(measure, entry.LinearOffset - lastMeasureOffset);
						}
						lastMeasureOffset = entry.LinearOffset;
						measure++;
						tempoChanged = false;
					}
				}
				else if (entry.Type == EntryType.Tempo)
				{
					if ((entry.LinearOffset - lastMeasureOffset).Numerator != 0)
					{
						tempoChanged = true;
					}
				}
			}

			measure = 0;
			lastMeasureOffset = new Fraction(0, 1);

			Fraction tickMeasureLength;
			if (lengthList.ContainsKey(0))
				tickMeasureLength = lengthList[0];
			else
				tickMeasureLength = new Fraction(0, 1);

			List<Entry> entryList = new List<Entry>();
			List<Entry> measureEntryList = new List<Entry>();

			// process
			foreach (Entry entry in entries)
			{
				if (entry.Type == EntryType.Measure || entry.Type == EntryType.Tempo || entry.Type == EntryType.EndOfSong)
				{
					Fraction measureDistance = ((entry.LinearOffset - lastTempoOffset) * TickRate) / rate;

					foreach (Entry tempoEntry in entryList)
					{
						tempoEntry.MetricOffset = measureLength + ((tempoEntry.LinearOffset - lastTempoOffset) / (entry.LinearOffset - lastTempoOffset));
						tempoEntry.MetricMeasure = measure;
						while ((double)tempoEntry.MetricOffset >= 1)
						{
							Fraction offs = tempoEntry.MetricOffset;
							tempoEntry.MetricMeasure++;
							offs.Numerator -= offs.Denominator;
							tempoEntry.MetricOffset = offs;
							measureEntryList.Add(tempoEntry);
						}
					}

					measureLength += measureDistance;

					if (entry.Type == EntryType.Measure || entry.Type == EntryType.EndOfSong)
					{
						if (tickMeasureLength.Numerator == 0)
						{
							foreach (Entry measureEntry in entryList)
							{
								if (measureEntry.MetricMeasure == measure)
								{
									Fraction temp = measureEntry.MetricOffset;
									temp /= measureLength;
									measureEntry.MetricOffset = temp;
									while ((double)measureEntry.MetricOffset >= 1)
									{
										Fraction offs = measureEntry.MetricOffset;
										measureEntry.MetricMeasure++;
										offs.Numerator -= offs.Denominator;
										measureEntry.MetricOffset = offs;
									}
								}
							}
						}

						foreach (Entry measureEntry in measureEntryList)
						{
							Fraction temp = measureEntry.MetricOffset;
							temp /= measureLength;
							measureEntry.MetricOffset = temp;
						}

						if (entry.LinearOffset != lastMeasureOffset)
						{
							MeasureLengths[measure] = measureLength;
							measure++;
							lastMeasureOffset = entry.LinearOffset;
							measureLength = new Fraction(0, 1);
						}
						entry.MetricOffset = new Fraction(0, 1);
						entry.MetricMeasure = measure;
					}
					else if (entry.Type == EntryType.Tempo)
					{
						bpm = entry.Value;
						rate = Util.CalculateMeasureRate(bpm);
					}
					lastTempoOffset = entry.LinearOffset;

					if (lengthList.ContainsKey(measure))
						tickMeasureLength = lengthList[measure];
					else
						tickMeasureLength = new Fraction(0, 1);

					entryList.Clear();
				}

				if (tickMeasureLength.Numerator > 0)
				{
					entry.MetricOffset = (entry.LinearOffset - lastTempoOffset) / tickMeasureLength;
					entry.MetricMeasure = measure;
					while ((double)entry.MetricOffset >= 1)
					{
						Fraction offs = entry.MetricOffset;
						entry.MetricMeasure++;
						offs.Numerator -= offs.Denominator;
						entry.MetricOffset = offs;
					}
				}
				else
				{
					entryList.Add(entry);
				}

				
			}
		}

		public void ClearUsed()
		{
			foreach (Entry entry in entries)
				entry.Used = false;
		}

		public List<Entry> Entries
		{
			get
			{
				return entries;
			}
		}

		public int Measures
		{
			get
			{
				int measureCount = -1;
				// find the highest measure index
				foreach (Entry entry in entries)
				{
					if (entry.MetricMeasure >= measureCount)
						measureCount = entry.MetricMeasure + 1;
				}
				return measureCount + 1;
			}
		}

		public Dictionary<int, Fraction> MeasureLengths
		{
			get
			{
				return lengths;
			}
		}

		public int NoteCount(int player)
		{
			int result = 0;

			foreach (Entry entry in entries)
				if (entry.Type == EntryType.Marker && entry.Player == player)
					result++;

			return result;
		}

		public int Players
		{
			get
			{
				int result = 0;
				foreach (Entry entry in entries)
				{
					if ((entry.Type == EntryType.Marker || entry.Type == EntryType.Sample) && (entry.Player > result))
						result = entry.Player;
				}
				return result;
			}
		}

		// Because so many people use BMSE and BMSE doesn't like measure lengths that are not a multiple of 1/64,
		// this is here to please the people that still use it. (I hate you guys.)
		public void QuantizeMeasureLengths(int quantizeValue)
		{
			// verify all required metric info is present
			foreach (Entry entry in entries)
				if (!entry.MetricOffsetInitialized)
					throw new Exception("Measure lengths can't be quantized because at least one entry is missing Metric offset information.");

			double quantizationFloat = quantizeValue;
			int measureCount = Measures;
			Fraction lengthBefore = Fraction.Rationalize(TotalMeasureLength);

			for (int i = 0; i < measureCount; i++)
			{
				if (lengths.ContainsKey(i))
				{
					if (lengths[i].Denominator != quantizeValue)
						lengths[i] = new Fraction((long)(Math.Round((double)lengths[i] * quantizationFloat)), quantizeValue);
				}
			}

			Fraction lengthAfter = Fraction.Rationalize(TotalMeasureLength);
			Fraction ratio = lengthAfter / lengthBefore;

			// since we adjusted measure lengths, we also need to adjust BPMs
			foreach (Entry entry in entries)
			{
				if (entry.Type == EntryType.Tempo)
				{
					entry.Value *= ratio;
				}
			}

			DefaultBPM *= ratio;

			// we also need to regenerate linear offsets
			CalculateLinearOffsets();
		}

		public void QuantizeNoteOffsets(int quantizeValue)
		{
			// verify all required metric info is present
			foreach (Entry entry in entries)
				if (!entry.MetricOffsetInitialized)
					throw new Exception("Metric note offsets can't be quantized because at least one entry is missing Metric offset information.");

			int measure = 0;
			Fraction lastMeasure = new Fraction(0, 1);
			long quantize = quantizeValue;

			foreach (Entry entry in entries)
			{
				if (entry.Type == EntryType.Measure)
				{
					if (entry.MetricOffset != lastMeasure)
						measure++;

					if (lengths.ContainsKey(measure))
						quantize = quantizeValue * (long)(Math.Round((double)lengths[measure]));
					else
						quantize = quantizeValue;
				}

				if (quantize == 0)
					quantize = 192;

				entry.MetricOffset = Fraction.Quantize(entry.MetricOffset, quantize);
			}
		}

		public void RemoveJudgements()
		{
			foreach (Entry entry in entries)
			{
				if (entry.Type == EntryType.Judgement)
					entry.Type = EntryType.Invalid;
			}
		}

		public void RemoveMeasureLines()
		{
			foreach (Entry entry in entries)
			{
				if (entry.Type == EntryType.Measure || entry.Type == EntryType.EndOfSong)
					entry.Type = EntryType.Invalid;
			}
		}

		public Dictionary<string, string> Tags
		{
			get
			{
				return tags;
			}
		}

		public double TotalMeasureLength
		{
			get
			{
				double result = 0;
				int measureCount = Measures;
				for (int i = 0; i < measureCount; i++)
				{
					if (lengths.ContainsKey(i))
					{
						result += (double)lengths[i];
					}
					else
					{
						result += 1;
					}
				}
				return result;
			}
		}
	}

	public class Entry : IComparable<Entry>
	{
		private Fraction linearOffset;
		private Fraction metricOffset;
		private Fraction value;

		public bool LinearOffsetInitialized;
		public int MetricMeasure;
		public bool MetricOffsetInitialized;
		public bool ValueInitialized;

		public int Column;
		public int Parameter;
		public int Player;
		public EntryType Type;
		public bool Used;

		public int CompareTo(Entry other)
		{
			if (other.MetricOffsetInitialized && this.MetricOffsetInitialized)
			{
				if (other.MetricMeasure > this.MetricMeasure)
					return -1;
				if (other.MetricMeasure < this.MetricMeasure)
					return 1;

				double myFloat = (double)metricOffset;
				double otherFloat = (double)other.metricOffset;

				if (otherFloat > myFloat)
					return -1;
				if (otherFloat < myFloat)
					return 1;
			}
			else if (other.LinearOffsetInitialized && this.LinearOffsetInitialized)
			{
				double myFloat = (double)linearOffset;
				double otherFloat = (double)other.linearOffset;

				if (otherFloat > myFloat)
					return -1;
				if (otherFloat < myFloat)
					return 1;
			}

			if (other.Player > this.Player)
				return -1;
			if (other.Player < this.Player)
				return 1;

			if (other.Column > this.Column)
				return -1;
			if (other.Column < this.Column)
				return 1;

			if (other.Parameter > this.Parameter)
				return -1;
			if (other.Parameter < this.Parameter)
				return 1;

			// these must come at the beginning
			if (this.Type == EntryType.Measure && other.Type != EntryType.Measure)
			{
				return -1;
			}
			if (this.Type != EntryType.Measure && other.Type == EntryType.Measure)
			{
				return 1;
			}
			if (this.Type == EntryType.Tempo && other.Type != EntryType.Tempo)
			{
				return -1;
			}
			if (this.Type != EntryType.Tempo && other.Type == EntryType.Tempo)
			{
				return 1;
			}
			if (this.Type == EntryType.Sample && other.Type != EntryType.Sample)
			{
				return -1;
			}
			if (this.Type != EntryType.Sample && other.Type == EntryType.Sample)
			{
				return 1;
			}

			// these must come at the end
			if (this.Type == EntryType.EndOfSong && other.Type != EntryType.EndOfSong)
			{
				return 1;
			}
			if (this.Type != EntryType.EndOfSong && other.Type == EntryType.EndOfSong)
			{
				return -1;
			}

			// at this point, order does not matter
			return 0;
		}

		public override string ToString()
		{
			// for debug purposes only
			return (Type.ToString() + ": P" + Player.ToString() + ", C" + Column.ToString());
		}

		public Fraction LinearOffset
		{
			get
			{
				return linearOffset;
			}
			set
			{
				linearOffset = value;
				LinearOffsetInitialized = true;
			}
		}

		public Fraction MetricOffset
		{
			get
			{
				return metricOffset;
			}
			set
			{
				metricOffset = value;
				MetricOffsetInitialized = true;
			}
		}

		public Fraction Value
		{
			get
			{
				return this.value;
			}
			set
			{
				this.value = value;
				ValueInitialized = true;
			}
		}
	}

	public enum EntryType
	{
		Invalid,
		Marker,
		Sample,
		Freeze,
		Tempo,
		Measure,
		Mine,
		Event,
		Judgement,
		BGA,
		EndOfSong
	}
}
