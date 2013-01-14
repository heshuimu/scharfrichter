using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Render2DXTroopers
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
				args = new string[] { @"D:\Torrent Seeds\DJHACKERS-LDJ\data\sound\20051" };

			ConvertHelper.Render.RenderWAV(args, 1, 1000);
		}
	}
}
