using System;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Security;

namespace LuminousVector.Aoba.Server.Modules
{
	public class IndexModule : NancyModule
	{
		public IndexModule() : base("/")
		{
			StatelessAuthentication.Enable(this, Aoba.StatelessConfig);
			this.RequiresAuthentication();
			Get["/"] = p =>
			{
				Console.WriteLine($"User: {Context.CurrentUser?.UserName}");
				return View["index"];
			};
			
		}
	}
}
