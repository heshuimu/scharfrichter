using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

// the decompression function is ported from unz.c (thanks Tau)

namespace Scharfrichter.Codec.Compression
{
	static public class BemaniLZSS2
	{
		static public void Compress(Stream source, Stream target, int length, out int decompLength)
		{
			decompLength = 0;
		}

		static public void Decompress(Stream source, Stream target, int length, int decompLength)
		{
			byte[] ring = new byte[0x1000];
			int ring_pos = 0x0FEE; 
			int chunk_offset; 
			int chunk_length; 
			int control_word = 1;
			byte cmd1; 
			byte cmd2;
			byte data;

			BinaryReaderEx sourceReader = new BinaryReaderEx(source);
			BinaryWriterEx writer = new BinaryWriterEx(target);

			using (MemoryStream mem = new MemoryStream(sourceReader.ReadBytes(length)))
			{
				BinaryReaderEx reader = new BinaryReaderEx(mem);

				while (decompLength > 0) {
					if (control_word == 1) {
						/* Read a control byte */
						control_word = 0x100 | reader.ReadByte();
					}

					/* Decode a byte according to the current control byte bit */
					if ((control_word & 1) != 0) {
						/* Straight copy, store into history ring */
						data = reader.ReadByte();

						writer.Write(data);
						ring[ring_pos] = data;

						ring_pos = (ring_pos + 1) % 0x1000;
						decompLength--;
					} else {
						/* Reference to data in ring buffer */
						cmd1 = reader.ReadByte();
						cmd2 = reader.ReadByte();

						chunk_length = (cmd2 & 0x0F) + 3;
						chunk_offset = ((cmd2 & 0xF0) << 4) | cmd1;

						for ( ; chunk_length > 0 && length > 0 ; chunk_length--) {
							/* Copy historical data to output AND current ring pos */
							writer.Write(ring[chunk_offset]);
							ring[ring_pos] = ring[chunk_offset];

							/* Update counters */
							chunk_offset = (chunk_offset + 1) % 0x1000;
							ring_pos = (ring_pos + 1) % 0x1000;
							decompLength--;
						}
					}

					/* Get next control bit */
					control_word >>= 1;
				}
			}
		}
	}
}
