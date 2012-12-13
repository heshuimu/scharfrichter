using Scharfrichter.Codec.Charts;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Archives
{
	// Be-Music Source File.

	public class BMS : Archive
	{
		private Chart chart = null;

		public override Chart[] Charts
		{
			get
			{
				if (chart != null)
					return new Chart[] { chart };
				else
					return base.Charts;
			}
			set
			{
				if (value != null && value.Length > 0)
					chart = value[0];
			}
		}

		public override int ChartCount
		{
			get
			{
				return (chart != null) ? 1 : 0;
			}
		}

		static public BMS Load(Stream source)
		{
			Dictionary<string, string> noteTags = new Dictionary<string,string>();
			BMS result = new BMS();
			Chart chart = new Chart();

			using (StreamReader reader = new StreamReader(source))
			{
				while (!reader.EndOfStream)
				{
					string currentLine = reader.ReadLine();

					if (currentLine.StartsWith("#"))
					{
						currentLine = currentLine.Substring(1);
						currentLine = currentLine.Replace("\t", " ");

						if (currentLine.Contains(" "))
						{
							int separatorOffset = currentLine.IndexOf(" ");
							string val = currentLine.Substring(separatorOffset + 1).Trim();
							string tag = currentLine.Substring(0, separatorOffset).Trim().ToUpper();
							if (tag != "")
								chart.Tags[tag] = val;
						}
						else if (currentLine.Contains(":"))
						{
							int separatorOffset = currentLine.IndexOf(":");
							string val = currentLine.Substring(separatorOffset + 1).Trim();
							string tag = currentLine.Substring(0, separatorOffset).Trim().ToUpper();
							if (tag != "")
								noteTags[tag] = val;
						}
					}
				}
			}
			 
			foreach (KeyValuePair<string, string> tag in noteTags)
			{
				if (tag.Key.Length == 5)
				{
					string measure = tag.Key.Substring(0, 3);
					string lane = tag.Key.Substring(3, 2);
				}
			}
			 
			result.chart = chart;
			return result;
		}

		public void Save(Stream target)
		{
		}
	}
}
