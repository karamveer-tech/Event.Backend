using eventManager.Dtos;
using eventManager.Helper;
using eventManager.Model;
using Microsoft.AspNetCore.Mvc;

namespace eventManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : Controller
    {
        public readonly AuthService _authService;
        public AuthController(AuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register")]
        public IActionResult Register(users userInput)
        {
            var res = _authService.AddUpdateUser(userInput);
            return Ok();
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(users userInput)
        {
            var res = await _authService.Login(userInput.email, userInput.password_hash);
            return Ok(res);
        }
        //[HttpPost("login")]
        //public async Task<IActionResult> Logout(users userInput)
        //{
        //    var res = await _authService.Login(userInput.email, userInput.password_hash);
        //    return Ok(res);
        //}
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePassword passInput)
        {
            var res = await _authService.ChangePassword(passInput);
            return Ok(res);
        }
    }
}
