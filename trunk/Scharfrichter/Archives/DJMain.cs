using Scharfrichter.Codec.Charts;
using Scharfrichter.Codec.Sounds;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Archives
{
	public class DJMainChunk : Archive
	{
		private List<Chart> charts;
		private List<Sound> sounds;

		static public Archive Read(Stream source, int[] chartOffsets, int[] sampleInfoOffsets, int sampleDataOffset)
		{
			byte[] rawData = new byte[0x1000000];
			source.Read(rawData, 0, rawData.Length);


		}
	}
}
