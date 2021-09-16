using MongoDB.Bson;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AobaServer.Utilz
{
	public static class Extensions
	{
		public static ObjectId ToObjectId(this string idString)
		{
			if (ObjectId.TryParse(idString, out var id))
				return id;
			return default;
		}

		public static ObjectId GetId(this ClaimsPrincipal user)
		{
			return user.FindFirstValue(ClaimTypes.NameIdentifier).ToObjectId();
		}

		public static string GetImageExt(this Stream media)
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
