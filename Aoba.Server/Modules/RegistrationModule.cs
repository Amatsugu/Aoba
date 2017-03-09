using System;
using Nancy;

namespace LuminousVector.Aoba.Server.Modules
{
	public class RegistrationModule : NancyModule
	{
		public RegistrationModule() : base("register")
		{
			Get["/{token}"] = p =>
			{
				if (Aoba.ValidateRegistrationToken((string)p.token))
					return View["register"];
				else
					return View["expired"];
			};
		}
	}
}
