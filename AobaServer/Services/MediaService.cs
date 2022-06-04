using AobaServer.Models;
using AobaServer.Utilz;

using Microsoft.AspNetCore.Mvc;

using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AobaServer.Services
{
	public class MediaService
	{
		private readonly GridFSBucket _gridFS;
		private readonly IMongoCollection<Media> _media;

		public MediaService(IMongoDatabase db, GridFSBucket gridFS)
		{
			_gridFS = gridFS;
			_media = db.GetCollection<Media>("media");
		}

		[RequestFormLimits(MultipartBodyLengthLimit = (long)1e9)]
		[RequestSizeLimit((long)1e9)]
		public async Task<ObjectId> UploadMedia(Stream data, string filename, ObjectId owner)
		{
			var id = ObjectId.GenerateNewId();
			if (string.IsNullOrWhiteSpace(filename))
				filename = $"{owner}_{id}";
			var ext = Path.GetExtension(filename);
			if(string.IsNullOrWhiteSpace(ext))
			{
				ext = data.GetImageExt();
				filename = $"{filename}{ext}";
			}
			var mediaId = await _gridFS.UploadFromStreamAsync(filename, data);
			var media = new Media
			{
				Ext = ext,
				Filename = filename,
				Id = id,
				MediaId = mediaId,
				MediaType = Media.GetMediaType(filename),
				ViewCount = 0,
				Owner = owner
			};

			await _media.InsertOneAsync(media);
			return media.Id;
		}

		public Task DeleteMediaAsync(ObjectId id)
		{
			return _media.DeleteOneAsync(m => m.Id == id);
		}

		public Task<Media> GetMedia(string legacyId)
		{
			return _media.Find(m => m.LegacyId == legacyId).FirstOrDefaultAsync();
		}

		public Task<Media> GetMedia(ObjectId id)
		{
			return _media.Find(m => m.Id == id).FirstOrDefaultAsync();
		}
	}
}
