using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;

namespace LuminousVector.Aoba.Server.Models
{
	public static class StatusCodeMessages
	{
		public static string GetMessage(HttpStatusCode statusCode)
		{
			if (messages.ContainsKey(statusCode))
			{
				var m = messages[statusCode];
				return m[rand.Next(m.Length-1)];
			}
			else
				return statusCode.ToString();
		}

		private static Random rand = new Random();

		private static Dictionary<HttpStatusCode, string[]> messages = new Dictionary<HttpStatusCode, string[]>()
		{
			{
				HttpStatusCode.NotFound,
				new string[] 
				{
					"Nothing here...",
					"I couldn't find anything"
				}
			},
			{
				HttpStatusCode.Unauthorized,
				new string[]
				{
					"You shouldn't be here..."
				}
			}
		};

	}
}
