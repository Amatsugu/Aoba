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
	class UserModel : IUserIdentity, IIdentity
	{
		public string Name { get; }

		public string AuthenticationType { get; }

		public bool IsAuthenticated { get; }

		public string UserName { get; }

		public IEnumerable<string> Claims { get; }

		public UserModel(string username, string authType = "stateless")
		{
			UserName = Name = Uri.EscapeDataString(username);
			AuthenticationType = authType;
			IsAuthenticated = true;
		}


	}
}
