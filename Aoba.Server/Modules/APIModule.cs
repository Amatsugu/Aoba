using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Security;
using LuminousVector.Aoba.Server.DataStore;
using LuminousVector.Aoba.Server.Models;
using Nancy.ModelBinding;
using Nancy.Authentication.Stateless;
using System.IO;

namespace LuminousVector.Aoba.Server.Modules
{
	public class APIModule : NancyModule
	{
		public APIModule() : base("/api")
		{
			StatelessAuthentication.Enable(this, Aoba.StatelessConfig);
			this.RequiresAuthentication();

			Get["/userStats"] = _ =>
			{
				return Response.AsJson(Aoba.GetUserStats(Context.CurrentUser.UserName));
			};

			Get["/"] = _ =>
			{
				return new Response { StatusCode = HttpStatusCode.OK };
			};

			Post["/image"] = p =>
			{
				if (!Directory.Exists(Aoba.SCREEN_DIR))
					Directory.CreateDirectory(Aoba.SCREEN_DIR);
				string id = null; 
				try
				{
					var f = Context.Request.Files.First();
					id = f.Name.ToBase60();
					Console.WriteLine($"File Recieved name:[{f.Name}]");
					using (FileStream file = new FileStream($"{Aoba.SCREEN_DIR}/{f.Name}", FileMode.CreateNew))
					{
						f.Value.CopyTo(file);
						file.Flush();
					}
					return Aoba.AddImage(Context.CurrentUser.UserName, $"/{f.Name}");
				}
				catch(Exception e)
				{
					Console.WriteLine($"Upload Failed: {e.Message}");
					return new Response() { StatusCode = HttpStatusCode.InternalServerError};
				}
			};
		}
	}
}
