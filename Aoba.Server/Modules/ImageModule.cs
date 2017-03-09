using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using System.IO;

namespace LuminousVector.Aoba.Server.Modules
{
	public class ImageModule : NancyModule
	{
		public ImageModule() : base("/i")
		{
			Get["/{id}"] = p =>
			{
				string img = Aoba.GetImage((string)p.id);
				if (string.IsNullOrWhiteSpace(img))
					return new Response { StatusCode = HttpStatusCode.NotFound };
				return Response.FromStream(File.OpenRead(img), MimeTypes.GetMimeType(img));
			};

			Get["/"] = _ =>
			{
				return new Response { StatusCode = HttpStatusCode.NotFound };
			};
		}
	}
}
