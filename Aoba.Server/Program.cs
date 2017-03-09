using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy.Hosting.Self;
using LuminousVector.Aoba.Server.Credentials;
using System.Diagnostics;
using System.Threading;

namespace LuminousVector.Aoba.Server
{
	class Program
	{
		static void Main(string[] args)
		{
			var host = new NancyHost(new HostConfiguration() { UrlReservations = new UrlReservations() { CreateAutomatically = true } }, new Uri("http://localhost:4321"));
			host.Start();
#if DEBUG
			Process.Start("http://localhost:4321");
#endif
			Console.ReadLine();
			host.Dispose();
		}
	}
}
