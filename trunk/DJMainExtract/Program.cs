using Scharfrichter.Codec;
using Scharfrichter.Codec.Archives;
using Scharfrichter.Codec.Charts;
using Scharfrichter.Codec.Media;
using Scharfrichter.Codec.Sounds;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DJMainExtract
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("DJSLACKERS - DJMainExtract");

			args = Subfolder.Parse(args);

			if (System.Diagnostics.Debugger.IsAttached && args.Length == 0)
			{
				Console.WriteLine();
				Console.WriteLine("Debugger attached. Input file name:");
				args = new string[] { Console.ReadLine() };
				if (args[0] == "")
				{
					args[0] = @"D:\chds\753jaa11.chd";
				}
			}

			for (int i = 0; i < args.Length; i++)
			{
				if (File.Exists(args[i]))
				{
					string sourceFileName = Path.GetFileNameWithoutExtension(args[i]);
					string sourcePath = Path.GetDirectoryName(args[i]);
					string targetPath = Path.Combine(sourcePath, sourceFileName);
					Directory.CreateDirectory(targetPath);

					Console.WriteLine();
					Console.WriteLine("Processing " + args[i]);

					using (FileStream fs = new FileStream(args[i], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						CHD chd = CHD.Load(fs);
						BinaryReader reader = new BinaryReader(chd);

						long totalChunks = (int)(chd.Length / 0x1000000L);

						for (int j = 0; j < totalChunks; j++)
						{
							chd.Position = (long)j * 0x1000000;
							//DJMainChunk chunk = DJMainChunk.Read(chd, new int[] { 0x002000, 0x006000, 0x00A000, 0x00E000, 0x012000, 0x016000 }, new int[] { 0x000000, 0x000200 }, 0x020000);
							DJMainChunk chunk = DJMainChunk.Read(chd, new int[] { 0x000400 }, new int[] { 0x000000, 0x000200 }, 0x002000);

							if (chunk.ChartCount > 0)
							{
								Console.WriteLine("Exporting set " + j.ToString());
								string fname = Path.Combine(Path.GetDirectoryName(args[i]), Util.ConvertToDecimalString(j, 3));
								ConvertHelper.ConvertFunctions.BemaniToBMSConvertChart(chunk.Charts[0], 32, fname, "", 0, chunk.SampleMaps[0]);
								ConvertHelper.ConvertFunctions.BemaniToBMSConvertSounds(chunk.Sounds, fname, 0.6f);
							}
						}

					}
				}
			}

		}
	}
}
