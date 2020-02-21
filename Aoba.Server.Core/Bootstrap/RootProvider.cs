using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuminousVector.Aoba.Server.Bootstrap
{
#if false
	public class RootProvider : IRootPathProvider
	{
		public string GetRootPath()
		{
			var curDir = Directory.GetCurrentDirectory();
			
			curDir = curDir.Replace(@"bin\Debug", "");

			return curDir;
		}
	}
#endif
}
