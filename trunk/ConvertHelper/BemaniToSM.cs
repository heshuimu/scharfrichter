using Scharfrichter.Codec;
using Scharfrichter.Codec.Archives;
using Scharfrichter.Codec.Charts;
using Scharfrichter.Codec.Sounds;
using Scharfrichter.Common;

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
			Splash.Show("Bemani To Stepmania");

			string[] args;

			if (inArgs.Length > 0)
				args = Subfolder.Parse(inArgs);
			else
				args = inArgs;

			if (args.Length == 0)
			{
				Console.WriteLine();
				Console.WriteLine("Usage: BemaniToSM <input file>");
				Console.WriteLine();
				Console.WriteLine("Drag and drop with files and folders is fully supported for this application.");
				Console.WriteLine();
				Console.WriteLine("Supported formats:");
				Console.WriteLine("SSQ, XWB");
			}

			foreach (string filename in args)
			{
				if (File.Exists(filename))
				{
					Console.WriteLine();
					Console.WriteLine("Processing File: " + filename);
					switch (Path.GetExtension(filename).ToUpper())
					{
						case @".XWB":
							{
								using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
								{
									Console.WriteLine("Reading XWB bank");
									MicrosoftXWB bank = MicrosoftXWB.Read(fs);
									string outPath = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));

									Directory.CreateDirectory(outPath);

									int count = bank.SoundCount;
									Console.WriteLine("Writing " + count.ToString() + " samples.");

									for (int i = 0; i < count; i++)
									{
										string outFileName;

										if ((bank.Sounds[i].Name == null) || (bank.Sounds[i].Name == ""))
											outFileName = Util.ConvertToHexString(i, 4);
										else
											outFileName = bank.Sounds[i].Name;

										string outFile = Path.Combine(outPath, outFileName + ".wav");
										bank.Sounds[i].WriteFile(outFile, 1.0f);
									}

									bank = null;
								}
							}
							break;
						case @".SSQ":
							{
								string outTitle = Path.GetFileNameWithoutExtension(filename);
								string outFile = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".SM");

								Console.WriteLine();
								Console.WriteLine("Processing file " + filename);

								using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
								{
									BemaniSSQ ssq = BemaniSSQ.Read(fs, 0x1000);
									StepmaniaSM sm = new StepmaniaSM();

									sm.Tags["TITLE"] = outTitle;
									sm.Tags["ARTIST"] = "";
									sm.Tags["TITLETRANSLIT"] = "";
									sm.Tags["ARTISTTRANSLIT"] = "";
									sm.Tags["BANNER"] = outTitle + ".png";
									sm.Tags["BACKGROUND"] = outTitle + "-bg.png";
									sm.Tags["OFFSET"] = "0.000";
									sm.Tags["SAMPLELENGTH"] = "14.000";

									sm.CreateTempoTags(ssq.TempoEntries.ToArray());

									foreach (Chart chart in ssq.Charts)
									{
										string gameType;
										string difficulty;
										switch (chart.Tags["Difficulty"])
										{
											case @"1": difficulty = "Easy"; break;
											case @"2": difficulty = "Medium"; break;
											case @"3": difficulty = "Hard"; break;
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

										// couples chart check
										if (gameType == "dance-single")
										{
											foreach (Entry entry in chart.Entries)
											{
												if (entry.Type == EntryType.Marker && entry.Column >= 4)
												{
													gameType = "dance-couple";
													chart.Tags["Panels"] = "8";
													break;
												}
											}
										}

										sm.CreateStepTag(chart.Entries.ToArray(), gameType, "", difficulty, "0", "", System.Convert.ToInt32(chart.Tags["Panels"]), 192);
									}

									sm.WriteFile(outFile);
								}
							}
							break;
					}
				}
			}
		}
	}
}
