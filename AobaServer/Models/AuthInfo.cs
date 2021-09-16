using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AobaServer.Models
{
	public class AuthInfo
	{
		public string Issuer;
		public string Audience;
		public byte[] SecureKey;

		/// <summary>
		/// Save this auth into in a json format to the sepcified file
		/// </summary>
		/// <param name="path">File path</param>
		/// <returns></returns>
		public AuthInfo Save(string path)
		{
			File.WriteAllText(path, JsonConvert.SerializeObject(this));
			return this;
		}

		/// <summary>
		/// Generate a new Auth Info with newly generated keys
		/// </summary>
		/// <param name="issuer"></param>
		/// <param name="audience"></param>
		/// <returns></returns>
		public static AuthInfo Create(string issuer, string audience)
		{
			var auth = new AuthInfo
			{
				Issuer = issuer,
				Audience = audience,
				SecureKey = GenetateJWTKey()
			};
			return auth;
		}

		/// <summary>
		/// Load auth info from a json file
		/// </summary>
		/// <param name="path">File path</param>
		/// <returns></returns>
		internal static AuthInfo Load(string path)
		{
			return JsonConvert.DeserializeObject<AuthInfo>(File.ReadAllText(path));
		}

		internal static AuthInfo LoadOrCreate(string path, string issuer, string audience)
		{
			if(File.Exists(path))
				return Load(path);
			else
			{
				var info = Create(issuer, audience);
				info.Save(path);
				return info;
			}
		}

		/// <summary>
		/// Generate a new key for use by JWT
		/// </summary>
		/// <returns></returns>
		public static byte[] GenetateJWTKey(int size = 64)
		{
			var key = new byte[size];
			using (var rng = new RNGCryptoServiceProvider())
			{
				rng.GetBytes(key);
			}
			return key;
		}
	}
}
