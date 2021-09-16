using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AobaServer.Controllers
{
	[AllowAnonymous]
	[Route("auth")]
	public class AuthController : Controller
	{
		[HttpGet("login")]
		public IActionResult Login(string ReturnUrl)
		{
			ViewData["returnUrl"] = ReturnUrl;
			return View();
		}

		[HttpGet("register/{token}")]
		public IActionResult Register(string token)
		{

			return View();
		}
	}
}
