using System;
using Nancy;
using Nancy.Security;

namespace LuminousVector.Aoba.Server.Modules
{
	public class IndexModule : NancyModule
	{
		public IndexModule() : base("/")
		{
			Get["/image/{id}"] = p => Response.AsRedirect($"/i/{(string)p.id}");

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
