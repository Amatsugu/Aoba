using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using LuminousVector.Aoba.Server.Models;

namespace LuminousVector.Aoba.Server.Modules
{
	public class AuthModule : NancyModule
	{
		public AuthModule() : base("/auth")
		{
			Post["/login"] = p =>
			{
				LoginCredentialsModel user = this.Bind<LoginCredentialsModel>();
				string apiKey = Aoba.ValidateUser(user);
				if (user.authMode == "API")
					return string.IsNullOrEmpty(apiKey) ? new Response { StatusCode = HttpStatusCode.Unauthorized } : Response.AsJson(new { ApiKey = apiKey });
				else
					return new Response { StatusCode = HttpStatusCode.NoResponse }; //string.IsNullOrEmpty(apiKey) ? new Response { StatusCode = HttpStatusCode.Unauthorized } : new Response { StatusCode = HttpStatusCode.Accepted };
			};

			Get["/logout"] = _ =>
			{

				return null;
			};


		}
	}
}
