using System;
using System.IO;
using System.Linq;
using Nancy;
using Nancy.Security;
using Nancy.Authentication.Stateless;
using LuminousVector.Aoba.Server.DataStore;
using LuminousVector.Aoba.Models;
using LuminousVector.Aoba.Server.Models;
using Nancy.Responses;

namespace LuminousVector.Aoba.Server.Modules
{
	public class APIModule : NancyModule
	{
		public APIModule() : base("/api")
		{
			StatelessAuthentication.Enable(this, AobaCore.StatelessConfig);
			Before.AddItemToEndOfPipeline(ctx =>
			{ 
				return (this.Context.CurrentUser == null) ? new HtmlResponse(HttpStatusCode.Unauthorized) : null;
			});

			Get("/userStats", _ =>
			{
				return Response.AsJson(AobaCore.GetUserStats(((UserModel)Context.CurrentUser).ID));
			});

			Get("/", _ =>
			{
				return new Response { StatusCode = HttpStatusCode.OK };
			});

			Post("/image", p =>
			{
				try
				{
					var f = Context.Request.Files.First();
					//using (FileStream file = new FileStream($"{AobaCore.MEDIA_DIR}/{fileNmae}", FileMode.CreateNew))
					//{
						//f.Value.CopyTo(file);
						//file.Flush();
					//}
					var media = new MediaModel
					{
						//uri = $"/{fileNmae}",
						type = MediaModel.GetMediaType(f.Name),
						mediaStream = f.Value,
						fileName = f.Name
						//Ext = Path.GetExtension(fileName)
					};
					media.mediaStream.Position = 0;
					//f.Value.Read(media.media, 0, (int)f.Value.Length);
					return AobaCore.AddMedia(((UserModel)Context.CurrentUser).ID, media);
				}
				catch(Exception e)
				{
					Console.WriteLine(e.StackTrace);
					return new Response() { StatusCode = HttpStatusCode.ImATeapot};
				}
			});
		}
	}
}
