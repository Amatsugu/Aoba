using System;
using System.IO;
using Nancy;
using Nancy.TinyIoc;
using Nancy.Conventions;
using Nancy.Bootstrapper;
using Nancy.Authentication.Stateless;

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
			StaticConfiguration.Caching.EnableRuntimeViewDiscovery = StaticConfiguration.Caching.EnableRuntimeViewUpdates = true;
			Conventions.ViewLocationConventions.Add((viewName, model, context) =>
			{
				return string.Concat("AobaWeb/", viewName);
			});

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

#if DEBUG
		protected override IRootPathProvider RootPathProvider
		{
			get { return new RootProvider(); }
		}
#endif

		protected override void ConfigureConventions(NancyConventions nancyConventions)
		{
			nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("res", $"AobaWeb/res"));
		}
	}
}

