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

		private int digitalOffsetDenominator;
		private int digitalOffsetNumerator;
		private double floatValue;
		private double metricOffsetValue;
		private int valueDenominator;
		private int valueNumerator;

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

		public int OffsetDenominator
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

		public int OffsetNumerator
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

		public int ValueDenominator
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

		public int ValueNumerator
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
