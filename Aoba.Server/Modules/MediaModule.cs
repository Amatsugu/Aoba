using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using static LuminousVector.Aoba.Models.MediaModel;
using Nancy.Responses;

namespace LuminousVector.Aoba.Server.Modules
{
	public class MediaModule : NancyModule
	{
		public MediaModule() : base("/i")
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
							try
							{
								var file = TagLib.File.Create(uri);
								return View["audio.cshtml", new { id = p.id, rawUri = $"/i/raw/{(string)p.id}{ext}", format = ext, title = file.Tag.Title, artist = (file.Tag.FirstPerformer ?? file.Tag.AlbumArtists.First()), album = file.Tag.Album }];
							}
							catch(TagLib.UnsupportedFormatException)
							{
								return View["audio.cshtml", new { id = p.id, rawUri = $"/i/raw/{(string)p.id}{ext}", format = ext }];
							}
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

			Get["/{id}/og"] = p =>
			{
				var media = Aoba.GetMedia((string)p.id);
				if (media == null)
				{
					return new Response { StatusCode = HttpStatusCode.NotFound };
				}
				else
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
							return new NotFoundResponse();
						//Code
						case MediaType.Code:
							return new NotFoundResponse();
						//Audio
						case MediaType.Audio:
							try
							{
								var file = TagLib.File.Create(uri);
								if (file.Tag.Pictures.Length == 0)
									return new NotFoundResponse();
								return Response.FromStream(new MemoryStream(file.Tag.Pictures.First().Data.Data), "image/png");
							}
							catch (TagLib.UnsupportedFormatException)
							{
								return new NotFoundResponse();
							}
						//Video
						case MediaType.Video:
							return new NotFoundResponse();
						default:
							return Response.AsRedirect($"/i/raw/{(string)p.id}{ext}");
					}
				}
			};

			Get["/raw/{id}.{ext}"] = p =>
			{
				var media = Aoba.GetMedia((string)p.id);
				string uri = $"{Aoba.MEDIA_DIR}{media.uri}";
				if (media == null)
				{
					return new NotFoundResponse();
				}
				else
				{
					return Response.FromStream(File.OpenRead(uri), MimeTypes.GetMimeType(uri));
				}
			};

			Get["/"] = _ =>
			{
				return new NotFoundResponse();
			};
		}
	}
}
