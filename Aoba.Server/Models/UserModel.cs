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
		public string UserName { get; }

		public IEnumerable<string> Claims { get; }

		public UserModel(string username)
		{
			UserName = Uri.EscapeDataString(username);
		}


	}
}
