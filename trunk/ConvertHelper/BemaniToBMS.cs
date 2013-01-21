using Scharfrichter.Codec;
using Scharfrichter.Codec.Archives;
using Scharfrichter.Codec.Charts;
using Scharfrichter.Codec.Sounds;
using Scharfrichter.Common;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ConvertHelper
{
	static public class BemaniToBMS
	{
		private const string configFileName = "Convert";
		private const string databaseFileName = "BeatmaniaDB";

		static string[] chartTitlesIIDX1 = new string[]
		{
			"[1P Hyper]",
			"[1P Normal]",
			"[1P Another]",
			"[1P Beginner]",
			"[1P 4]",
			"[1P 5]",
			"[2P Hyper]",
			"[2P Normal]",
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

		static public void Convert(string[] inArgs, long unitNumerator, long unitDenominator)
		{
			// configuration
			Configuration config = LoadConfig();
			int quantizeMeasure = config["BMS"].GetValue("QuantizeMeasure");
			int quantizeNotes = config["BMS"].GetValue("QuantizeNotes");

			// splash
			Splash.Show("Bemani to BeMusic Script");
			Console.WriteLine("Timing: " + unitNumerator.ToString() + "/" + unitDenominator.ToString());
			Console.WriteLine("Measure Quantize: " + quantizeMeasure.ToString());

			// args
			string[] args;
			if (inArgs.Length > 0)
				args = Subfolder.Parse(inArgs);
			else
				args = inArgs;

			// debug args (if applicable)
			if (System.Diagnostics.Debugger.IsAttached && args.Length == 0)
			{
				Console.WriteLine();
				Console.WriteLine("Debugger attached. Input file name:");
				args = new string[] { Console.ReadLine() };
			}

			// show usage if no args provided
			if (args.Length == 0)
			{
				Console.WriteLine();
				Console.WriteLine("Usage: BemaniToBMS <input file>");
				Console.WriteLine();
				Console.WriteLine("Drag and drop with files and folders is fully supported for this application.");
				Console.WriteLine();
				Console.WriteLine("Supported formats:");
				Console.WriteLine("1, 2DX, CS, SD9, SSP");
			}

			// process files
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
								ConvertArchive(Bemani1.Read(source, unitNumerator, unitDenominator), quantizeMeasure, quantizeNotes, args[i], chartTitlesIIDX1, difficultyTagsIIDX1);
							break;
						case @".2DX":
							using (MemoryStream source = new MemoryStream(data))
							{
								Console.WriteLine("Converting Samples");
								Bemani2DX archive = Bemani2DX.Read(source);
								ConvertSounds(archive.Sounds, args[i], 0.6f);
							}
							break;
						case @".CS":
							using (MemoryStream source = new MemoryStream(data))
								ConvertChart(BeatmaniaIIDXCSNew.Read(source), quantizeMeasure, quantizeNotes, args[i], "", 0, null);
							break;
						case @".CS2":
							using (MemoryStream source = new MemoryStream(data))
								ConvertChart(BeatmaniaIIDXCSOld.Read(source), quantizeMeasure, quantizeNotes, args[i], "", 0, null);
							break;
						case @".CS5":
							using (MemoryStream source = new MemoryStream(data))
								ConvertChart(Beatmania5Key.Read(source), quantizeMeasure, quantizeNotes, args[i], "", 0, null);
							break;
						case @".CS9":
							break;
						case @".SD9":
							using (MemoryStream source = new MemoryStream(data))
							{
								Sound sound = BemaniSD9.Read(source);
								string targetFile = Path.GetFileNameWithoutExtension(args[i]);
								string targetPath = Path.Combine(Path.GetDirectoryName(args[i]), targetFile) + ".wav";
								sound.WriteFile(targetPath, 1.0f);
							}
							break;
						case @".SSP":
							using (MemoryStream source = new MemoryStream(data))
								ConvertSounds(BemaniSSP.Read(source).Sounds, args[i], 1.0f);
							break;
					}
				}
			}

			// wrap up
			Console.WriteLine("BemaniToBMS finished.");
			SaveConfig(config);
		}

		static public void ConvertArchive(Archive archive, int quantizeMeasure, int quantizeNotes, string filename, string[] chartTitles, int[] difficultyTags)
		{
			for (int j = 0; j < archive.ChartCount; j++)
			{
				if (archive.Charts[j] != null)
				{
					Console.WriteLine("Converting Chart " + j.ToString());
					ConvertChart(archive.Charts[j], quantizeMeasure, quantizeNotes, filename, chartTitles[j], difficultyTags[j], null);
				}
			}
		}

		static public void ConvertChart(Chart chart, int quantizeMeasure, int quantizeNotes, string filename, string title, int difficulty, int[] map)
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

				if (map == null)
					bms.GenerateSampleMap();
				else
					bms.SampleMap = map;

				bms.Charts[0].QuantizeNoteOffsets(quantizeNotes);
				bms.GenerateSampleTags();
				bms.Write(mem, true);

				File.WriteAllBytes(output, mem.ToArray());
			}
		}

		static public void ConvertSounds(Sound[] sounds, string filename, float volume)
		{
			string name = Path.GetFileNameWithoutExtension(Path.GetFileName(filename));
			string targetPath = Path.Combine(Path.GetDirectoryName(filename), name);

			if (!Directory.Exists(targetPath))
				Directory.CreateDirectory(targetPath);

			int count = sounds.Length;

			for (int j = 0; j < count; j++)
			{
				int sampleIndex = j + 1;
				sounds[j].WriteFile(Path.Combine(targetPath, Scharfrichter.Codec.Util.ConvertToBMEString(sampleIndex, 4) + @".wav"), volume);
			}
		}

		static private Configuration LoadConfig()
		{
			Configuration config = Configuration.ReadFile(configFileName);
			config["BMS"].SetDefaultValue("QuantizeMeasure", 16);
			config["BMS"].SetDefaultValue("QuantizeNotes", 192);
			config["IIDX"].SetDefaultString("Difficulty0", "Hyper");
			config["IIDX"].SetDefaultString("Difficulty1", "Normal");
			config["IIDX"].SetDefaultString("Difficulty2", "Another");
			config["IIDX"].SetDefaultString("Difficulty3", "Beginner");
			return config;
		}

		static private Configuration LoadDB()
		{
			Configuration config = Configuration.ReadFile(databaseFileName);
			return config;
		}

		static private void SaveConfig(Configuration config)
		{
			config.WriteFile(configFileName);
		}
	}
}
