using ConvertHelper;

using Scharfrichter.Codec;
using Scharfrichter.Codec.Archives;
using Scharfrichter.Codec.Charts;
using Scharfrichter.Codec.Media;
using Scharfrichter.Codec.Sounds;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace TwinkleIIDXExtract
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("DJSLACKERS - TwinkleIIDXExtract");

            args = Subfolder.Parse(args);

            if (System.Diagnostics.Debugger.IsAttached && args.Length == 0)
            {
                Console.WriteLine();
                Console.WriteLine("Debugger attached. Input file name:");
                args = new string[] { Console.ReadLine() };
                if (args[0] == "")
                {
                    args[0] = @"C:\Users\Tony\Desktop\work\2nd\2ndstyle.img";
                }
            }

            for (int i = 0; i < args.Length; i++)
            {
                if (File.Exists(args[i]))
                {
                    string sourceFileName = Path.GetFileNameWithoutExtension(args[i]);
                    string sourcePath = Path.GetDirectoryName(args[i]);
                    string targetPath = Path.Combine(sourcePath, sourceFileName);
                    Directory.CreateDirectory(targetPath);

                    Console.WriteLine();
                    Console.WriteLine("Processing " + args[i]);

                    using (FileStream fs = new FileStream(args[i], FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        BinaryReader reader = new BinaryReader(fs);
                        fs.Position = 0x8000000;

                        long totalChunks = (int)((fs.Length - fs.Position) / 0x1A00000L);
                        byte[] rawData = new byte[0x1A00000];

                        for (int j = 0; j < totalChunks; j++)
                        {
                            fs.Read(rawData, 0, rawData.Length);

                            using (MemoryStream ms = new MemoryStream(rawData))
                            {
                                TwinkleChunk chunk = TwinkleChunk.Read(ms, new int[] { 0x002000, 0x006000, 0x00E000 }, new int[] { 0x000000 }, 0x100000);
                                if (chunk.ChartCount > 0)
                                {
                                    Console.WriteLine("Exporting set " + j.ToString());
                                    string fname = Path.Combine(Path.GetDirectoryName(args[i]), Util.ConvertToDecimalString(j, 3));

                                    ConvertHelper.BemaniToBMS.ConvertChart(chunk.Charts[0], null, fname, 0, chunk.SampleMaps[0]);
                                    ConvertHelper.BemaniToBMS.ConvertSounds(chunk.Sounds, fname, 0.6f);
                                }
                            }
                        }

                    }
                }
            }

        }
    }
}
