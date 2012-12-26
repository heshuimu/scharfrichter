using Scharfrichter.Codec.Archives;
using Scharfrichter.Codec.Charts;
using Scharfrichter.Codec.Sounds;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConvertHelper
{
	static public class ConvertFunctions
	{
		static string[] chartTitlesIIDX1 = new string[]
		{
			"[1P Hyper]",
			"[1P Light]",
			"[1P Another]",
			"[1P Beginner]",
			"[1P 4]",
			"[1P 5]",
			"[2P Hyper]",
			"[2P Light]",
			"[2P Another]",
			"[2P Beginner]",
			"[2P 4]",
			"[2P 5]"
		};

		static int[] difficultyTagsIIDX1 = new int[]
		{
			3,
			2,
			4,
			1,
			0,
			0,
			3,
			2,
			4,
			1,
			0,
			0
		};

		static public void BemaniToBMS(string[] args, long unitNumerator, long unitDenominator, int quantizeMeasure)
		{
			Console.WriteLine("BemaniToBMS");
			Console.WriteLine("Timing: " + unitNumerator.ToString() + "/" + unitDenominator.ToString());
			Console.WriteLine("Measure Quantize: " + quantizeMeasure.ToString());

			if (System.Diagnostics.Debugger.IsAttached && args.Length == 0)
			{
				Console.WriteLine();
				Console.WriteLine("Debugger attached. Input file name:");
				args = new string[] { Console.ReadLine() };
			}

			for (int i = 0; i < args.Length; i++)
			{
				if (File.Exists(args[i]))
				{
					Console.WriteLine();
					Console.WriteLine("Processing File: " + args[i]);

					byte[] data = File.ReadAllBytes(args[i]);
					switch (Path.GetExtension(args[i]).ToUpper())
					{
						case @".1":
							using (MemoryStream source = new MemoryStream(data))
								BemaniToBMSConvertArchive(Bemani1.Read(source, unitNumerator, unitDenominator), quantizeMeasure, args[i], chartTitlesIIDX1, difficultyTagsIIDX1);
							break;
						case @".2DX":
							using (MemoryStream source = new MemoryStream(data))
							{
								Console.WriteLine("Converting Samples");
								string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
								int alphabetLength = alphabet.Length;

								string name = Path.GetFileNameWithoutExtension(Path.GetFileName(args[i]));
								string targetPath = Path.Combine(Path.GetDirectoryName(args[i]), name);

								if (!Directory.Exists(targetPath))
									Directory.CreateDirectory(targetPath);

								Bemani2DX archive = Bemani2DX.Read(source);
								Sound[] soundList = archive.Sounds;
								int count = soundList.Length;

								for (int j = 0; j < count; j++)
								{
									int sampleIndex = j + 1;
									using (FileStream outfile = new FileStream(Path.Combine(targetPath, Scharfrichter.Codec.Util.ConvertToBMEString(sampleIndex, 4) + @".wav"), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
									{
										soundList[j].Write(outfile);
									}
								}
							}
							break;
						case @".CS":
							using (MemoryStream source = new MemoryStream(data))
								BemaniToBMSConvertChart(BeatmaniaIIDXCSOld.Read(source), quantizeMeasure, args[i], "", 0);
							break;
						case @".CS2":
							break;
						case @".CS5":
							break;
						case @".CS9":
							break;
					}
				}
			}
			Console.WriteLine("BemaniToBMS finished.");
		}

		static private void BemaniToBMSConvertArchive(Archive archive, int quantizeMeasure, string filename, string[] chartTitles, int[] difficultyTags)
		{
			for (int j = 0; j < archive.ChartCount; j++)
			{
				if (archive.Charts[j] != null)
				{
					Console.WriteLine("Converting Chart " + j.ToString());
					BemaniToBMSConvertChart(archive.Charts[j], quantizeMeasure, filename, chartTitles[j], difficultyTags[j]);
				}
			}
		}

		static private void BemaniToBMSConvertChart(Chart chart, int quantizeMeasure, string filename, string title, int difficulty)
		{
			if (quantizeMeasure > 0)
				chart.QuantizeMeasureLengths(quantizeMeasure);

			using (MemoryStream mem = new MemoryStream())
			{
				BMS bms = new BMS();
				bms.Charts = new Chart[] { chart };

				string name = Path.GetFileNameWithoutExtension(Path.GetFileName(filename)); //ex: "1204 [1P Another]"
				if (title != null && title.Length > 0)
				{
					name += " " + title;
				}

				string output = Path.Combine(Path.GetDirectoryName(filename), @"@" + name + ".bms");

				// write some tags
				bms.Charts[0].Tags["TITLE"] = Path.GetFileNameWithoutExtension(Path.GetFileName(filename));

				if (difficulty > 0)
					bms.Charts[0].Tags["DIFFICULTY"] = difficulty.ToString();

				if (bms.Charts[0].Players > 1)
					bms.Charts[0].Tags["PLAYER"] = "3";
				else
					bms.Charts[0].Tags["PLAYER"] = "1";

				bms.GenerateSampleMap();
				bms.GenerateSampleTags();
				bms.Write(mem);

				File.WriteAllBytes(output, mem.ToArray());
			}
		}

	}
}
