﻿using System;
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
	/*public class StatusCodeHandler : IStatusCodeHandler
	{

		private readonly HttpStatusCode[] _handledCodes = new HttpStatusCode[] 
		{
			HttpStatusCode.NotFound,
			HttpStatusCode.Unauthorized,
#if !DEBUG
			HttpStatusCode.InternalServerError
#endif
			
		};

		private ViewRenderer viewRenderer;

		public StatusCodeHandler(ViewRenderer renderer)
		{
			viewRenderer = renderer;
		}

		public void Handle(HttpStatusCode statusCode, NancyContext context)
		{
			if (statusCode == HttpStatusCode.Unauthorized && context.Request.Path == "/")
			{
				//context.Response = viewRenderer.RenderView(context, "login");
				context.Response.StatusCode = HttpStatusCode.Unauthorized;
			}
			else
			{
				context.Response = viewRenderer.RenderView(context, "error", new
				{
					statusCode,
					message = StatusCodeMessages.GetMessage(statusCode)
				});
				context.Response.StatusCode = statusCode;
			}
		}

		public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context) => _handledCodes.Any(x => x == statusCode);
	}
	*/
}
