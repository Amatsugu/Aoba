using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuminousVector.Aoba.Server.Bootstrap
{
#if DEBUG
	public class RootProvider : IRootPathProvider
	{
		public string GetRootPath()
		{
			return @"C:\Users\Karuta\Documents\GitHub\Aoba\Aoba.Server";
		}
	}
#endif
}
