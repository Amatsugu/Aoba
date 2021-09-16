using AobaServer.Services;
using AobaServer.Utilz;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AobaServer.Controllers.API
{
	[Authorize]
	[ApiController]
	[Route("api/media")]
	public class MediaAPI : ControllerBase
	{
		private readonly MediaService _media;

		public MediaAPI(MediaService media)
		{
			_media = media;
		}

		[HttpPost("upload")]
		public async Task<IActionResult> UploadMedia([FromForm] IFormFile file)
		{
			var id = await _media.UploadMedia(file.OpenReadStream(), file.FileName, User.GetId());

			return Ok(new 
			{
				id
			});
		}


		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(string id)
		{
			return StatusCode(StatusCodes.Status418ImATeapot);
		}
	}
}
