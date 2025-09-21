using eventManager.Service;
using Microsoft.AspNetCore.Mvc;

namespace eventManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;

        public EmailController(IEmailService emailService)
        {
            _emailService = emailService;
        }
        [HttpPost("send-email")]
        public async Task<IActionResult> SendMail(string to, string subject, string message)
        {
            var result = await _emailService.SendEmailAsync(to, subject, message);
            return Ok(result);
        }
    }
}
