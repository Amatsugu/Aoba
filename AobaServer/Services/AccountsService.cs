using AobaServer.Models;

using MongoDB.Bson;
using MongoDB.Driver;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace AobaServer.Services
{
	public class AccountsService
	{
		private readonly IMongoCollection<UserAccount> _users;

		public AccountsService(IMongoDatabase db)
		{
			_users = db.GetCollection<UserAccount>("users");
		}

		public async Task<UserAccount> RegisterUser(LoginCredentials credentials, ObjectId regToken, string role = null)
		{
			var update = Builders<UserAccount>.Update.Pull(u => u.RegTokens, regToken);
			var updateResult = await _users.UpdateOneAsync("{}", update);
			if (updateResult.MatchedCount == 0)
				throw new Exception("Invalid registration Token");
			return await AddUser(credentials, role);
		}

		public async Task<UserAccount> AddUser(LoginCredentials credentials, string role = null)
		{
			var user = new UserAccount
			{
				Id = ObjectId.GenerateNewId(),
				Username = credentials.Username,
				PasswordHash = HashPassword(credentials.Password),
				Role = role,
			};

			await _users.InsertOneAsync(user);
			return user;
		}

		public Task<UserAccount> GetUser(ObjectId id)
		{
			return _users.Find(u => u.Id == id).FirstOrDefaultAsync();
		}

		public async Task<UserAccount> VerifyLogin(LoginCredentials credentials)
		{
			if (string.IsNullOrWhiteSpace(credentials.Username) || string.IsNullOrWhiteSpace(credentials.Password))
				return null;
			var user = await _users.Find(u => u.Username == credentials.Username).FirstOrDefaultAsync();

			if (user == null)
				return null;

			if (VerifyPassword(credentials.Password, user.PasswordHash))
				return user;
			return null;
		}

		public async Task<ObjectId> GetRegistrationToken(ObjectId id)
		{
			var token = ObjectId.GenerateNewId();
			var update = Builders<UserAccount>.Update.Push(u => u.RegTokens, token);
			await _users.UpdateOneAsync(u => u.Id == id, update);
			return token;
		}

		public Task<UserAccount> VerifyRegistrationToken(ObjectId token)
		{
			var filter = Builders<UserAccount>.Filter.AnyEq(u => u.RegTokens, token);
			return _users.Find(filter).FirstOrDefaultAsync();
		}

		#region Password Hash

		/// <summary>
		/// Generate a password hast
		/// </summary>
		/// <param name="password"></param>
		/// <returns></returns>
		public string HashPassword(string password)
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

		/// <summary>
		/// Verify a password against it's hash
		/// </summary>
		/// <param name="password"></param>
		/// <param name="passwordHash"></param>
		/// <returns></returns>
		public bool VerifyPassword(string password, string passwordHash)
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

		#endregion Password Hash
	}
}
