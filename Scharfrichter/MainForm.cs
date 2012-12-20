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
			// bemani1 to BMS test with quantization
			using (FileStream fs = new FileStream(@"D:\BMS\1101\1101.1", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				Bemani1 test = Bemani1.Read(fs, 100, 5994);
				for (int i = 0; i < 12; i++)
				{
					if (test.Charts[i] != null)
					{
						BMS bms = new BMS();
						test.Charts[i].QuantizeMeasureLengths(32);
						test.Charts[i].QuantizeNoteOffsets(192);
						bms.Charts[0] = test.Charts[i];
						using (MemoryStream mem = new MemoryStream())
						{
							string bmsString = i.ToString();
							while (bmsString.Length < 2)
							{
								bmsString = "0" + bmsString;
							}
							bms.Write(mem);
							File.WriteAllBytes(@"D:\BMS\1101\1101-" + bmsString + ".bms", mem.ToArray());
						}
					}
				}
			}
			using (FileStream fs = new FileStream(@"D:\BMS\1101\1101.2dx", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				Bemani2DX archive = Bemani2DX.Read(fs);
			}

			MessageBox.Show("Finished.");
		}
	}
}
