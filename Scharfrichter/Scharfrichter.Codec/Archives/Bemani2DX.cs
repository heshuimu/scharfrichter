using Scharfrichter.Codec.Encryption;
using Scharfrichter.Codec.Sounds;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Archives
{
	public enum Bemani2DXType
	{
		Unencrypted,
		IIDX9,
		IIDX10,
		IIDX11,
		IIDX12,
		IIDXHID,
		IIDX16
	}

	public class Bemani2DX : Archive
	{
		public Bemani2DXType Type;

		static public Bemani2DX Read(Stream source)
		{
			Bemani2DX result = new Bemani2DX();
			BinaryReader reader = new BinaryReader(source);
			byte[] key = new byte[] { };
			Bemani2DXEncryptionType encType = Bemani2DXEncryptionType.Standard;

			string headerID = new string(reader.ReadChars(4));

			switch (headerID)
			{
				case @"%eNc":
					result.Type = Bemani2DXType.IIDX9;
					key = Bemani2DXEncryptionKeys.IIDX9;
					encType = Bemani2DXEncryptionType.Standard;
					break;
				case @"%e10":
					result.Type = Bemani2DXType.IIDX10;
					key = Bemani2DXEncryptionKeys.IIDX10;
					encType = Bemani2DXEncryptionType.Standard;
					break;
				case @"%e11":
					result.Type = Bemani2DXType.IIDX11;
					key = Bemani2DXEncryptionKeys.IIDX11;
					encType = Bemani2DXEncryptionType.Standard;
					break;
				case @"%e12":
					result.Type = Bemani2DXType.IIDX12;
					key = Bemani2DXEncryptionKeys.IIDX11;
					encType = Bemani2DXEncryptionType.Partial;
					break;
				case @"%hid":
					result.Type = Bemani2DXType.IIDXHID;
					key = Bemani2DXEncryptionKeys.IIDX11;
					encType = Bemani2DXEncryptionType.Partial;
					break;
				case @"%iO0":
					result.Type = Bemani2DXType.IIDX16;
					key = Bemani2DXEncryptionKeys.IIDX16;
					encType = Bemani2DXEncryptionType.Standard;
					break;
				default:
					result.Type = Bemani2DXType.Unencrypted;
					source.Position -= 4;
					break;
			}

			if (result.Type != Bemani2DXType.Unencrypted)
			{
				MemoryStream decodedData = new MemoryStream();
				int filelength = reader.ReadInt32();
				int fileExtraBytes = (8 - (filelength % 8)) % 8;
				byte[] data = reader.ReadBytes(filelength + fileExtraBytes);
				using (MemoryStream encodedDataMem = new MemoryStream(data))
				{
					Bemani2DXEncryption.Decrypt(encodedDataMem, decodedData, key, encType);
				}
				decodedData.Position = 0;
				reader = new BinaryReader(decodedData);
				File.WriteAllBytes(@"D:\BMS\2dxout.dat", decodedData.ToArray());
			}

			return result;
		}
	}
}
