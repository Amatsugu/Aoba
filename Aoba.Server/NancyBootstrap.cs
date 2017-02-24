using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;
using Nancy.Conventions;
using Nancy.Authentication.Stateless;

namespace LuminousVector.Aoba.Server
{
	public class NancyBootstrap : DefaultNancyBootstrapper
	{
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
		}
	}
}

