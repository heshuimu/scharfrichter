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

	public struct Entry
	{
		private const int FallbackDenominator = 2 * 3 * 5 * 7 * 9 * 11 * 13 * 17;

		private long digitalOffsetDenominator;
		private long digitalOffsetNumerator;
		private double floatValue;
		private double metricOffsetValue;
		private long valueDenominator;
		private long valueNumerator;

		public bool DigitalOffsetInitialized;
		public bool FloatInitialized;
		public bool MetricOffsetInitialized;
		public bool ValueInitialized;

		public int Column;
		public int Measure;
		public int Player;
		public EntryType Type;

		public override string ToString()
		{
			return (Type.ToString() + ": P" + Player.ToString() + ", C" + Column.ToString() + ", O" + Measure.ToString() + "-" + digitalOffsetNumerator.ToString() + "/" + digitalOffsetDenominator.ToString() + "(" + metricOffsetValue.ToString() + ")");
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

		public double OffsetMetric
		{
			get
			{
				return metricOffsetValue;
			}
			set
			{
				MetricOffsetInitialized = true;
				metricOffsetValue = value;
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

		public double ValueFloat
		{
			get
			{
				return floatValue;
			}
			set
			{
				FloatInitialized = true;
				floatValue = value;
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
		BGA
	}
}
