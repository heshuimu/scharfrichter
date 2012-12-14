﻿using System;
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

			// bemani1 writeback test with quantization
			using (FileStream fs = new FileStream(@"D:\BMS\1101.1", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				Bemani1 test = Bemani1.Read(fs, 100, 5994);
				using (MemoryStream mem = new MemoryStream())
				{
					test.Write(mem, 100, 5994);
					File.WriteAllBytes(@"D:\BMS\1101.out", mem.ToArray());
				}
				using (MemoryStream mem = new MemoryStream())
				{
					for (int i = 0; i < 12; i++)
					{
						if (test.Charts[i] != null)
							test.Charts[i].QuantizeMeasureLengths(32);
					}
					test.Write(mem, 100, 5994);
					File.WriteAllBytes(@"D:\BMS\1101q.out", mem.ToArray());
				}
			}

			// bemaniLZ decompression test and bemanics2 to bemani1 test
			using (FileStream fs = new FileStream(@"D:\BMS\compchart.dat", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				using (MemoryStream mem = new MemoryStream())
				{
					BemaniLZ.Decode(fs, mem);
					File.WriteAllBytes(@"D:\BMS\compchart.dec", mem.ToArray());
					mem.Position = 0;

					using (MemoryStream output = new MemoryStream())
					{
						Bemani1 b1 = new Bemani1();
						BemaniCS2 bcs2 = BemaniCS2.Read(mem);
						b1.Charts[0] = bcs2.Charts[0];
						b1.Write(output, 100, 5994);
						File.WriteAllBytes(@"D:\BMS\compchart.1", output.ToArray());
					}
				}
			}

			MessageBox.Show("Finished.");
		}
	}
}
