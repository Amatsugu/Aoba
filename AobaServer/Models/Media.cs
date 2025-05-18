using MongoDB.Bson;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AobaServer.Models
{
	public class Media
	{
		public ObjectId Id { get; set; }
		public string LegacyId { get; set; }
		public ObjectId MediaId { get; set; }
		public string Filename { get; set; }
		public MediaType MediaType { get; set; }
		public string Ext { get; set; }
		public int ViewCount { get; set; }
		public ObjectId Owner { get; set; }
		public DateTime UploadDate { get; set; }


		public static readonly Dictionary<string, MediaType> KnownTypes = new()
		{
			{ ".jpg", MediaType.Image },
			{ ".avif", MediaType.Image },
			{ ".jpeg", MediaType.Image },
			{ ".png", MediaType.Image },
			{ ".apng", MediaType.Image },
			{ ".webp", MediaType.Image },
			{ ".ico", MediaType.Image },
			{ ".gif", MediaType.Image },
			{ ".mp3", MediaType.Audio },
			{ ".flac", MediaType.Audio },
			{ ".alac", MediaType.Audio },
			{ ".mp4", MediaType.Video },
			{ ".webm", MediaType.Video },
			{ ".mov", MediaType.Video },
			{ ".avi", MediaType.Video },
			{ ".mkv", MediaType.Video },
			{ ".txt", MediaType.Text },
			{ ".log", MediaType.Text },
			{ ".css", MediaType.Code },
			{ ".cs", MediaType.Code },
			{ ".cpp", MediaType.Code },
			{ ".lua", MediaType.Code },
			{ ".js", MediaType.Code },
			{ ".htm", MediaType.Code },
			{ ".html", MediaType.Code },
			{ ".cshtml", MediaType.Code },
			{ ".xml", MediaType.Code },
			{ ".json", MediaType.Code },
			{ ".py", MediaType.Code },
		};

		public string GetUrlString()
		{
			var fn = Path.GetFileNameWithoutExtension(Filename);
			fn = Uri.EscapeDataString(fn);

			return this switch
			{
				//Media { Ext: ".gif"} => $"/i/dl/{Id}/{fn}{Ext}",
				Media { MediaType: MediaType.Raw } => $"/i/dl/{Id}/{fn}{Ext}",
				Media { MediaType: MediaType.Text } => $"/i/dl/{Id}/{fn}{Ext}",
				Media { MediaType: MediaType.Code } => $"/i/dl/{Id}/{fn}{Ext}",
				//Media { MediaType: MediaType.Video } => $"/i/dl/{Id}/{fn}{Ext}",
				_ => $"/i/{Id}"
			};
		}

		public static MediaType GetMediaType(string filename)
		{
			string ext = Path.GetExtension(filename);
			if (KnownTypes.ContainsKey(ext))
				return KnownTypes[ext];
			else
				return MediaType.Raw;
		}
	}

	public enum MediaType
	{
		Image,
		Audio,
		Video,
		Text,
		Code,
		Raw
	}
}
