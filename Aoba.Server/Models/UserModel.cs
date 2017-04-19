using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Security;
using System.Security.Claims;
using System.Security.Principal;

namespace LuminousVector.Aoba.Server.Models
{
	class UserModel : IUserIdentity
	{
		internal static readonly UserModel Overlord = new UserModel("The Overlord", null, null);

		public string UserName { get; }
		public string ID { get; }

		public IEnumerable<string> Claims { get; }

		public UserModel(string username, string id, string[] claims = null)
		{
			UserName = Uri.EscapeDataString(username);
			ID = id;
			Claims = claims;
		}


	}
}
