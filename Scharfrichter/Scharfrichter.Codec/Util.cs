using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec
{
	public struct Fraction
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

		public static Fraction operator +(Fraction a, Fraction b)
		{
			Fraction result = new Fraction();
			Fraction commonA;
			Fraction commonB;

			Commonize(a, b, out commonA, out commonB);
			result.Numerator = (commonA.Numerator + commonB.Numerator);
			result.Denominator = commonA.Denominator;

			return Reduce(result);
		}

		public static Fraction operator -(Fraction a, Fraction b)
		{
			Fraction result = new Fraction();
			Fraction commonA;
			Fraction commonB;

			Commonize(a, b, out commonA, out commonB);
			result.Numerator = (commonA.Numerator - commonB.Numerator);
			result.Denominator = commonA.Denominator;

			return Reduce(result);
		}

		public static Fraction operator *(Fraction a, Fraction b)
		{
			Fraction result = new Fraction();

			result.Numerator = (a.Numerator * b.Numerator);
			result.Denominator = (a.Denominator * b.Denominator);

			return Reduce(result);
		}

		public static Fraction operator /(Fraction a, Fraction b)
		{
			Fraction result = new Fraction();

			result.Numerator = (a.Numerator * b.Denominator);
			result.Denominator = (a.Denominator * b.Numerator);

			return Reduce(result);
		}

		public override string ToString()
		{
			return Numerator.ToString() + "/" + Denominator.ToString();
		}

		public static void Commonize(Fraction a, Fraction b, out Fraction outputA, out Fraction outputB)
		{
			long newNumeratorA = a.Numerator * b.Denominator;
			long newDenominator = a.Denominator * b.Denominator;
			long newNumeratorB = b.Numerator * a.Denominator;
			bool finished = false;

			if (a.Denominator != b.Denominator)
			{
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
				outputA = new Fraction(newNumeratorA, newDenominator);
				outputB = new Fraction(newNumeratorB, newDenominator);
			}
			else
			{
				outputA = a;
				outputB = b;
			}
		}

		public static Fraction Rationalize(double input)
		{
			Fraction result = new Fraction();
			result.Denominator = 1;
			while (input != Math.Round(input))
			{
				input *= 10;
				result.Denominator *= 10;
			}
			result.Numerator = (long)input;
			return Reduce(result);
		}

		public static Fraction Reduce(Fraction input)
		{
			bool finished = false;

			while (!finished)
			{
				long max = input.Denominator / 2;

				for (int i = 0; i < PrimeCount; i++)
				{
					long thisPrime = Primes[i];

					if (thisPrime > max)
					{
						finished = true;
						break;
					}
					if ((input.Denominator % thisPrime == 0) && (input.Numerator % thisPrime == 0))
					{
						input.Denominator /= thisPrime;
						input.Numerator /= thisPrime;
						break;
					}
				}
			}

			return input;
		}

		public Fraction(long newNum, long newDen)
		{
			Denominator = newDen;
			Numerator = newNum;
		}

		public long Denominator;
		public long Numerator;
	}

	static class Util
	{
		public static Fraction CalculateMeasureRate(Fraction bpm)
		{
			Fraction result = new Fraction();
			result.Numerator = 240 * bpm.Denominator;
			result.Denominator = bpm.Numerator;
			return Fraction.Reduce(result);
		}
	}
}
