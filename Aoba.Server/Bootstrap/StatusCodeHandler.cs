using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nancy;
using Nancy.ErrorHandling;
using Nancy.ViewEngines;
using LuminousVector.Aoba.Server.Models;

namespace LuminousVector.Aoba.Server.Bootstrap
{
	public class StatusCodeHandler : IStatusCodeHandler
	{

		private readonly HttpStatusCode[] _handledCodes = new HttpStatusCode[] 
		{
			HttpStatusCode.NotFound,
			HttpStatusCode.Unauthorized
		};

		private IViewRenderer viewRenderer;

		public StatusCodeHandler(IViewRenderer renderer)
		{
			viewRenderer = renderer;
		}

		public void Handle(HttpStatusCode statusCode, NancyContext context) => context.Response = viewRenderer.RenderView(context, "error", new { statusCode = statusCode, message = StatusCodeMessages.GetMessage(statusCode) });

		public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context) => _handledCodes.Any(x => x == statusCode);
	}
}
