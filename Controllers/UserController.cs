using eventManager.Model;
using eventManager.Service;
using Microsoft.AspNetCore.Mvc;

namespace eventManager.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _ctx;
        public UserController(IWebHostEnvironment env, IHttpContextAccessor ctx, UserService userService)
        {
            _userService = userService;
            _env = env;
            _ctx = ctx;
        }
        [HttpGet("get-user-by-email")]
        public async Task<IActionResult> GetUserDetails([FromQuery] string emailId)
        {
            var res = await _userService.GetUserDetails(emailId);
            return Ok(res);
        }
        [HttpGet("get-user-by-id")]
        public async Task<IActionResult> GetUserDetailsById([FromQuery] int userId)
        {
            var res = await _userService.GetUserDetailsById(userId);
            return Ok(res);
        }

        [HttpPost("book-event")]
        public async Task<IActionResult> BookEvent([FromForm] bookings dto)
        {
            try
            {
                if (dto.booking_date == default)
                    dto.booking_date = DateTime.UtcNow;

                // Deserialize tickets JSON into list
                if (!string.IsNullOrEmpty(dto.bookingTicketsJson))
                {
                    dto.ticket_Types = Newtonsoft.Json.JsonConvert
                        .DeserializeObject<List<ticket_type>>(dto.bookingTicketsJson);
                }

                var res = await _userService.AddOrUpdateBooking(dto);

                return Ok(res);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
          
        }
        [HttpGet("get-booked-ticket")]
        public async Task<int> GetBookedTicketCount([FromQuery] int eventId)
        {
            var res = await _userService.GetBookedTicketCount(eventId);
            return res;
        }
        
        [HttpGet("get-my-bookings")]
        public async Task<IActionResult> GetMyBookings([FromQuery] int userId)
        {
            var res = await _userService.GetMyBookings(userId);
            return Ok(res);
        }

        [HttpGet("api-version")]
        public IActionResult GetApiVersion()
        {
            return Ok(new { version = "1.0.0" });
        }
    }
}
