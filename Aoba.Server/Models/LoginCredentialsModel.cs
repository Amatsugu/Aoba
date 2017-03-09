using System;

namespace LuminousVector.Aoba.Server.Models
{
	public enum AuthMode
	{
		API,
		Form
	}

	public class LoginCredentialsModel
	{
		public string username { get; set; }
		public string password { get; set; }
		public AuthMode authMode { get; set; }

		public override string ToString()
		{
			return $"{username}|{password}|{authMode.ToString()}";
		}
	}
}
