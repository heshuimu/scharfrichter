using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Archives
{

	public class MicrosoftXWB : Archive
	{
		static public MicrosoftXWB Read(Stream source)
		{
			MicrosoftXWB result = new MicrosoftXWB();
			using (BinaryReader reader = new BinaryReader(source))
			{
				
			}
			return result;
		}

		public override int SoundCount
		{
			get
			{
				return base.SoundCount;
			}
		}

		public override Sounds.Sound[] Sounds
		{
			get
			{
				return base.Sounds;
			}
			set
			{
				base.Sounds = value;
			}
		}
	}
}
