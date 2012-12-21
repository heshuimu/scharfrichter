using ConvertHelper;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Convert1toBMS
{
	class Program
	{
		static int quantizeMeasure = 16;
		static long unitNumerator = 100;
		static long unitDenominator = 5994;

		static void Main(string[] args)
		{
			ConvertFunctions.BemaniToBMS(args, unitNumerator, unitDenominator, quantizeMeasure);
		}
	}
}
