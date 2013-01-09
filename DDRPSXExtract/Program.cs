using Scharfrichter.Codec;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DDRPSXExtract
{
	class Program
	{
		static void Main(string[] args)
		{
			if (!System.Diagnostics.Debugger.IsAttached)
			{
				Console.WriteLine("This tool is currently for diagnostics only.");
				Console.WriteLine("It has next to no functionality. It can only be run in the IDE.");
				return;
			}

			string basePath = @"C:\Users\Tony_2\Desktop\New folder (7)\";

			byte[] executableData = File.ReadAllBytes(Path.Combine(basePath, @"SLPM_868.97"));
			byte[] fileData = File.ReadAllBytes(Path.Combine(basePath, @"READ_DT.BIN"));

			using (MemoryStream exe = new MemoryStream(executableData), data = new MemoryStream(fileData))
			{
				BinaryReader exeReader = new BinaryReader(exe);
				BinaryReader dataReader = new BinaryReader(data);

				exe.Position = 0x929F8;

				while (true)
				{
					int length = exeReader.ReadInt32();
					int offset = exeReader.ReadInt32();
					if (length == 0)
						break;

					offset -= 0x4E20;
					offset *= 0x800;
					data.Position = offset;

					byte[] extracted = dataReader.ReadBytes(length);
					File.WriteAllBytes(Path.Combine(basePath, Util.ConvertToHexString(offset, 8) + ".dat"), extracted);
				}
			}
		}
	}
}
