using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec
{
	static class Util
	{
		public static void Rationalize(double input, out long numerator, out long denominator)
		{
			denominator = 1;
			while (input != Math.Round(input))
			{
				input *= 10;
				denominator *= 10;
			}
			numerator = (long)input;
		}
	}
}
