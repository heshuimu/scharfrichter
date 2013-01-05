using Scharfrichter.Codec;
using Scharfrichter.Codec.Archives;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IFSExtract
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("DJSLACKERS - IFSExtract");
			args = Subfolder.Parse(args);

			if (System.Diagnostics.Debugger.IsAttached && args.Length == 0)
			{
				Console.WriteLine();
				Console.WriteLine("Debugger attached. Input file name:");
				args = new string[] { Console.ReadLine() };
				if (args[0] == "")
				{
					args[0] = @"d:\bms\d_result.ifs";
					//args[0] = @"d:\bms\sound\[ifs]\01000.ifs";
				}
			}

			if (args != null && args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					string filename = args[i];

					if (File.Exists(filename))
					{
						Console.WriteLine();
						Console.WriteLine("Processing file " + args[i]);

						using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
						{
							string outputPath = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename));
							string outputFileBase = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(Path.GetFileName(filename)));

							Directory.CreateDirectory(outputPath);

							BemaniIFS archive = BemaniIFS.Read(fs);
							int count = archive.RawDataCount;

							bool standardLayout = false;

							if (count > 1)
							{
								byte[] data = archive.RawData[count - 1];
								if (data.Length > 0x60)
								{
									if (data[0] == 0x60 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x00)
									{
										standardLayout = true;
									}
								}
							}

							Console.WriteLine("Exporting files.");

							for (int j = 0; j < count; j++)
							{
								string outputNumber = Util.ConvertToDecimalString(j, 3);
								string outputFile = outputFileBase;
								if (!standardLayout)
								{
									outputFile += "." + outputNumber;
								}
								else
								{
									if (j == count - 1)
										outputFile += ".1";
									else
										outputFile += "-" + outputNumber + ".2dx";
								}
								byte[] data = archive.RawData[j];

								File.WriteAllBytes(outputFile, archive.RawData[j]);
							}
						}
					}
				}
			}
		}
	}
}
