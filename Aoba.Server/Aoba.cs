using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Npgsql;
using Nancy.Authentication.Stateless;
using LuminousVector.Aoba.Server.Models;
using LuminousVector.Aoba.Server.DataStore;
using Nancy.Security;
using Nancy.Extensions;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace LuminousVector.Aoba.Server
{
	public static class Aoba
	{
		public const string HOST = "karuta.luminousvector.com";
		internal static StatelessAuthenticationConfiguration StatelessConfig = new StatelessAuthenticationConfiguration(nancyContext =>
		{
			
			var apiKey = JsonConvert.DeserializeObject<ApiKeyModel>(nancyContext.Request.Body.AsString()).ApiKey;


			return GetUserFromApiKey(apiKey);
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

		internal static UserModel GetUserFromApiKey(string apiKey)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					Console.WriteLine($"Key: {apiKey}");
					cmd.CommandText = $"SELECT username FROM apiKeys WHERE apiKey = {apiKey}";
					string username = (string)cmd.ExecuteScalar();
					
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
					cmd.CommandText = $"SELECT password FROM users WHERE username='{user.username}';";
					using (var reader = cmd.ExecuteReader())
					{
						if (!reader.HasRows)
							return null;
						reader.Read();
						string passHash = reader.GetString(0);
						if (VerifyPassword(user.password, passHash))
						{
							return GetApiKey(user.username);
						}
						else
							return null;
					}
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
						cmd.CommandText = $"SELECT apiKey FROM apiKeys WHERE username='{username.ToBase60()}'";
						using (var reader = cmd.ExecuteReader())
						{
							if (!reader.HasRows)
								return RegisterNewApiKey(username);
							reader.Read();
							string apiKey = reader.GetString(0);
							if (string.IsNullOrWhiteSpace(apiKey))
								return RegisterNewApiKey(username);
							else
								return apiKey;
						}
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
					cmd.CommandText = $"INSERT INTO users VALUES('{username}', '{HashPassword(password)}', '{(username).ToBase60()}')";
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
					cmd.CommandText = $"DELETE FROM users WHERE username = '{username}'";
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
					cmd.CommandText = $"INSERT INTO apiKeys VALUES('{user.ToBase60()}', '{id}')";
					cmd.ExecuteNonQuery();
				}
			}
			return id;
		}
	}
}
