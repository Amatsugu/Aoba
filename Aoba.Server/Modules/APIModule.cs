using System;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.Security;
using Nancy.Authentication.Stateless;
using LuminousVector.Aoba.Server.DataStore;
using LuminousVector.Aoba.Models;
using LuminousVector.Aoba.Server.Models;

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
				return Response.AsJson(Aoba.GetUserStats(((UserModel)Context.CurrentUser).ID));
			};

			Get["/"] = _ =>
			{
				return new Response { StatusCode = HttpStatusCode.OK };
			};

			Post["/image"] = p =>
			{
				if (!Directory.Exists(Aoba.MEDIA_DIR))
					Directory.CreateDirectory(Aoba.MEDIA_DIR);
				try
				{
					var f = Context.Request.Files.First();
					string fileNmae = $"{Aoba.GetNewID()}{Path.GetExtension(f.Name)}";
					Console.WriteLine($"File Recieved name:[{fileNmae}]");
					using (FileStream file = new FileStream($"{Aoba.MEDIA_DIR}/{fileNmae}", FileMode.CreateNew))
					{
						f.Value.CopyTo(file);
						file.Flush();
					}
					return Aoba.AddMedia(((UserModel)Context.CurrentUser).ID, new MediaModel($"/{fileNmae}", (MediaModel.MediaType)Enum.Parse(typeof(MediaModel.MediaType), f.ContentType)));
				}
				catch(Exception e)
				{
					Console.WriteLine($"Upload Failed: {e.Message}");
					return new Response() { StatusCode = HttpStatusCode.ImATeapot};
				}
			};
		}
	}
}
