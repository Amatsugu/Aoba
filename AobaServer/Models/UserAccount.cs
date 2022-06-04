using Microsoft.IdentityModel;
using Microsoft.IdentityModel.Tokens;

using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace AobaServer.Models
{
	public class UserAccount
	{
		[BsonId]
		public ObjectId Id { get; set; }
		public string Username { get; set; }
		public string PasswordHash { get; set; }
		public string Role { get; set; }
		public ObjectId[] ApiKeys { get; set; }
		public List<ObjectId> RegTokens { get; set; }

		internal string GetToken(AuthInfo authInfo)
		{
			var handler = new JwtSecurityTokenHandler();
			var signCreds = new SigningCredentials(new SymmetricSecurityKey(authInfo.SecureKey), SecurityAlgorithms.HmacSha256);
			var identity = GetIdentity();
			var token = handler.CreateEncodedJwt(authInfo.Issuer, authInfo.Audience, identity, notBefore: DateTime.Now, expires: null, issuedAt: DateTime.Now, signCreds);
			return token;
		}

		public ClaimsIdentity GetIdentity()
		{
			var id = new ClaimsIdentity(new []
			{
				new Claim(ClaimTypes.NameIdentifier, Id.ToString()),
				new Claim(ClaimTypes.Name, Username),
			});

			if (Role != null)
				id.AddClaim(new Claim(ClaimTypes.Role, Role));
			return id;
		}
	}
}
