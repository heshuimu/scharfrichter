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
			if (args != null && args.Length > 0)
			{
				for (int i = 0; i < args.Length; i++)
				{
					string filename = args[i];

					using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
					{
						string outputPath = Path.GetDirectoryName(filename);
						string outputFileBase = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(Path.GetFileName(filename)));

						BemaniIFS archive = BemaniIFS.Read(fs);
						int count = archive.RawDataCount;

						for (int j = 0; j < count; j++)
						{
							string outputNumber = j.ToString();
							while (outputNumber.Length < 3)
							{
								outputNumber = "0" + outputNumber;
							}
							string outputFile = outputFileBase + "-" + outputNumber;
							byte[] data = archive.RawData[j];

							File.WriteAllBytes(outputFile, archive.RawData[j]);
						}
					}
				}
			}
		}
	}
}
