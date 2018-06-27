using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using static LuminousVector.Aoba.Models.MediaModel;
using Nancy.Responses;
using LuminousVector.Aoba.Server.DataStore;

namespace LuminousVector.Aoba.Server.Modules
{
	public class MediaModule : NancyModule
	{
		public MediaModule() : base("/i")
		{
			Get["/{id}"] = p =>
			{
				var media = AobaCore.GetMedia((string)p.id);
				if(media == null)
					return new NotFoundResponse();
				else
				{
					if (media.mediaStream == null || media.mediaStream.Length <= 0)
						return new NotFoundResponse();
					string ext = media.ext;
					AobaCore.IncrementViewCount(media.id);
					switch (media.type)
					{
						//Image
						case MediaType.Image:
							return Response.FromStream(media.mediaStream, MimeTypes.GetMimeType(ext));
						//Text
						case MediaType.Text:
							return Response.FromStream(media.mediaStream, "text/plain");
						//Code
						case MediaType.Code:
							return Response.FromStream(media.mediaStream, "text/plain"); // TODO: Code View
							//return View["code.cshtml", new { code = File.ReadAllText(uri) }];
						//Audio
						case MediaType.Audio:
							try
							{
								var file = TagLib.File.Create(new FileStreamAbstraction($"{media.id}{ext}", media.mediaStream));
								return View["audio.cshtml", new { p.id, rawUri = $"/i/raw/{(string)p.id}{ext}", format = ext, title = file.Tag.Title, artist = (file.Tag.FirstPerformer ?? file.Tag.AlbumArtists.First()), album = file.Tag.Album }];
							}
							catch(TagLib.UnsupportedFormatException)
							{
								return View["audio.cshtml", new { p.id, rawUri = $"/i/raw/{(string)p.id}{ext}", format = ext }];
							}
						//Video
						case MediaType.Video:
							return Response.FromStream(media.mediaStream, MimeTypes.GetMimeType(media.ext)); // TODO: Video player
							//return View["video.cshtml", new { rawUri = $"/i/raw/{(string)p.id}{ext}", format = Path.GetExtension(media.uri).ToLower() }];
						//Raw
						default:
							return Response.AsRedirect($"/i/{(string)p.id}/raw");
					}
				}
			};

			Get["/{id}/og"] = p =>
			{
				var media = AobaCore.GetMedia((string)p.id);
				if (media == null)
					return new NotFoundResponse();
				else
				{
					if (media.mediaStream == null || media.mediaStream.Length <= 0)
						return new NotFoundResponse();
					string ext = media.ext;
					switch (media.type)
					{
						//Image
						case MediaType.Image:
							return Response.FromStream(media.mediaStream, MimeTypes.GetMimeType(ext));
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
								var file = TagLib.File.Create(new FileStreamAbstraction($"{media.id}{ext}", media.mediaStream));
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
				var media = AobaCore.GetMedia((string)p.id);
				if (media == null)
					return new NotFoundResponse();
				else
					return Response.FromStream(media.mediaStream, MimeTypes.GetMimeType(media.ext));
			};

			//Get["/"] = _ =>
			//{
				//return new NotFoundResponse();
			//};
		}
	}
}
