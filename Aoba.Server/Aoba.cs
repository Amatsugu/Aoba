using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Nancy.Authentication.Stateless;
using LuminousVector.Aoba.Server.Models;
using LuminousVector.Aoba.Server.DataStore;
using System.Security.Principal;
using System.Security.Claims;
using Nancy.Security;
using Nancy;
using Nancy.Extensions;
using System.Security.Cryptography;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using LuminousVector.Aoba.DataStore;
using LuminousVector.Aoba.Server.Credentials;

namespace LuminousVector.Aoba.Server
{
	public static class Aoba
	{
		public const string HOST = "aoba.luminousvector.com";

		
#if !DEBUG
		public const string BASE_DIR = "/Storage/Aoba";
#else
		public const string BASE_DIR = "K:/Aoba";
#endif
		public static string SCREEN_DIR { get { return $"{BASE_DIR}/Screenshots"; } }
		internal static StatelessAuthenticationConfiguration StatelessConfig = new StatelessAuthenticationConfiguration(nancyContext =>
		{
			string ApiKey = nancyContext.Request.Cookies.First(c => c.Key == "ApiKey").Value;
			Console.WriteLine($"API KEY: {ApiKey}");
			return GetUserFromApiKey(ApiKey);
		});
		private static string CONNECTION_STRING { get { return $"Host={HOST};Username={_dbUser};Password={_dbPass};Database={_dbName};Pooling=true"; } }
		private static string _dbUser, _dbPass, _dbName;

		public static void Init(string dbUser, string dbPass, string dbName)
		{
			_dbUser = dbUser;
			_dbPass = dbPass;
			_dbName = dbName;
		}


		private static string HashPassword(string password)
		{
			byte[] salt;
			new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

			var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
			byte[] hash = pbkdf2.GetBytes(20);

			byte[] hashBytes = new byte[36];
			Array.Copy(salt, 0, hashBytes, 0, 16);
			Array.Copy(hash, 0, hashBytes, 16, 20);

			return Convert.ToBase64String(hashBytes);
		}

		private static bool VerifyPassword(string password, string passwordHash)
		{
			if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(passwordHash))
				return false;
			/* Extract the bytes */
			byte[] hashBytes = Convert.FromBase64String(passwordHash);
			/* Get the salt */
			byte[] salt = new byte[16];
			Array.Copy(hashBytes, 0, salt, 0, 16);
			/* Compute the hash on the password the user entered */
			var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10000);
			byte[] hash = pbkdf2.GetBytes(20);
			/* Compare the results */
			for (int i = 0; i < 20; i++)
				if (hashBytes[i + 16] != hash[i])
					return false;
			return true;
		}

		/// <summary>
		/// Returns a new DB connection
		/// </summary>
		/// <returns></returns>
		internal static NpgsqlConnection GetConnection()
		{
			var con = new NpgsqlConnection(CONNECTION_STRING);
			con.Open();
			return con;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="apiKey"></param>
		/// <returns>The user that belongs to the current apiKey</returns>
		internal static UserModel GetUserFromApiKey(string apiKey)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					Console.WriteLine($"Key: {apiKey}");
					cmd.CommandText = $"SELECT username FROM {DBCredentials.DB_ApiTable} WHERE apikey = '{apiKey}'";
					string username = cmd.ExecuteScalar() as string;
					if (string.IsNullOrWhiteSpace(username))
						return null;
					else
						return new UserModel(username);
				}
			}
		}

		internal static string ValidateUser(LoginCredentialsModel user)
		{
			if (string.IsNullOrWhiteSpace(user.username) || string.IsNullOrWhiteSpace(user.password))
				return null;
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = $"SELECT password FROM {DBCredentials.DB_UserTable} WHERE username='{user.username}';";
					string passHash = cmd.ExecuteScalar() as string;
					if (VerifyPassword(user.password, passHash))
						return GetApiKey(user.username);
					else
						return null;
				}
			}
		}

		internal static string GetApiKey(string username)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					try
					{
						cmd.CommandText = $"SELECT apiKey FROM {DBCredentials.DB_ApiTable} WHERE username='{username}'";
						string apiKey = cmd.ExecuteScalar() as string;
						if (string.IsNullOrWhiteSpace(apiKey))
							return RegisterNewApiKey(username);
						else
							return apiKey;
					}
					catch
					{
						return null;
					}
				}
			}
		}

		internal static void RegisterUser(string username, string password)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = $"INSERT INTO {DBCredentials.DB_UserTable} VALUES('{username}', '{HashPassword(password)}', '{(username).ToBase60()}')";
					cmd.ExecuteNonQuery();
				}
			}
		}

		internal static void RemoveUser(string username)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = $"DELETE FROM {DBCredentials.DB_UserTable} WHERE username = '{username}'";
					cmd.ExecuteNonQuery();
				}
			}
		}

		internal static string RegisterNewApiKey(string user)
		{
			string id = Guid.NewGuid().ToString();
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = $"INSERT INTO {DBCredentials.DB_ApiTable} VALUES('{user}', '{id}')";
					cmd.ExecuteNonQuery();
				}
			}
			return id;
		}

		internal static string AddImage(string userName, string fileUri)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = $"INSERT INTO {DBCredentials.DB_MediaTable} VALUES('{userName.ToBase60()}{fileUri.ToBase60()}', '{userName}', '{Uri.EscapeDataString(fileUri)}')";
					cmd.ExecuteNonQuery();
					return $"{HOST}/i/{userName.ToBase60()}{fileUri.ToBase60()}";
				}
			}
		}

		internal static string GetImage(string id)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = $"SELECT fileuri FROM {DBCredentials.DB_MediaTable} WHERE id = '{id}'";
					return $"{SCREEN_DIR}{Uri.UnescapeDataString((string)cmd.ExecuteScalar())}";
				}
			}
		}

		internal static UserStatsModel GetUserStats(string userName)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					try
					{
						cmd.CommandText = $"SELECT COUNT(owner) FROM {DBCredentials.DB_MediaTable} WHERE owner = '{userName}'";
						return new UserStatsModel() { screenShotCount = (int)(long)cmd.ExecuteScalar()};
					}catch(Exception e)
					{
						return new UserStatsModel();
					}
				}
			}
		}
	}
}
