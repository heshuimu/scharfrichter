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

		static public void BemaniToBMS(string[] args, long unitNumerator, long unitDenominator, int quantizeMeasure)
		{
			for (int i = 0; i < args.Length; i++)
			{
				if (File.Exists(args[i]))
				{
					byte[] data = File.ReadAllBytes(args[i]);
					switch (Path.GetExtension(args[i]).ToUpper())
					{
						case @".1":
							using (MemoryStream source = new MemoryStream(data))
							{
								Bemani1 archive = Bemani1.Read(source, unitNumerator, unitDenominator);
								for (int j = 0; j < archive.ChartCount; j++)
								{
									if (archive.Charts[j] != null)
									{
										if (quantizeMeasure > 0)
											archive.Charts[j].QuantizeMeasureLengths(quantizeMeasure);

										using (MemoryStream mem = new MemoryStream())
										{
											BMS bms = new BMS();
											bms.Charts = new Chart[] { archive.Charts[j] };

											string output = Path.GetFileNameWithoutExtension(args[i]) + " " + chartTitles[j] + ".bms";
											string name = Path.GetFileNameWithoutExtension(Path.GetFileName(output));

											bms.Charts[0].Tags["TITLE"] = name;
											bms.GenerateSampleTags();
											bms.Write(mem);

											File.WriteAllBytes(output, mem.ToArray());
										}
									}
								}
							}
							break;
						case @".2DX":
							using (MemoryStream source = new MemoryStream(data))
							{
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
									using (FileStream outfile = new FileStream(Path.Combine(targetPath, alphabet.Substring(sampleIndex / alphabetLength, 1) + alphabet.Substring(sampleIndex % alphabetLength, 1) + @".wav"), FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
									{
										soundList[j].Write(outfile);
									}
								}
							}
							break;
					}
				}
			}
		}
	}
}
