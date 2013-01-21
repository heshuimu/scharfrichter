using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Scharfrichter.Common
{
	public class Configuration
	{
		public Dictionary<string, InfoCollection> DB = new Dictionary<string,InfoCollection>();

		private Encoding enc;

		public Configuration()
		{
			enc = Encoding.Unicode;
		}

		public Configuration(Encoding encoding)
		{
			enc = encoding;
		}

		public InfoCollection this[string key]
		{
			get 
			{
				if (DB.ContainsKey(key))
					return DB[key];
				else
					return new InfoCollection();
			}
			set 
			{ 
				DB[key] = value; 
			}
		}

		static public string ConfigPath
		{
			get { return Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), "Config"); }
		}

		static public Configuration Read(Stream source)
		{
			try
			{
				StreamReader reader = new StreamReader(source);
				Encoding enc = Encoding.GetEncoding(Convert.ToInt32(reader.ReadLine()));
				Configuration result = new Configuration(enc);
				InfoCollection currentKey = null;
				string currentKeyName = "";

				while (reader.EndOfStream)
				{
					string line = reader.ReadLine().Trim();
					if (line.StartsWith("[") && line.EndsWith("]"))
					{
						currentKey = new InfoCollection();
						currentKeyName = line.Substring(1, line.Length - 2);
					}
					else if (currentKey != null && line.Contains("="))
					{
						string keyTag = line.Substring(0, line.IndexOf("=")).Trim().ToUpper();
						string keyValue = line.Substring(line.IndexOf("=") + 1).Trim();
						currentKey[keyTag] = keyValue;
					}
					else if (line.Length > 0)
					{
						currentKey[line] = "";
					}
				}

				return result;
			}
			catch
			{
				return new Configuration(Encoding.Unicode);
			}
		}

		static public Configuration ReadFile(string configName)
		{
			using (FileStream fs = new FileStream(Path.Combine(Configuration.ConfigPath, configName), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				return Read(fs);
		}

		public void Write(Stream target)
		{
			StreamWriter writer = new StreamWriter(target);
			writer.WriteLine(enc.CodePage.ToString());
			foreach (KeyValuePair<string, InfoCollection> entry in DB)
			{
				writer.WriteLine("[" + entry.Key + "]");
				foreach (KeyValuePair<string, string> item in entry.Value.Items)
				{
					writer.WriteLine(item.Key + "=" + item.Value);
				}
			}
			writer.Flush();
		}

		public void WriteFile(string configName)
		{
			using (FileStream fs = new FileStream(Path.Combine(Configuration.ConfigPath, configName), FileMode.Create, FileAccess.Write, FileShare.None))
				Write(fs);
		}
	}

	public class InfoCollection
	{
		public Dictionary<string, string> Items;

		public string this[string key]
		{
			get
			{
				if (Items.ContainsKey(key))
					return Items[key];
				else
					return "";
			}
			set
			{
				Items[key] = value;
			}
		}
	}
}
