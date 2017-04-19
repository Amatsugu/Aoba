using Nancy.Authentication.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.ModelBinding;
using Nancy.Security;

namespace LuminousVector.Aoba.Server.Models
{
	class UserMapper : IUserMapper
	{
		public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
		{
			Console.WriteLine(context.Request.Form.username);
			return Aoba.GetUserFromApiKey(identifier.ToString());
		}
	}
}
