using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Scharfrichter.Codec.Archives;
using Scharfrichter.Codec.Charts;
using Scharfrichter.Codec.Compression;
using Scharfrichter.Codec.Graphics;
using Scharfrichter.Codec.Sounds;
using Scharfrichter.Codec.Videos;

namespace Scharfrichter
{
	public partial class MainForm : Form
	{
		public MainForm()
		{
			InitializeComponent();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			string sourceFileName = "0927";
			string targetPath = @"D:\BMS\" + sourceFileName + @"\";
			Directory.CreateDirectory(targetPath);

			string sourcePath = @"D:\Torrent Seeds\Happy Sky\ECO\data\sd_data\" + sourceFileName + @"\";

#if (true)
			// bemani1 to BMS test with quantization
			using (FileStream fs = new FileStream(sourcePath + sourceFileName + ".1", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				Bemani1 test = Bemani1.Read(fs, 100, 5994);
				for (int i = 0; i < 12; i++)
				{
					if (test.Charts[i] != null)
					{
						BMS bms = new BMS();
						test.Charts[i].QuantizeMeasureLengths(16);
						test.Charts[i].QuantizeNoteOffsets(192);
						bms.Charts[0] = test.Charts[i];
						using (MemoryStream mem = new MemoryStream())
						{
							string bmsString = i.ToString();
							while (bmsString.Length < 2)
							{
								bmsString = "0" + bmsString;
							}
							bms.GenerateSampleTags();
							bms.Write(mem);
							File.WriteAllBytes(targetPath + "@" + bmsString + ".bms", mem.ToArray());
						}
					}
				}
			}
#endif

#if (true)
			string alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			int alphabetLength = alphabet.Length;

			using (FileStream fs = new FileStream(sourcePath + sourceFileName + ".2dx", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				Bemani2DX archive = Bemani2DX.Read(fs);
				Sound[] soundList = archive.Sounds;
				int count = soundList.Length;

				for (int i = 0; i < count; i++)
				{
					int sampleIndex = i + 1;
					using (FileStream outfile = new FileStream(targetPath + alphabet[sampleIndex / alphabetLength] + alphabet[sampleIndex % alphabetLength] + @".wav", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
					{
						soundList[i].Write(outfile);
					}
				}
			}
#endif

			MessageBox.Show("Finished.");
		}
	}
}
