using System;
using System.Linq;
using System.Security.Cryptography;
using Nancy.Authentication.Stateless;
using Npgsql;
using LuminousVector.Aoba.Server.Models;
using LuminousVector.Aoba.DataStore;
using LuminousVector.Aoba.Server.Credentials;
using LuminousVector.Aoba.Models;

namespace LuminousVector.Aoba.Server
{
	public static class Aoba
	{
		public const string HOST = "aobacapture.com";
#if !DEBUG
		public const string BASE_DIR = "/Storage/Aoba";
#else
		public const string BASE_DIR = "K:/Aoba";
#endif
		public static string MEDIA_DIR { get { return $"{BASE_DIR}/Media"; } }

		internal static StatelessAuthenticationConfiguration StatelessConfig { get; private set; } = new StatelessAuthenticationConfiguration(nancyContext =>
		{
			try
			{
				string ApiKey = nancyContext.Request.Cookies.First(c => c.Key == "ApiKey").Value;
				Console.WriteLine($"API KEY: {ApiKey}");
				return GetUserFromApiKey(ApiKey);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				return null;
			}
		});

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
			var con = new NpgsqlConnection(DBCredentials.CONNECTION_STRING);
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
					try
					{

						Console.WriteLine($"Key: {apiKey}");
						cmd.CommandText = $"SELECT userid FROM {DBCredentials.ApiTable} WHERE apikey = '{apiKey}'";
						string userid = (string)cmd.ExecuteScalar();
						if (string.IsNullOrWhiteSpace(userid))
							return null;
						else
						{
							cmd.CommandText = $"SELECT username FROM {DBCredentials.UserTable} WHERE id = '{userid}'";
							return new UserModel((string)cmd.ExecuteScalar(), userid);
						}
					}catch(Exception e)
					{
						Console.WriteLine("Auth Failed");
						Console.WriteLine(e.Message);
						Console.WriteLine(e.StackTrace);
						return null;
					}
				}
			}
		}

		internal static bool CheckUserExists(string username)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = $"SELECT username FROM {DBCredentials.UserTable} WHERE username = '{Uri.EscapeDataString(username)}'";
					return cmd.ExecuteReader().HasRows;
				}
			}
		}

		internal static UserModel GetUserFromID(string userid)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					try
					{
						cmd.CommandText = $"SELECT username, claims FROM {DBCredentials.UserTable} WHERE id = '{userid}';";
						using (var reader = cmd.ExecuteReader())
						{
							if (!reader.HasRows)
								return null;
							reader.Read();
							string username = reader.GetString(0);
							string[] claims = reader.GetValue(1) as string[];
							return new UserModel(Uri.UnescapeDataString(username), userid, claims);
						}
					}
					catch (Exception e)
					{
						Console.WriteLine(e.Message);
						return null;
					}
				}
			}
		}

		internal static string ValidateUser(LoginCredentialsModel user)
		{
			if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Password))
				return null;
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					try
					{
						cmd.CommandText = $"SELECT password FROM {DBCredentials.UserTable} WHERE username = '{user.Username}';";
						string passHash = (string)cmd.ExecuteScalar();
						if (!VerifyPassword(user.Password, passHash))
							return null;
						cmd.CommandText = $"SELECT id FROM {DBCredentials.UserTable} WHERE password = '{passHash}'";
						return GetApiKey((string)cmd.ExecuteScalar());
					}catch(Exception e)
					{
						Console.WriteLine(e.Message);
						return null;
					}
				}
			}
		}

		internal static string GetApiKey(string userid)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					try
					{
						cmd.CommandText = $"SELECT apiKey FROM {DBCredentials.ApiTable} WHERE userid='{userid}'";
						string apiKey = cmd.ExecuteScalar() as string;
						if (string.IsNullOrWhiteSpace(apiKey))
							return RegisterNewApiKey(userid);
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

		internal static bool RegisterUser(LoginCredentialsModel user, string token = null)
		{
			var referer = ValidateRegistrationToken(token);
			if (referer != null)
			{
				try
				{
					using (var con = GetConnection())
					{
						using (var cmd = con.CreateCommand())
						{
							cmd.CommandText = $"INSERT INTO {DBCredentials.UserTable} VALUES('{Uri.EscapeDataString(user.Username)}', '{HashPassword(user.Password)}', '{GetNewID()}')";
							cmd.ExecuteNonQueryAsync();
						}
					}
					RemoveRegToken(token);
					return true;
				}catch
				{
					return false;
				}
			}
			else
				return false;

		}

		internal static void RemoveUser(string userid)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = $"DELETE FROM {DBCredentials.UserTable} WHERE userid = '{userid}';";
					cmd.CommandText += $"DELETE FROM {DBCredentials.ApiTable} WHERE userid = '{userid}';";
					cmd.ExecuteNonQueryAsync();
				}
			}
		}

		internal static string RegisterNewApiKey(string userid)
		{
			string id = Guid.NewGuid().ToString();
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = $"INSERT INTO {DBCredentials.ApiTable} VALUES('{userid}', '{id}')";
					cmd.ExecuteNonQueryAsync();
				}
			}
			return id;
		}

		internal static void RemoveRegToken(string token)
		{
			token = Uri.EscapeDataString(token);
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = $"DELETE FROM {DBCredentials.RegTokenTable} WHERE token = '{token}'";
					cmd.ExecuteNonQuery();
				}
			}
		}

		internal static string GetNewRegToken(string userid)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					string token = Uri.EscapeDataString(GetNewID());
					cmd.CommandText = $"INSERT INTO {DBCredentials.RegTokenTable} VALUES('{token}', '{userid}')";
					cmd.ExecuteNonQuery();
					return token;
				}
			}
		}

		internal static string GetNewID() => Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("+", "-").Replace("/", "~").Replace("=", "").Replace(@"\", ".");

		internal static string AddMedia(string userid, MediaModel media)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					string id = GetNewID();
					cmd.CommandText = $"INSERT INTO {DBCredentials.MediaTable} VALUES('{id}', '{userid}', '{Uri.EscapeDataString(media.uri)}', '{media.type.ToString()}')";
					cmd.ExecuteNonQuery();
					return $"{HOST}/i/{id}";
				}
			}
		}

		internal static MediaModel GetMedia(string id)
		{
			id = Uri.EscapeDataString(id);
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = $"SELECT fileuri, type FROM {DBCredentials.MediaTable} WHERE id = '{id}'";
					try
					{
						using (var reader = cmd.ExecuteReader())
						{
							if (!reader.HasRows)
								return null;
							reader.Read();
							return new MediaModel(Uri.UnescapeDataString(reader.GetString(0)), (MediaModel.MediaType)Enum.Parse(typeof(MediaModel.MediaType), reader.GetString(1)));
						}
					}catch
					{
						return null;
					}
				}
			}
		}

		internal static UserStatsModel GetUserStats(string userid)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					try
					{
						cmd.CommandText = $"SELECT COUNT(owner) FROM {DBCredentials.MediaTable} WHERE owner = '{userid}'";
						return new UserStatsModel() { screenShotCount = (int)(long)cmd.ExecuteScalar()};
					}catch
					{
						return new UserStatsModel();
					}
				}
			}
		}

		internal static UserModel ValidateRegistrationToken(string token)
		{
			token = Uri.EscapeDataString(token);
			using (var conn = GetConnection())
			{
				using (var cmd = conn.CreateCommand())
				{
					try
					{
						cmd.CommandText = $"SELECT referer FROM {DBCredentials.RegTokenTable} WHERE token='{token}'";
						string refererID = (string)cmd.ExecuteScalar();
						if (string.IsNullOrWhiteSpace(refererID))
							return null;
						else
							return GetUserFromID(refererID);

					}catch
					{
						return null;
					}
				}
			}
		}
	}
}
