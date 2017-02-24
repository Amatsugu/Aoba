using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Security;
using Nancy.Responses;

namespace LuminousVector.Aoba.Server.Modules
{
	public class IndexModule : NancyModule
	{
		public IndexModule()
		{
#if !DEBUG
			this.RequiresHttps();
#endif
			Get["/"] = p =>
			{
				Console.WriteLine($"User: {Context.CurrentUser?.UserName}");
				if (Context.CurrentUser == null)
					return View["login"];
				else
					return View["index"];
			};
			
		}
	}
}
