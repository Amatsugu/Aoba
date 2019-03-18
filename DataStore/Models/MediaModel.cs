using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace LuminousVector.Aoba.Models
{
    public class MediaModel
    {
		public enum MediaType
		{
			Image,
			Audio,
			Video,
			Text,
			Code,
			Raw
		}

		public string id;
		public MediaType type;
		public Stream mediaStream;
		public string fileName;
		public string Ext => Path.GetExtension(fileName);
		public string mediaId;


		private static readonly Dictionary<string, MediaType> AllowedFiles = new Dictionary<string, MediaType>()
		{
			{ ".jpg", MediaType.Image },
			{ ".png", MediaType.Image },
			{ ".ico", MediaType.Image },
			{ ".gif", MediaType.Image },
			{ ".mp3", MediaType.Audio },
			{ ".flac", MediaType.Audio },
			{ ".alac", MediaType.Audio },
			{ ".mp4", MediaType.Video },
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


		private static string _filterString;

		public static string GetFilterString()
		{
			if (_filterString != null)
				return _filterString;
			else
			{
				for (int i = 0; i < 5; i++)
				{
					MediaType t = (MediaType)i;
					var exts = (from e in AllowedFiles where e.Value == t select e.Key);
					string extChain = "";
					foreach (string s in exts)
					{
						extChain += $"*{s}{(exts.Last() == s ? "" : ";")}";
					}
					string filter = $"{t} ({extChain})|{extChain}";
					if (_filterString == null)
						_filterString = filter;
					else
						_filterString += $"|{filter}";
				}
				_filterString = $"All files (*.*)|*.*|{_filterString}";
				return _filterString;
			}
		}

		public static MediaType GetMediaType(string file)
		{
			string ext = Path.GetExtension(file);
			if (AllowedFiles.ContainsKey(ext))
				return AllowedFiles[ext];
			else
				return MediaType.Raw;
		}

	}
}
