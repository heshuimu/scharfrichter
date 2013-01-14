using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// Because the WaveFileWriter does not write the header information until the stream
// is disposed, the class is useless in its current state. This class extends the functionality
// of UpdateHeader to the program so we can keep the stream open. We aren't always using
// a FileStream after all.

namespace NAudio.Wave
{
	public class WaveWriter : WaveFileWriter
	{
		private BinaryWriter writer;

		public WaveWriter(Stream target, WaveFormat format)
			: base(target, format)
		{
			writer = new BinaryWriter(target);
		}

		~WaveWriter()
		{
			Dispose(true);
		}

		public void Update()
		{
			UpdateHeader(writer);
		}
	}
}
