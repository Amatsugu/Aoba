using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AobaServer.Models
{
	/*
	 {
		"Version": "13.1.0",
		"Name": "Aoba",
		"DestinationType": "ImageUploader, TextUploader, FileUploader",
		"RequestMethod": "POST",
		"RequestURL": "https://aoba.app/api/image",
		"Headers": {
		"Authorization": "Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9hdXRoZW50aWNhdGlvbiI6IjM3NzRhYjA5LTQ0OWQtNDMwMy04OTI3LTg3YTRjNjI1NzIzMyIsIm5iZiI6MTYzMTE1MDY4OCwiZXhwIjoxNjMxNzU1NDg4LCJpYXQiOjE2MzExNTA2ODgsImlzcyI6IkFvYmEuYXBwIiwiYXVkIjoiQW9iYSJ9.3mUgnEaQNKh1giQiWQHT3bKZCq0swktzUTGoOAIlKmM",
		"Accept-Encoding": "JSON"
		},
		"Body": "MultipartFormData",
		"Arguments": {
		"name": "$filename$"
		},
		"FileFormName": "file",
		"RegexList": [
		"([^/]+)/?$"
		],
		"URL": "https://$json:url$",
		"ThumbnailURL": "https://aoba.app/i/$json:id$/og",
		"DeletionURL": "https://aoba.app/api/image/$json:id$"
	} 
	 */

	public class ShareXDestination
	{
		public string Version { get; set; } = "13.1.0";
		public string Name { get; set; } = "Aoba";
		public string DestinationType { get; set; } = "ImageUploader, TextUploader, FileUploader";
		public string RequestMethod { get; set; } = "POST";
		public string RequestURL { get; set; } = "https://aoba.app/api/image";
		public Dictionary<string, string> Headers { get; set; } = new();
		public string Body { get; set; } = "MultipartFormData";
		public Dictionary<string, string> Arguments { get; set; } = new() { {"name", "$filename$" } };
		public string FileFormName { get; set; } = "file";
		public string[] RegexList { get; set; } = new[] { "([^/]+)/?$" };
		public string URL { get; set; } = "https://aoba.app/i/$json:id$";
		public string ThumbnailURL { get; set; } 
		public string DeletionURL { get; set; }
	}

}
