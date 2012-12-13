using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec
{
	static class Util
	{
		private static long[] Primes = new long[]
		{
			2, 3, 5, 7, 9, 
			11, 13, 17, 19, 23, 
			29, 31, 37, 41, 43,
			47, 53, 59, 61, 67,
			71, 73, 79, 83, 89,
			97, 101, 103, 107, 109,
			113, 127, 131, 137, 139,
			149, 151, 157, 163, 167,
			173, 179, 181, 191, 193,
			197, 199, 211, 223, 227,
			229, 233, 239, 241, 251,
			257, 263, 269, 271, 277,
			281, 283, 293, 307, 311,
			313, 317, 331, 337, 347,
			349, 353, 359, 367, 373,
			379, 383, 389, 397, 401,
			409, 419, 421, 431, 433,
			439, 443, 449, 457, 461,
			463, 467, 479, 487, 491,
			499, 503, 509, 521, 523,
			541, 547, 557, 563, 569,
			571, 577, 587, 593, 599,
			601, 607, 613, 617, 619,
			631, 641, 643, 647, 653,
			659, 661, 673, 677, 683,
			691, 701, 709, 719, 727,
			733, 739, 743, 751, 757,
			761, 769, 773, 787, 797,
			809, 811, 821, 823, 827,
			829, 839, 853, 857, 859,
			863, 877, 881, 883, 887,
			907, 911, 919, 929, 937,
			941, 947, 953, 967, 971,
			977, 983, 991, 997, 1009
		};
		private static int PrimeCount = Primes.Length;

		public static void Approximate(ref long numerator, ref long denominator, long desiredDenominator)
		{
			numerator = (numerator * desiredDenominator) / denominator;
			denominator = desiredDenominator;
		}

		public static void Commonize(ref long numeratorA, ref long denominatorA, ref long numeratorB, ref long denominatorB)
		{
			long newNumeratorA = numeratorA * denominatorB;
			long newDenominator = denominatorA * denominatorB;
			long newNumeratorB = numeratorB * denominatorA;
			bool finished = false;

			while (!finished)
			{
				long max = newDenominator / 2;

				for (int i = 0; i < PrimeCount; i++)
				{
					long thisPrime = Primes[i];

					if (thisPrime > max)
					{
						finished = true;
						break;
					}
					if ((newDenominator % thisPrime == 0) && (newNumeratorA % thisPrime == 0) && (newNumeratorB % thisPrime == 0))
					{
						newDenominator /= thisPrime;
						newNumeratorA /= thisPrime;
						newNumeratorB /= thisPrime;
						break;
					}
				}
			}

			numeratorA = newNumeratorA;
			numeratorB = newNumeratorB;
			denominatorA = newDenominator;
			denominatorB = newDenominator;
		}

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

		public static void Reduce(ref long numerator, ref long denominator)
		{
			bool finished = false;

			while (!finished)
			{
				long max = denominator / 2;

				for (int i = 0; i < PrimeCount; i++)
				{
					long thisPrime = Primes[i];

					if (thisPrime > max)
					{
						finished = true;
						break;
					}
					if ((denominator % thisPrime == 0) && (numerator % thisPrime == 0))
					{
						denominator /= thisPrime;
						numerator /= thisPrime;
						break;
					}
				}
			}
		}
	}
}
