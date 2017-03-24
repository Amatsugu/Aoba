using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using System.IO;
using static LuminousVector.Aoba.Models.MediaModel;
using Nancy.Responses;

namespace LuminousVector.Aoba.Server.Modules
{
	public class ImageModule : NancyModule
	{
		public ImageModule() : base("/i")
		{
			Get["/{id}"] = p =>
			{
				var media = Aoba.GetMedia((string)p.id);
				if(media == null)
				{
					return new Response { StatusCode = HttpStatusCode.NotFound };
				}else
				{
					if (string.IsNullOrWhiteSpace(media.uri))
						return new Response { StatusCode = HttpStatusCode.NotFound };
					string uri = $"{Aoba.MEDIA_DIR}{media.uri}";
					string ext = Path.GetExtension(media.uri).ToLower();
					switch (media.type)
					{
						//Image
						case MediaType.Image:
							return Response.FromStream(File.OpenRead(uri), MimeTypes.GetMimeType(uri));
						//Text
						case MediaType.Text:
							return Response.FromStream(File.OpenRead(uri), "text/plain");
						//Code
						case MediaType.Code:
							return Response.FromStream(File.OpenRead(uri), "text/plain"); // TODO: Code View
							//return View["code.cshtml", new { code = File.ReadAllText(uri) }];
						//Audio
						case MediaType.Audio:
							return View["audio.cshtml", new { rawUri = $"/i/raw/{(string)p.id}{ext}", format = ext }];
						//Video
						case MediaType.Video:
							return Response.FromStream(File.OpenRead(uri), MimeTypes.GetMimeType(media.uri)); // TODO: Video player
							//return View["video.cshtml", new { rawUri = $"/i/raw/{(string)p.id}{ext}", format = Path.GetExtension(media.uri).ToLower() }];
						//Raw
						default:
							return Response.AsRedirect($"/i/{(string)p.id}/raw");
					}
				}
			};

			Get["/raw/{id}.{ext}"] = p =>
			{
				var media = Aoba.GetMedia((string)p.id);
				string uri = $"{Aoba.MEDIA_DIR}{media.uri}";
				if (media == null)
				{
					return new Response { StatusCode = HttpStatusCode.NotFound };
				}
				else
				{
					return Response.FromStream(File.OpenRead(uri), MimeTypes.GetMimeType(uri));
				}
			};

			Get["/"] = _ =>
			{
				return new Response { StatusCode = HttpStatusCode.NotFound };
			};
		}
	}
}
