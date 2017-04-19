using System;

namespace LuminousVector.Aoba.Server.Models
{
	public enum AuthMode
	{
		Form,
		API
	}

	public class LoginCredentialsModel
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public AuthMode AuthMode { get; set; }

		public override string ToString()
		{
			return $"{Username}|{Password}|{AuthMode.ToString()}";
		}
	}
}
