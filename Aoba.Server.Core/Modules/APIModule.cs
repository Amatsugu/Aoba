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
				var uid = ((UserModel)Context.CurrentUser).ID;
				return Response.AsJson(AobaCore.GetUserStats(uid)).WithHeader("Authorization", $"Bearer {AobaCore.GetJWT(AobaCore.GetApiKey(uid), 365)}");
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
					var uid = ((UserModel)Context.CurrentUser).ID;
					return Response.AsText(AobaCore.AddMedia(uid, media)).WithHeader("Authorization", $"Bearer {AobaCore.GetJWT(AobaCore.GetApiKey(uid), 365)}");
				}
				catch (Exception e)
				{
					Console.WriteLine(e.StackTrace);
					return new Response() { StatusCode = HttpStatusCode.ImATeapot };
				}
			});
		}
	}
}
