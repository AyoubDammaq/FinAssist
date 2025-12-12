using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpPost]
        public IActionResult Login()
        {
            // Placeholder for login logic
            return Ok("Login successful");
		}
	}
}
