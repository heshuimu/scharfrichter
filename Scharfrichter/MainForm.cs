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
			// run all tests (local files here)

			// bms->bemani1 test
			using (FileStream fs = new FileStream(@"D:\BMS\026\@026 1P Another.bms", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				BMS test = BMS.Read(fs);
				using (MemoryStream mem = new MemoryStream())
				{
					Bemani1 output = new Bemani1();
					output.Charts[0] = test.Charts[0];
					output.Write(mem, 100, 5994);
					File.WriteAllBytes(@"D:\BMS\bms.out", mem.ToArray());
				}
			}

			// bemani1 to BMS test with quantization
			using (FileStream fs = new FileStream(@"D:\BMS\0100\0100.1", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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
							File.WriteAllBytes(@"D:\BMS\0100\0100-" + bmsString + ".bms", mem.ToArray());
						}
					}
				}
			}

			MessageBox.Show("Finished.");
		}
	}
}
