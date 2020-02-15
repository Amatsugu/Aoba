using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LuminousVector.Aoba.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using Nancy.Hosting.Self;

namespace LuminousVector.Aoba.Server
{
	class Program
	{
		static void Main(string[] args)
		{
			var host = new NancyHost(new HostConfiguration() { AllowChunkedEncoding = false, UrlReservations = new UrlReservations() { CreateAutomatically = true } }, new Uri("http://localhost:4321"));
			host.Start();
			Console.WriteLine("Hosting on localhost:4321");
			Console.ReadLine();
			host.Dispose();
		}
	}

}
