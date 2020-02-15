using Nancy.ViewEngines.Razor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuminousVector.Aoba.Server.Bootstrap
{
	public class RazorConfig : IRazorConfiguration
	{
		public IEnumerable<string> GetAssemblyNames()
		{
			return null;
		}

		public IEnumerable<string> GetDefaultNamespaces()
		{
			yield return "LuminousVector.Aoba.Server";
			yield return "LuminousVector.Aoba.Server.Models";
		}

		public bool AutoIncludeModelNamespace
		{
			get { return true; }
		}
	}
}
