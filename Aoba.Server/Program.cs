using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LuminousVector.Aoba.Models;
using Nancy.Hosting.Self;

namespace LuminousVector.Aoba.Server
{
	class Program
	{
		static void Main(string[] args)
		{
			var host = new NancyHost(new HostConfiguration() { AllowChunkedEncoding = false, UrlReservations = new UrlReservations() { CreateAutomatically = true } }, new Uri("http://localhost:4321"));
			host.Start();
			//Console.Write("Getting Data... ");
			//var users = Aoba.GetAllUsers();
			//Console.WriteLine("Done!");
			//Console.Write("Posting Data... ");
			//AobaCore.AddUsers(users);
			//Console.WriteLine("Done!");
			Console.WriteLine("Hosting on localhost:4321");
			Console.ReadLine();
			host.Dispose();
		}
	}
}
