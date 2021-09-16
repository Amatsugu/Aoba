using AobaServer.Models;
using AobaServer.Services;
using AobaServer.Utilz;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace AobaServer.Controllers
{
	[Authorize]
	public class HomeController : Controller
	{
		private readonly ILogger<HomeController> _logger;
		private readonly AccountsService _accounts;

		public HomeController(ILogger<HomeController> logger, AccountsService accounts)
		{
			_logger = logger;
			_accounts = accounts;
		}

		public async Task<IActionResult> IndexAsync([FromServices] AuthInfo authInfo)
		{
			var user = await _accounts.GetUser(User.GetId());
			var shareXInfo = new ShareXDestination();
			shareXInfo.Headers.Add("Authorization", $"Bearer {user.GetToken(authInfo)}");

			ViewData["shareX"] = JsonConvert.SerializeObject(shareXInfo, Formatting.Indented);
			return View();
		}


		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
	}
}
