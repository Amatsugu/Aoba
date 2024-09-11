using AobaServer.Models;
using AobaServer.Services;
using AobaServer.Utilz;

using Microsoft.AspNetCore.Mvc;

using MimeTypes;

using MongoDB.Driver.GridFS;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AobaServer.Controllers
{
	[Route("i")]
	public class MediaController : Controller
	{
		private readonly MediaService _media;
		private readonly GridFSBucket _gridFS;

		public MediaController(MediaService media, GridFSBucket gridFS)
		{
			_media = media;
			_gridFS = gridFS;
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> IndexAsync(string id)
		{
			var imgId = id.ToObjectId();
			var media = imgId == default ? await _media.GetMedia(id) : await _media.GetMedia(imgId);
			if (media == null)
				return NotFound();
			await _media.IncrementView(media.Id);
			var file = await _gridFS.OpenDownloadStreamAsync(media.MediaId);
			if (!MimeTypeMap.TryGetMimeType(media.Ext, out var mimeType))
				mimeType = "application/octet-stream";
			return File(file, mimeType);
			//return media.MediaType switch
			//{
			//	MediaType.Image => File(file, media.Ext == ".gif" ? "image/gif" : "image/png"),
			//	MediaType.Text => File(file, "text"),
			//	MediaType.Code => File(file, "text"),
			//	MediaType.Video => File(file, "video"),
			//	_ => File(file, "application/octet-stream")
			//};
		}

		[HttpGet("dl/{id}")]
		[HttpGet("dl/{id}/{filename}")]
		[HttpGet("{id}/raw/{filename?}")]
		public async Task<IActionResult> Download(string id)
		{
			var imgId = id.ToObjectId();
			var media = imgId == default ? await _media.GetMedia(id) : await _media.GetMedia(imgId);
			if (media == null)
				return NotFound();
			await _media.IncrementView(media.Id);
			var file = await _gridFS.OpenDownloadStreamAsync(media.MediaId);
			return File(file, "application/octet-stream");
		}
	}
}
