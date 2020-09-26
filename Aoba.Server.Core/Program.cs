using Nancy.Hosting.Self;

using System;
using System.IO;
using System.Threading;

namespace LuminousVector.Aoba.Server
{
	internal class Program
	{
		private static void Main(string[] args)
		{
			using var host = new NancyHost(new HostConfiguration() { AllowChunkedEncoding = false, UrlReservations = new UrlReservations() { CreateAutomatically = true } }, new Uri("http://localhost:4321"));
			host.Start();
			Console.WriteLine(Directory.GetCurrentDirectory());
			Console.WriteLine("Hosting on localhost:4321");
			var ev = new ManualResetEvent(false);
			ev.WaitOne();
		}
	}
}