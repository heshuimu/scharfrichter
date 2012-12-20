using Scharfrichter.Codec.Archives;
using Scharfrichter.Codec.Charts;

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Convert1toBMS
{
	class Program
	{
		static string[] chartTitles = new string[]
		{
			"[1P Hyper]",
			"[1P Light]",
			"[1P Another]",
			"[1P 3]",
			"[1P 4]",
			"[1P 5]",
			"[2P Hyper]",
			"[2P Light]",
			"[2P Another]",
			"[2P 3]",
			"[2P 4]",
			"[2P 5]"
		};

		static void Main(string[] args)
		{
			for (int i = 0; i < args.Length; i++)
			{
				byte[] data = File.ReadAllBytes(args[i]);
				using (MemoryStream source = new MemoryStream(data))
				{
					Bemani1 archive = Bemani1.Read(source, 100, 5994);
					for (int j = 0; j < archive.ChartCount; j++)
					{
						if (archive.Charts[j] != null)
						{
							archive.Charts[j].QuantizeMeasureLengths(32);

							using (MemoryStream mem = new MemoryStream())
							{
								BMS bms = new BMS();
								bms.Charts = new Chart[] { archive.Charts[j] };

								string output = Path.GetFileNameWithoutExtension(args[i]) + " " + chartTitles[j] + ".bms";
								string name = Path.GetFileNameWithoutExtension(Path.GetFileName(output));

								bms.Charts[0].Tags["TITLE"] = name;

								bms.Write(mem);

								File.WriteAllBytes(output, mem.ToArray());
							}
						}
					}
				}
			}
		}
	}
}
