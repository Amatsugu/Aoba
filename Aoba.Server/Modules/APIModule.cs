using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Security;
using LuminousVector.Aoba.Server.Models;
using Nancy.ModelBinding;
using Nancy.Authentication.Stateless;

namespace LuminousVector.Aoba.Server.Modules
{
	public class APIModule : NancyModule
	{
		public APIModule() : base("/api")
		{
			StatelessAuthentication.Enable(this, Aoba.StatelessConfig);
			this.RequiresAuthentication();

			Post["/"] = _ =>
			{
				Console.WriteLine(Context.CurrentUser.IsAuthenticated());
				return new Response { StatusCode = HttpStatusCode.OK };
			};

			Put["/image"] = p =>
			{
				var id = Context.CurrentUser;

				var userModel = new UserModel(id.UserName);

				return null;
			};
		}
	}
}
