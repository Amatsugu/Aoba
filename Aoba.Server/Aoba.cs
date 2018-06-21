using System;
using System.Linq;
using System.Security.Cryptography;
using Nancy.Authentication.Stateless;
using Npgsql;
using LuminousVector.Aoba.Server.Models;
using LuminousVector.Aoba.DataStore;
using LuminousVector.Aoba.Server.Credentials;
using LuminousVector.Aoba.Models;
using System.Collections.Generic;
using System.IO;

namespace LuminousVector.Aoba.Server
{
	public static class Aoba
	{
		/// <summary>
		/// Returns a new DB connection
		/// </summary>
		/// <returns></returns>
		internal static NpgsqlConnection GetConnection()
		{
			var con = new NpgsqlConnection(DBCredentials.PG_CONNECTION_STRING);
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
						if (!AobaCore.VerifyPassword(user.Password, passHash))
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

		internal static bool RegisterUser(LoginCredentialsModel user, string token)
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
							cmd.CommandText = $"INSERT INTO {DBCredentials.UserTable} VALUES('{Uri.EscapeDataString(user.Username)}', '{AobaCore.HashPassword(user.Password)}', '{AobaCore.GetNewID()}')";
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
					string token = Uri.EscapeDataString(AobaCore.GetNewID());
					cmd.CommandText = $"INSERT INTO {DBCredentials.RegTokenTable} VALUES('{token}', '{userid}')";
					cmd.ExecuteNonQuery();
					return token;
				}
			}
		}


		internal static string AddMedia(string userid, MediaModel media)
		{
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					string id = AobaCore.GetNewID();
					cmd.CommandText = $"INSERT INTO {DBCredentials.MediaTable} VALUES('{id}', '{userid}', '{Uri.EscapeDataString(media.uri)}', '{media.type.ToString()}')";
					cmd.ExecuteNonQuery();
					return $"{AobaCore.HOST}/i/{id}";
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
							return new MediaModel
							{
								uri = Uri.UnescapeDataString(reader.GetString(0)),
								type = (MediaModel.MediaType)Enum.Parse(typeof(MediaModel.MediaType), reader.GetString(1))
							};
						}
					}catch
					{
						return null;
					}
				}
			}
		}

		internal static List<UserModel> GetAllUsers()
		{
			var users = new List<UserModel>();
			using (var con = GetConnection())
			{
				using (var cmd = con.CreateCommand())
				{
					cmd.CommandText = $"SELECT * FROM {DBCredentials.UserTable}";
					try
					{
						using (var userReader = cmd.ExecuteReader())
						{
							if (!userReader.HasRows)
								return null;
							while (userReader.Read())
							{
								var imageCon = GetConnection();
								var user = new UserModel(userReader.GetString(0), userReader.GetString(2), userReader.GetValue(3) as string[])
								{
									passHash = userReader.GetString(1),
									media = new List<MediaModel>(),
									apiKeys = new List<string>(),
									regTokens = new List<string>()
								};
								var imageCommand = imageCon.CreateCommand();
								imageCommand.CommandText = $"SELECT * FROM {DBCredentials.MediaTable} WHERE owner = '{user.ID}'";
								using (var imageReader = imageCommand.ExecuteReader())
								{
									if (imageReader.HasRows)
									{
										while (imageReader.Read())
										{
											var uri = Uri.UnescapeDataString(imageReader.GetString(2));
											try
											{

												user.media.Add(new MediaModel
												{
													uri = uri,
													type = (MediaModel.MediaType)Enum.Parse(typeof(MediaModel.MediaType), imageReader.GetString(3)),
													media = File.ReadAllBytes($"{AobaCore.MEDIA_DIR}/{uri}"),
													id = imageReader.GetString(0),
													ext = Path.GetExtension(uri)
												});
											}catch(Exception imageE)
											{
												Console.WriteLine(imageE.Message);
												continue;
											}
										}
									}
								}

								var apiCon = GetConnection();
								var apiCommand = apiCon.CreateCommand();
								apiCommand.CommandText = $"SELECT * FROM {DBCredentials.ApiTable} WHERE userid = '{user.ID}'";
								using (var apiReader = apiCommand.ExecuteReader())
								{
									if (apiReader.HasRows)
										while (apiReader.Read())
											user.apiKeys.Add(apiReader.GetString(1));
								}

								var regCon = GetConnection();
								var regCommand = regCon.CreateCommand();
								regCommand.CommandText = $"SELECT * FROM {DBCredentials.RegTokenTable} WHERE referer = '{user.ID}'";
								using (var regReader = regCommand.ExecuteReader())
								{
									if (regReader.HasRows)
										while (regReader.Read())
											user.regTokens.Add(regReader.GetString(0));
								}

								users.Add(user);
								imageCon.Dispose();
								apiCon.Dispose();
								regCon.Dispose();
							}
						}
					}
					catch(Exception e)
					{
						Console.WriteLine(e.Message);
						return users;
					}
				}
			}
			return users;
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
