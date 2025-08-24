using eventManager.Model;
using eventManager.Service;
using Microsoft.AspNetCore.Mvc;

namespace eventManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : Controller
    {
        private readonly AdminService _adminService;
        public AdminController(AdminService adminService)
        {
            _adminService = adminService;
        }
        [HttpGet("pending-organiser")]
        public IActionResult GetAllPendingOrganizer()
        {
            var res = _adminService.GetAllPendingOrganizerStaff();
            return Ok(res);
        }
        [HttpPost("approve-organiser")]
        public async Task<IActionResult> ChangeOrganizerStatus(string status, string organiser_id)
        {
            var res = await _adminService.ChangeOrganiserStatus(status, organiser_id);
            return Ok(res);
        }
        [HttpPost("add-staff")]
        public async Task<IActionResult> AddAdminStaff([FromBody] users input)
        {
            var res = _adminService.AddAdminStaff(input);
            return Ok(res);
        }
    }
}
