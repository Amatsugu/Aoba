using System;
using Nancy;
using Nancy.Extensions;
using Nancy.Authentication.Forms;
using Nancy.ModelBinding;
using Nancy.ModelBinding.DefaultBodyDeserializers;
using Nancy.ModelBinding.DefaultConverters;
using LuminousVector.Aoba.Server.Models;
using LuminousVector.Aoba.Server.Credentials;

namespace LuminousVector.Aoba.Server.Modules
{
	public class AuthModule : NancyModule
	{
		public AuthModule() : base("/auth")
		{
			Post["/login"] = p =>
			{
				LoginCredentialsModel user = this.Bind<LoginCredentialsModel>();
				if (user == null)
					return new Response { StatusCode = HttpStatusCode.Unauthorized };
				string apiKey = Aoba.ValidateUser(user);
				if (apiKey == null)
					return new Response { StatusCode = HttpStatusCode.Unauthorized };
				if (user.AuthMode == AuthMode.API)
					return Response.AsJson(new { ApiKey = apiKey });
				else
					return new Response().WithCookie("ApiKey", apiKey);
			};

			Get["/logout"] = _ =>
			{
				return new Response().WithCookie("ApiKey", "");
			};

			Post["/register/{token}"] = p =>
			{
				LoginCredentialsModel user = this.Bind<LoginCredentialsModel>();
				if (Aoba.RegisterUser(user, (string)p.token))
				{
					return new Response { StatusCode = HttpStatusCode.OK };
				}
				else
					return new Response { StatusCode = HttpStatusCode.Unauthorized };
			};

			Post["/checkuser"] = p =>
			{
				return (Aoba.CheckUserExists(Request.Body.AsString())) ? new Response { StatusCode = HttpStatusCode.NotAcceptable } : new Response { StatusCode = HttpStatusCode.OK };
			};


		}
	}
}
