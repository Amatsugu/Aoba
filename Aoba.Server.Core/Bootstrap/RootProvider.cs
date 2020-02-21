using Nancy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuminousVector.Aoba.Server.Bootstrap
{
	public class RootProvider : IRootPathProvider
	{
		public string GetRootPath()
		{
			var curDir = Directory.GetCurrentDirectory();
			
			curDir = Directory.GetParent(curDir).FullName;
			Console.WriteLine($"Root: {curDir}");
			return curDir;
		}
	}
}
