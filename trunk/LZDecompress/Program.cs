using Scharfrichter.Codec;
using Scharfrichter.Codec.Compression;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LZDecompress
{
	class Program
	{
		static void Main(string[] args)
		{
			args = Subfolder.Parse(args);

			if (args.Length > 0)
			{
				byte[] data = File.ReadAllBytes(args[0]);
				using (MemoryStream mem = new MemoryStream(data))
				{
					using (MemoryStream output = new MemoryStream())
					{
						BemaniLZ.Decode(mem, output);
						File.WriteAllBytes(Path.GetFileNameWithoutExtension(args[0]) + ".out", output.ToArray()); 
					}
				}
			}
		}
	}
}
