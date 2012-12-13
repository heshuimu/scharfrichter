using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Charts
{
	public class Chart
	{
		private Dictionary<string, string> tags = new Dictionary<string,string>();

		public Dictionary<string, string> Tags
		{
			get
			{
				return tags;
			}
		}
	}

	// this structure converts between both metric and
	// digital seamlessly and behind the scenes.

	public struct Entry
	{
		private const int FallbackDenominator = 2 * 3 * 5 * 7 * 9 * 11 * 13 * 17;

		private int digitalOffsetDenominator;
		private bool digitalOffsetInitialized;
		private int digitalOffsetNumerator;
		private bool floatInitialized;
		private double floatValue;
		private bool metricOffsetInitialized;
		private double metricOffsetValue;
		private int valueDenominator;
		private bool valueInitialized;
		private int valueNumerator;

		public int OffsetDenominator
		{
			get
			{
				if (!digitalOffsetInitialized && metricOffsetInitialized)
				{
					digitalOffsetDenominator = FallbackDenominator;
					digitalOffsetNumerator = (int)((double)FallbackDenominator * metricOffsetValue);
					digitalOffsetInitialized = true;
				}
				return digitalOffsetDenominator;
			}
			set
			{
				metricOffsetInitialized = false;
				digitalOffsetInitialized = true;
				digitalOffsetDenominator = value;
			}
		}

		public double OffsetMetric
		{
			get
			{
				if (!metricOffsetInitialized && digitalOffsetInitialized)
				{
					metricOffsetValue = (double)digitalOffsetNumerator / (double)digitalOffsetDenominator;
					metricOffsetInitialized = true;
				}
				return metricOffsetValue;
			}
			set
			{
				digitalOffsetInitialized = false;
				metricOffsetInitialized = true;
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
				metricOffsetInitialized = false;
				digitalOffsetNumerator = value;
			}
		}

		public int ValueDenominator
		{
			get
			{
				if (!valueInitialized && floatInitialized)
				{
					valueDenominator = FallbackDenominator;
					valueNumerator = (int)((double)FallbackDenominator * floatValue);
					valueInitialized = true;
				}
				return valueDenominator;
			}
			set
			{
				floatInitialized = false;
				valueInitialized = true;
				valueDenominator = value;
			}
		}

		public double ValueFloat
		{
			get
			{
				if (!floatInitialized && valueInitialized)
				{
					floatValue = (double)valueNumerator / (double)valueDenominator;
					floatInitialized = true;
				}
				return floatValue;
			}
			set
			{
				valueInitialized = false;
				floatInitialized = true;
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
				floatInitialized = false;
				valueNumerator = value;
			}
		}

	}
}
