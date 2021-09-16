using AobaServer.Models;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Linq;
using System.Threading.Tasks;

namespace AobaServer
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllersWithViews()
#if DEBUG
			.AddRazorRuntimeCompilation()
#endif
			.AddNewtonsoftJson();

			var authInfo = AuthInfo.LoadOrCreate("Auth.json", "aoba", "aoba");

			var signingKey = new SymmetricSecurityKey(authInfo.SecureKey);

			var validationParams = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = signingKey,
				ValidateIssuer = true,
				ValidIssuer = authInfo.Issuer,
				ValidateAudience = true,
				ValidAudience = authInfo.Audience,
				ValidateLifetime = false,
				ClockSkew = TimeSpan.FromMinutes(1),
			};

			services.AddAuthentication(options =>
			{
				options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
				options.DefaultChallengeScheme = "Aoba";
			}).AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options => //Bearer auth
			{
				options.TokenValidationParameters = validationParams;
				options.Events = new JwtBearerEvents
				{
					OnMessageReceived = ctx => //Retreive token from cookie if not found in headers
					{
						if (string.IsNullOrWhiteSpace(ctx.Token))
							ctx.Token = ctx.Request.Cookies["token"];
						if (string.IsNullOrWhiteSpace(ctx.Token))
							ctx.Token = ctx.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "");
						return Task.CompletedTask;
					},
					OnAuthenticationFailed = ctx =>
					{
						//Console.WriteLine($"[!!!] Auth Failed {ctx.HttpContext.Request.GetDisplayUrl()} \n{ctx.Exception}");
						ctx.Response.Cookies.Append("token", "", new CookieOptions
						{
							MaxAge = TimeSpan.Zero,
							Expires = DateTime.Now
						});
						ctx.Options.ForwardChallenge = CookieAuthenticationDefaults.AuthenticationScheme;

						return Task.CompletedTask;
					}
				};
				Configuration.Bind("JwtSettings", options);
			}).AddScheme<AuthenticationSchemeOptions, AobaAuthenticationHandler>("Aoba", cfg => { });
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseBrowserLink();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}
			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}