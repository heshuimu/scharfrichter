using Scharfrichter.Codec;
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
			Console.WriteLine("DJMainExtract");

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
						chd.Position = 0x4000000;
						byte[] data = reader.ReadBytes(0x1000000);
						Util.ByteSwap(data, 2);						
						File.WriteAllBytes(Path.Combine(targetPath, "temp"), data);
					}
				}
			}

		}
	}
}
