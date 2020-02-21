using System;
using System.Collections.Generic;
using System.Net;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Security;
using System.Security.Claims;
using System.Security.Principal;

namespace LuminousVector.Aoba.Models
{
	public class UserModel : ClaimsPrincipal
	{
		internal static readonly UserModel Overlord = new UserModel("The Overlord", null, null);


		public string Username { get; }

		public string ID { get; }

		public UserModel(string username, string id, IEnumerable<string> claims = null)
		{
			Username = username;
			ID = id;
		}


	}
}
