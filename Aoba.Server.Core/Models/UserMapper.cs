using Nancy.Authentication.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;
using System.Security.Claims;

namespace LuminousVector.Aoba.Server.Models
{
	class UserMapper : IUserMapper
	{
		public ClaimsPrincipal GetUserFromIdentifier(Guid identifier, NancyContext context)
		{
			Console.WriteLine(context.Request.Form.username);
			return AobaCore.GetUserFromApiKey(identifier.ToString());
		}
	}
}
