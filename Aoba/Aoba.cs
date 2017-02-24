using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Flurl;
using Flurl.Http;
using LuminousVector.Aoba.DataStore;

namespace LuminousVector.Aoba
{
	public static class Aoba
	{
		public static Settings Settings { get { return _settings; } }

		private static string _apiUri = "https://aoba.luminousvector.com/api";
		private static string _authUri = "https://aoba.luminousvector.com/auth";
		private static Settings _settings;

		internal static void Init()
		{
			try
			{
				if (File.Exists("Settings.data"))
					_settings = Settings.Load("Settings.data");
				if (Settings.HasAuth)
				{
					try
					{
						var t = _apiUri.PostJsonAsync(new { apiKey = Settings.AuthToken }).ReceiveString();
						t.Wait();
					}
					catch
					{
						Settings.AuthToken = null;
					}
				}

			}
			catch
			{
				_settings = new Settings();
			}
#if DEBUG
			_apiUri = "http://localhost:4321/api";
			_authUri = "http://localhost:4321/auth";
#endif
		}

		internal async static Task Login()
		{
			var token = await _authUri.AppendPathSegment("login").PostJsonAsync(new { username = Settings.Username, password = Settings.Password, authMode = "API"}).ReceiveJson<Token>();
			Settings.AuthToken = token.ApiKey;
			var r = await _apiUri.PostJsonAsync(new { ApiKey = token });
			Console.WriteLine(r.StatusCode.ToString());
		}

		internal static void Save()
		{
			Settings.Save("Settings.data");
		}
	}
}
