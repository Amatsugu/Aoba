using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using System;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AobaServer
{
	internal class AobaAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
	{
		public AobaAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock) : base(options, logger, encoder, clock)
		{
		}

		protected override Task<AuthenticateResult> HandleAuthenticateAsync()
		{
			throw new System.NotImplementedException();
		}

		protected override Task HandleChallengeAsync(AuthenticationProperties properties)
		{
			//Don't challenge API requests
			if (OriginalPath.StartsWithSegments("/api"))
			{
				Response.StatusCode = StatusCodes.Status401Unauthorized;
				Response.BodyWriter.Complete();
				return Task.CompletedTask;
			}
			//Redirect to login page
			Response.Redirect($"/auth/login?ReturnUrl={Uri.EscapeDataString(OriginalPath)}");
			return Task.CompletedTask;
		}

		protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
		{
			//Don't show error page for api requests
			if (OriginalPath.StartsWithSegments("/api"))
			{
				Response.StatusCode = StatusCodes.Status403Forbidden;
				Response.BodyWriter.Complete();
				return Task.CompletedTask;
			}
			//Show Error page
			Response.Redirect($"/error/{StatusCodes.Status403Forbidden}");
			return Task.CompletedTask;
		}
	}
}