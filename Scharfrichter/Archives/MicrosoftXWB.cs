using SlimDX;
using SlimDX.XACT3;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Scharfrichter.Codec.Archives
{
	public class MicrosoftXWB : Archive
	{
		private WaveBank bank;
		private Engine engine;

		public MicrosoftXWB()
		{
			engine = new Engine();
		}

		static public MicrosoftXWB Read(Stream source)
		{
			MicrosoftXWB result = new MicrosoftXWB();

			using (DataStream ds = (DataStream)source)
			{
				result.bank = result.engine.CreateWaveBank(ds);
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
