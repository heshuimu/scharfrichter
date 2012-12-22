using ConvertHelper;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BemaniToBMSTroopers
{
	class Program
	{
		static int quantizeMeasure = 16;
		static long unitNumerator = 1;
		static long unitDenominator = 1000;

		static void Main(string[] args)
		{
			if (System.Diagnostics.Debugger.IsAttached && args.Length == 0)
			{
				Console.WriteLine("Debugger attached. Input file name:");
				args = new string[] { Console.ReadLine() };
			}
			ConvertFunctions.BemaniToBMS(args, unitNumerator, unitDenominator, quantizeMeasure);
		}
	}
}
