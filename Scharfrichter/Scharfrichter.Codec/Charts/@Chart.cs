using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Charts
{
	public class Chart
	{
		private List<Entry> entries = new List<Entry>();
		private Dictionary<int, double> lengths = new Dictionary<int, double>();
		private Dictionary<string, string> tags = new Dictionary<string, string>();

		public long DefaultBPMDenominator;
		public long DefaultBPMNumerator;

		public void AddMeasureLines()
		{
			int measureCount = -1;

			// find the highest measure index
			foreach (Entry entry in entries)
			{
				if (entry.MetricMeasure >= measureCount)
					measureCount = entry.MetricMeasure + 1;
			}

			// add measure lines for each measure
			for (int i = 0; i < measureCount; i++)
			{
				Entry entry = new Entry();
				entry.Column = 0;
				entry.MetricMeasure = i;
				entry.MetricDenominator = 1;
				entry.MetricNumerator = 0;
				entry.Player = 0;
				entry.Type = EntryType.Measure;
				entry.ValueDenominator = 1;
				entry.ValueNumerator = 0;
				entries.Add(entry);
			}

			// add end of song marker
			if (measureCount >= 0)
			{
				Entry entry = new Entry();
				entry.Column = 0;
				entry.MetricMeasure = measureCount;
				entry.MetricDenominator = 1;
				entry.MetricNumerator = 0;
				entry.Player = 0;
				entry.Type = EntryType.EndOfSong;
				entry.ValueDenominator = 1;
				entry.ValueNumerator = 0;
				entries.Add(entry);
			}
		}

		// *HUGE* TODO: consolidate all these numerator/denominator things into one struct

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
			Fraction bpm = new Fraction(DefaultBPMNumerator, DefaultBPMDenominator);
			double lengthFloat = 1;
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
						length = Fraction.Rationalize(lengths[measure]);
						rate = length * measureRate;
					}
					else
					{
						length = new Fraction(1, 1);
						rate = measureRate;
					}
					lastMetric.Denominator = entry.MetricDenominator;
					lastMetric.Numerator = entry.MetricNumerator;
				}

				Fraction entryOffset = new Fraction(entry.MetricNumerator, entry.MetricDenominator);
				entryOffset -= lastMetric;
				entryOffset *= rate;
				entryOffset += baseLinear;
				entry.OffsetNumerator = entryOffset.Numerator;
				entry.OffsetDenominator = entryOffset.Denominator;

				if (entry.Type == EntryType.Tempo)
				{
					measureRate = Util.CalculateMeasureRate(bpm);
					rate = length * measureRate;
					lastMetric.Numerator = entry.MetricNumerator;
					lastMetric.Denominator = entry.MetricDenominator;
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

		}

		public double DefaultBPM
		{
			get
			{
				if (DefaultBPMDenominator > 0)
					return (DefaultBPMNumerator / DefaultBPMDenominator);
				return 0;
			}
			set
			{
				Fraction bpmFrac = Fraction.Rationalize(value);
				DefaultBPMDenominator = bpmFrac.Denominator;
				DefaultBPMNumerator = bpmFrac.Numerator;
			}
		}

		public List<Entry> Entries
		{
			get
			{
				return entries;
			}
		}

		public Dictionary<int, double> MeasureLengths
		{
			get
			{
				return lengths;
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
		private const int FallbackDenominator = 2 * 3 * 5 * 7 * 9 * 11 * 13 * 17;

		private long linearOffsetDenominator;
		private long linearOffsetNumerator;
		private long metricOffsetDenominator;
		private long metricOffsetNumerator;
		private long valueDenominator;
		private long valueNumerator;

		public bool LinearOffsetInitialized;
		public int MetricMeasure;
		public bool MetricOffsetInitialized;
		public bool ValueInitialized;

		public int Column;
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

				double myFloat = (double)MetricNumerator / (double)MetricDenominator;
				double otherFloat = (double)other.MetricNumerator / (double)other.MetricDenominator;

				if (otherFloat > myFloat)
					return -1;
				if (otherFloat < myFloat)
					return 1;
			}
			else if (other.LinearOffsetInitialized && this.LinearOffsetInitialized)
			{
				double myFloat = (double)OffsetNumerator / (double)OffsetDenominator;
				double otherFloat = (double)other.OffsetNumerator / (double)other.OffsetDenominator;

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

		public long MetricDenominator
		{
			get
			{
				return metricOffsetDenominator;
			}
			set
			{
				MetricOffsetInitialized = true;
				metricOffsetDenominator = value;
			}
		}

		public long MetricNumerator
		{
			get
			{
				return metricOffsetNumerator;
			}
			set
			{
				metricOffsetNumerator = value;
			}
		}

		public long OffsetDenominator
		{
			get
			{
				return linearOffsetDenominator;
			}
			set
			{
				LinearOffsetInitialized = true;
				linearOffsetDenominator = value;
			}
		}

		public long OffsetNumerator
		{
			get
			{
				return linearOffsetNumerator;
			}
			set
			{
				linearOffsetNumerator = value;
			}
		}

		public long ValueDenominator
		{
			get
			{
				return valueDenominator;
			}
			set
			{
				ValueInitialized = true;
				valueDenominator = value;
			}
		}

		public long ValueNumerator
		{
			get
			{
				return valueNumerator;
			}
			set
			{
				valueNumerator = value;
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
