using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Flurl.Http;
using Nancy.ModelBinding;
using Newtonsoft.Json;

namespace LuminousVector.Aoba.Server.Modules
{
	public class KaiseiLoginModule : NancyModule
	{
		public KaiseiLoginModule() : base ("/kaisei")
		{
			Get("/", (_) =>
			{
				return View["kaiseiConnect", new { callback = $"http://{AobaCore.HOST}/kaisei" }];
			});

			Post("/", (_) =>
			{
				var authId = this.Bind<Auth>();
				var kaiseiApi = "http://localhost:6130/app";
				var userId = $"{kaiseiApi}/sso/confirm".WithCookie("apiKey", "RaLz1XpKNE6di6Ux_icRRQ").PostJsonAsync(authId).GetAwaiter().GetResult().Content.ReadAsStringAsync().GetAwaiter().GetResult();
				//var userId = "QSYzd0mXAUCyrdXVHNf6jA";
				var user = $"{kaiseiApi}/user/{userId}".WithCookie("apiKey", "RaLz1XpKNE6di6Ux_icRRQ").GetJsonAsync().GetAwaiter().GetResult();

				return JsonConvert.SerializeObject(new
				{
					authId.authId,
					userId,
					user
				});
			});
		}

	}

	public struct Auth
	{
		public string authId;
	}
}
