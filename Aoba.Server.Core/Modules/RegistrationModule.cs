using System;
using Nancy;
using Nancy.ModelBinding;
using LuminousVector.Aoba.Models;

namespace LuminousVector.Aoba.Server.Modules
{
	public class RegistrationModule : NancyModule
	{
		public RegistrationModule() : base("register")
		{
			Get("/{token}", p =>
			{
				var referer = AobaCore.ValidateRegistrationToken(((string)p.token).Replace(' ', '+'));
				if (referer == null || referer == UserModel.Overlord)
					return new Response { StatusCode = HttpStatusCode.Unauthorized };
				else
					return View["register", new { referer = referer.UserName}];
			});

			Post("/{token}", p =>
			{
				var userInfo = this.Bind<LoginCredentialsModel>();
				var token = (string)p.token;
				if (!string.IsNullOrWhiteSpace(token) && AobaCore.RegisterUser(userInfo, token.Replace(' ', '+')))
				{
					return new Response { StatusCode = HttpStatusCode.OK };
				}
				else
					return new Response { StatusCode = HttpStatusCode.Unauthorized };
			});
		}
	}
}
