using System;
using System.IO;
using Nancy;
using Nancy.TinyIoc;
using Nancy.Conventions;
using Nancy.Bootstrapper;
using Nancy.Authentication.Stateless;

namespace LuminousVector.Aoba.Server
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
			Conventions.ViewLocationConventions.Add((viewName, model, context) =>
			{
				return string.Concat("AobaWeb/", viewName);
			});
		}

		protected override void ConfigureConventions(NancyConventions nancyConventions)
		{
			nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("res", $"AobaWeb/res"));
			nancyConventions.StaticContentsConventions.Add(StaticContentConventionBuilder.AddDirectory("media", $"media"));
		}
	}
}

