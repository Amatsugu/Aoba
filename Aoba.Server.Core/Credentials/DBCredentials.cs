using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuminousVector.Aoba.Server.Credentials
{
	public static class DBCredentials
	{
		public const string DB_User = "aoba";
		public const string DB_Pass = "b0v2@U9e*uyS^1w*";
		public const string DB_Name = "AobaDB";
		public const string ApiTable = "apiKeys";
		public const string UserTable = "users";
		public const string MediaTable = "images";
		public const string RegTokenTable = "regtokens";
#if DEBUG
		public static string CONNECTION_STRING = $"mongodb://192.168.86.74:27017";
#else
		public static string CONNECTION_STRING = $"mongodb://localhost:27017";
#endif
		public static string PG_CONNECTION_STRING => $"Host={AobaCore.HOST};Username={DB_User};Password={DB_Pass};Database={DB_Name};Pooling=true";
	}
}
