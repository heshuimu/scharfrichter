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

			using (FileStream fs = new FileStream(@"D:\BMS\026\@026 1P Another.bms", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				BMS test = BMS.Read(fs);
			}

			using (FileStream fs = new FileStream(@"D:\BMS\1101.1", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
			{
				Bemani1 test = Bemani1.Read(fs, 100, 5994);
			}
		}
	}
}
