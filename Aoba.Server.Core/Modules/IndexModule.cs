using System;
using Nancy;
using Nancy.Authentication.Stateless;
using Nancy.Security;
using System.Linq;
using LuminousVector.Aoba.Models;

namespace LuminousVector.Aoba.Server.Modules
{
	public class IndexModule : NancyModule
	{
		public IndexModule() : base("/")
		{
			StatelessAuthentication.Enable(this, AobaCore.StatelessConfig);
			//this.RequiresAuthentication();
			Get("/",  p =>
			{
				var curUser = ((UserModel)Context.CurrentUser)?.Username;
				if (curUser == null)
					return View["login.html"];
				Console.WriteLine($"User: {curUser}");
				return View["index"];
			});
		}
	}
}
