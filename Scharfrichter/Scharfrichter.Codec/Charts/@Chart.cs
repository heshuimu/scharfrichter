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
		private const int FallbackDenominator = 768;

		private int digitalDenominator;
		private bool digitalInitialized;
		private int digitalNumerator;
		private bool metricInitialized;
		private double metricValue;

		public int Denominator
		{
			get
			{
				if (!digitalInitialized && metricInitialized)
				{
					digitalDenominator = FallbackDenominator;
					digitalInitialized = true;
				}
				return digitalDenominator;
			}
			set
			{
				metricInitialized = false;
				digitalInitialized = true;
				digitalDenominator = value;
			}
		}

		public double Metric
		{
			get
			{
				if (!metricInitialized && digitalInitialized)
				{
					metricValue = (double)digitalNumerator / (double)digitalDenominator;
					metricInitialized = true;
				}
				return metricValue;
			}
			set
			{
				digitalInitialized = false;
				metricInitialized = true;
				metricValue = value;
			}
		}

		public int Numerator
		{
			get
			{
				return digitalNumerator;
			}
			set
			{
				metricInitialized = false;
				digitalNumerator = value;
			}
		}
	}
}
