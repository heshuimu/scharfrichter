using Scharfrichter.Codec.Compression;
using Scharfrichter.Codec.Graphics;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace IFSImageConvert
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("DJSLACKERS - IFSImageConvert");
			if (System.Diagnostics.Debugger.IsAttached && args.Length == 0)
			{
				Console.WriteLine();
				Console.WriteLine("Debugger attached. Using test file.");
				args = new string[] { @"D:\BMS\d_result\d_result.118" };
			}

			foreach (string filename in args)
			{
				if (File.Exists(filename))
				{
					Console.WriteLine();
					Console.WriteLine("Processing file " + filename);

					using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						using (MemoryStream mem = new MemoryStream())
						{
							string outfile = Path.Combine(Path.GetDirectoryName(filename), Path.GetFileNameWithoutExtension(filename) + ".export");
							Console.Write("Input size: " + (fs.Length - 8).ToString() + " ");
							IFSImage.Decompress(fs, mem);
							Console.WriteLine("Output size: " + mem.Length.ToString());
							File.WriteAllBytes(outfile, mem.ToArray());
						}
					}
				}
			}
		}
	}
}
