using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JwtAuthen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenService authenService;

        public AuthController(IAuthenService authenService)
        {
            this.authenService = authenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            var user = await authenService.Register(request);

            if (user == null) return BadRequest("User already exists");

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var result = await authenService.Login(request);

            return Ok(result);
        }


        [HttpGet("TestAdminRole"), Authorize(Roles = "Admin")]
        public IActionResult test()
        {
            return Ok("Authorize Success");
        }

        [HttpGet("GetMe"), Authorize]
        public IActionResult GetMe()
        {
            var result = authenService.GetMe();
            return Ok(result);
        }

    }
}
