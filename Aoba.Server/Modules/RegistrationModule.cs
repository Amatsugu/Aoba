using System;
using Nancy;
using Nancy.ModelBinding;
using LuminousVector.Aoba.Server.Models;

namespace LuminousVector.Aoba.Server.Modules
{
	public class RegistrationModule : NancyModule
	{
		public RegistrationModule() : base("register")
		{
			Get["/{token}"] = p =>
			{
				var referer = Aoba.ValidateRegistrationToken((string)p.token);
				if (referer == null || referer == UserModel.Overlord)
					return new Response { StatusCode = HttpStatusCode.Unauthorized };
				else
					return View["register", new { referer = referer.UserName}];
			};

			Post["/{token}"] = p =>
			{
				var userInfo = this.Bind<LoginCredentialsModel>();
				if (Aoba.RegisterUser(userInfo, (string)p.token))
					return new Response { StatusCode = HttpStatusCode.OK };
				else
					return new Response { StatusCode = HttpStatusCode.Unauthorized };
			};
		}
	}
}
