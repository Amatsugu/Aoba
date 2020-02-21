using System;
using System.IO;
using Nancy;
using Nancy.TinyIoc;
using Nancy.Conventions;
using Nancy.Bootstrapper;
using Nancy.Authentication.Stateless;
using Nancy.Configuration;

namespace LuminousVector.Aoba.Server.Bootstrap
{
	public class NancyBootstrap : DefaultNancyBootstrapper
	{

		private byte[] favicon;

		protected override byte[] FavIcon
		{
			get { return favicon ?? (favicon = LoadFavIcon()); }
		}

		private byte[] LoadFavIcon()
		{
			return File.ReadAllBytes(@"AobaWeb/res/img/Aoba.ico");
		}


		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			//StaticConfiguration.Caching.EnableRuntimeViewDiscovery = StaticConfiguration.Caching.EnableRuntimeViewUpdates = true;
#if DEBUG
			pipelines.AfterRequest += AfterRequest;
#endif
		}

#if DEBUG
		private void AfterRequest(NancyContext context)
		{
			Console.WriteLine($"DEBUG: {context.Request.UserHostAddress}");
		}
#endif

		protected override IRootPathProvider RootPathProvider
		{
			get { return new RootProvider(); }
		}

		public override void Configure(INancyEnvironment environment)
		{
			environment.Tracing(false, true);
			base.Configure(environment);
		}

		protected override void ConfigureConventions(NancyConventions nancyConventions)
		{
			nancyConventions.ViewLocationConventions.Add((viewName, model, context) =>
			{
				return string.Concat("AobaWeb/", viewName);
			});
			nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("res", $"AobaWeb/res"));
		}
	}
}

