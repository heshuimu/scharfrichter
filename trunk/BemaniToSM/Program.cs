using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BemaniToSM
{
	class Program
	{
		static void Main(string[] args)
		{
			if (System.Diagnostics.Debugger.IsAttached && args.Length == 0)
			{
				args = new string[] { @"D:\SSQ" };
			}

			ConvertHelper.BemaniToSM.Convert(args);
		}
	}
}
