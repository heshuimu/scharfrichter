using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec
{
	static public class Xact3WB
	{
		public const int MaxWmaAvgBytesPerSecEntries = 7;

		public enum WaveBankSegIdx
		{
			BankData = 0,
			EntryMetaData,
			SeekTables,
			EntryNames,
			EntryWaveData,
			Count
		}

		public struct WaveBankHeader
		{
			public int Signature;
			public int Version;
			public int HeaderVersion;
			public WaveBankRegion[] Segments;
		}

		public struct WaveBankRegion
		{
			public int Offset;
			public int Length;
		}

		public struct WaveBankSampleRegion
		{
			public int StartSample;
			public int TotalSamples;
		}
	}
}
