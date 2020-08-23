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
using System.Text;

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
					var media = new MediaModel
					{
						type = MediaModel.GetMediaType(f.Name),
						mediaStream = f.Value,
						fileName = f.Name
					};

					media.mediaStream.Position = 0;
					if (string.IsNullOrEmpty(media.Ext))
					{
						var ext = GetImageExt(media.mediaStream);
						media.fileName = $"{media.fileName}{ext}";
						media.type = MediaModel.GetMediaType(ext);
					}
					var uid = ((UserModel)Context.CurrentUser).ID;
					AobaCore.AddMedia(uid, media);
					var response = string.Empty;
					if (media.type == MediaModel.MediaType.Raw)
						response = $"{AobaCore.HOST}/i/raw/{media.id}/{media.fileName}";
					else if (media.Ext == ".gif")
						response =$"{AobaCore.HOST}/i/raw/{media.id}/{media.fileName}";
					else
						response = $"{AobaCore.HOST}/i/{media.id}";
					if(Context.Request.Headers.AcceptEncoding.Contains("JSON"))
					{
						return Response.AsJson(new
						{
							id = media.id,
							url = response
						});
					}
					return Response.AsText(response).WithHeader("Authorization", $"Bearer {AobaCore.GetJWT(AobaCore.GetApiKey(uid), 365)}");
				}
				catch (Exception e)
				{
					Console.WriteLine(e.StackTrace);
					return new Response() { StatusCode = HttpStatusCode.ImATeapot };
				}
			});

			Delete("image/{id}", p =>
			{
				AobaCore.DeleteImage(p.id);
				return new Response() { StatusCode = HttpStatusCode.OK };
			});
		}

		private string GetImageExt(Stream media)
		{
			var headerBytes = new byte[16];
			media.Read(headerBytes, 0, 16);
			media.Position = 0;
			// see http://www.mikekunz.com/image_file_header.html  
			var bmp = Encoding.ASCII.GetBytes("BM");     // BMP
			var gif = Encoding.ASCII.GetBytes("GIF");    // GIF
			var png = new byte[] { 137, 80, 78, 71 };    // PNG
			var png2 = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }; //PNG
			var tiff = new byte[] { 73, 73, 42 };         // TIFF
			var tiff2 = new byte[] { 77, 77, 42 };         // TIFF
			var jpeg = new byte[] { 255, 216, 255, 224 }; // jpeg
			var jpeg2 = new byte[] { 255, 216, 255, 225 }; // jpeg canon
			if (bmp.SequenceEqual(headerBytes.Take(bmp.Length)))
				return ".bmp";
			if (gif.SequenceEqual(headerBytes.Take(gif.Length)))
				return ".gif";
			if (png.SequenceEqual(headerBytes.Take(png.Length)))
				return ".png";
			if (png2.SequenceEqual(headerBytes.Take(png2.Length)))
				return ".png";
			if (tiff.SequenceEqual(headerBytes.Take(tiff.Length)))
				return ".tiff";
			if (tiff2.SequenceEqual(headerBytes.Take(tiff2.Length)))
				return ".tiff";
			if (jpeg.SequenceEqual(headerBytes.Take(jpeg.Length)))
				return ".jpeg";
			if (jpeg2.SequenceEqual(headerBytes.Take(jpeg2.Length)))
				return ".jpeg";
			return string.Empty;
		}
	}
}
