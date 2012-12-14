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

		public Fraction DefaultBPM;

		// for now we are going to default to IIDX timing

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

		public void CalculateDigitalOffsets()
		{
			// verify all required metric info is present
			foreach (Entry entry in entries)
				if (!entry.MetricOffsetInitialized)
					throw new Exception("Linear offsets can't be calculated because at least one entry is missing Metric offset information.");

			// reset all linear offset information
			foreach (Entry entry in entries)
				entry.LinearOffsetInitialized = false;

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
			// and determine its length from there. after that, we take all the entries
			// we passed and assign them a position within the measure in the metric offset.
			// if we encounter a tempo change mid-measure, that's where things get tricky.
			// in order to convert losslessly, we have to multiply both the pre-change and
			// post-change granularities, and the number will get exponentially larger as
			// more tempo changes are included (however with the built-in fraction reduction
			// this effect should be minimized).
			//
			// we determine how long the measure line should be by converting BPM into
			// seconds per measure.

			// initialization
			Fraction bpm = DefaultBPM;
			Fraction lastMeasureOffset = new Fraction(0, 1);
			Fraction lastTempoOffset = new Fraction(0, 1);
			Fraction length = new Fraction(0, 1);
			int measure = 0;
			Fraction measureLength = new Fraction(0, 1);
			Fraction metricBase = new Fraction(0, 1);
			Fraction rate = Util.CalculateMeasureRate(bpm);

			lengths.Clear();

			// process
			foreach (Entry entry in entries)
			{
				if (entry.Type == EntryType.Measure || entry.Type == EntryType.Tempo)
				{
					measureLength += (entry.LinearOffset - lastTempoOffset) / rate;

					if (entry.Type == EntryType.Measure)
					{
						if (entry.LinearOffset != lastMeasureOffset)
						{
							MeasureLengths[measure] = measureLength;
							measure++;
							lastMeasureOffset = entry.LinearOffset;
							measureLength = new Fraction(0, 1);
						}
					}
					else if (entry.Type == EntryType.Tempo)
					{
						bpm = entry.Value;
						rate = Util.CalculateMeasureRate(bpm);
					}
					lastTempoOffset = entry.LinearOffset;
				}

				entry.MetricOffset = (entry.LinearOffset - lastTempoOffset) / rate;
				entry.MetricMeasure = measure;
			}
		}

		public List<Entry> Entries
		{
			get
			{
				return entries;
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
