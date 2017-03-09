using System;
using Nancy;
using Nancy.Authentication.Forms;
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
				if (user.authMode == AuthMode.API)
					return string.IsNullOrEmpty(apiKey) ? new Response { StatusCode = HttpStatusCode.Unauthorized } : Response.AsJson(new { ApiKey = apiKey });
				else
					return this.LoginWithoutRedirect(new Guid(apiKey));
			};

			Get["/logout"] = _ =>
			{
				return this.LogoutWithoutRedirect();
			};


		}
	}
}
