using AobaServer.Models;
using AobaServer.Services;
using AobaServer.Utilz;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;

namespace AobaServer.Controllers.API
{
	[Route("api/auth")]
	[ApiController]
	public class AuthAPI : ControllerBase
	{
		private readonly AccountsService _accounts;
		private readonly AuthInfo _authInfo;

		public AuthAPI(AccountsService accounts, AuthInfo authInfo)
		{
			_accounts = accounts;
			_authInfo = authInfo;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromForm] LoginCredentials credentials)
		{
			var user = await _accounts.VerifyLogin(credentials);
			if (user == null)
				return Unauthorized();

			var token = user.GetToken(_authInfo);

			Response.Cookies.Append("token", token);
			return Ok();
		}

		[HttpPost("register/{regToken}")]
		public async Task<IActionResult> RegisterUser([FromForm] LoginCredentials credentials, string regToken)
		{
			var user = await _accounts.RegisterUser(credentials, regToken.ToObjectId());
			if (user == null)
				return Unauthorized();

			var token = user.GetToken(_authInfo);
			Response.Cookies.Append("token", token);
			return Ok();
		}

		[Authorize("admin")]
		[HttpGet("regToken")]
		public async Task<IActionResult> GetRegistrationToken()
		{
			var id = User.GetId();
			var regToken = await _accounts.GetRegistrationToken(id);
			return Ok(new {
				regToken
			});
		}


	}
}