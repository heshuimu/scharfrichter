using Scharfrichter.Codec;
using Scharfrichter.Codec.Archives;
using Scharfrichter.Codec.Charts;
using Scharfrichter.Codec.Sounds;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ConvertHelper
{
	static public class BemaniToSM
	{
		static public void Convert(string[] inArgs)
		{
			Console.WriteLine("DJSLACKERS - BemaniToSM");

			string[] args;

			if (inArgs.Length > 0)
				args = Subfolder.Parse(inArgs);
			else
				args = inArgs;

			foreach (string filename in args)
			{
				if (File.Exists(filename) && Path.GetExtension(filename).ToUpper() == ".SSQ")
				{
					string outTitle = Path.GetFileNameWithoutExtension(filename);
					string outFile = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".SM");

					Console.WriteLine();
					Console.WriteLine("Processing file " + filename);

					using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						BemaniSSQ ssq = BemaniSSQ.Read(fs, 0x1000);
						StepmaniaSM sm = new StepmaniaSM();

						sm.Tags["Title"] = outTitle;
						sm.Tags["Artist"] = "";
						sm.CreateTempoTags(ssq.TempoEntries.ToArray());

						foreach (Chart chart in ssq.Charts)
						{
							string gameType;
							string difficulty;
							switch (chart.Tags["Difficulty"])
							{
								case @"1": difficulty = "Light"; break;
								case @"2": difficulty = "Standard"; break;
								case @"3": difficulty = "Heavy"; break;
								case @"4": difficulty = "Beginner"; break;
								case @"6": difficulty = "Challenge"; break;
								default: difficulty = ""; break;
							}
							switch (chart.Tags["Panels"])
							{
								case @"4": gameType = "dance-single"; break;
								case @"8": gameType = "dance-double"; break;
								default: gameType = ""; break;
							}
							chart.Entries.Sort();
							sm.CreateStepTag(chart.Entries.ToArray(), gameType, "", difficulty, "", "", System.Convert.ToInt32(chart.Tags["Panels"]), 16);
						}

						sm.WriteFile(outFile);
					}
				}
			}
		}
	}
}
