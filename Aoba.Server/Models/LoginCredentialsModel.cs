using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuminousVector.Aoba.Server.Models
{
	public class LoginCredentialsModel
	{
		public string username { get; set; }
		public string password { get; set; }
		public string authMode { get; set; }

		public override string ToString()
		{
			return $"{username}|{password}|{authMode}";
		}
	}
}
