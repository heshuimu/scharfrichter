using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Charts
{
	public class Chart
	{
		private List<Entry> entries = new List<Entry>();
		private Dictionary<string, string> tags = new Dictionary<string, string>();

		public long DefaultBPMDenominator;
		public long DefaultBPMNumerator;

		public void AddMeasureLines()
		{
			int measureCount = -1;

			// find the highest measure index
			foreach (Entry entry in entries)
			{
				if (entry.Measure >= measureCount)
					measureCount = entry.Measure + 1;
			}

			// add measure lines for each measure
			for (int i = 0; i < measureCount; i++)
			{
				Entry entry = new Entry();
				entry.Column = 0;
				entry.Measure = i;
				entry.OffsetDenominator = 1;
				entry.OffsetNumerator = 0;
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
				entry.Measure = measureCount;
				entry.OffsetDenominator = 1;
				entry.OffsetNumerator = 0;
				entry.MetricDenominator = 1;
				entry.MetricNumerator = 0;
				entry.Player = 0;
				entry.Type = EntryType.EndOfSong;
				entry.ValueDenominator = 1;
				entry.ValueNumerator = 0;
				entries.Add(entry);
			}
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
				Util.Rationalize(value, out DefaultBPMNumerator, out DefaultBPMDenominator);
			}
		}

		public List<Entry> Entries
		{
			get
			{
				return entries;
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

	public struct Entry : IComparable<Entry>
	{
		private const int FallbackDenominator = 2 * 3 * 5 * 7 * 9 * 11 * 13 * 17;

		private long digitalOffsetDenominator;
		private long digitalOffsetNumerator;
		private long metricOffsetDenominator;
		private long metricOffsetNumerator;
		private long valueDenominator;
		private long valueNumerator;

		public bool DigitalOffsetInitialized;
		public bool MetricOffsetInitialized;
		public bool ValueInitialized;

		public int Column;
		public int Measure;
		public int Player;
		public EntryType Type;

		public int CompareTo(Entry other)
		{
			if (other.Measure > this.Measure)
				return -1;
			if (other.Measure < this.Measure)
				return 1;

			if (other.MetricOffsetInitialized && this.MetricOffsetInitialized)
			{
				double myFloat = (double)MetricNumerator / (double)MetricDenominator;
				double otherFloat = (double)other.MetricNumerator / (double)other.MetricDenominator;

				if (otherFloat > myFloat)
					return -1;
				if (otherFloat < myFloat)
					return 1;
			}
			else if (other.DigitalOffsetInitialized && this.DigitalOffsetInitialized)
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
				return digitalOffsetDenominator;
			}
			set
			{
				DigitalOffsetInitialized = true;
				digitalOffsetDenominator = value;
			}
		}

		public long OffsetNumerator
		{
			get
			{
				return digitalOffsetNumerator;
			}
			set
			{
				digitalOffsetNumerator = value;
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
