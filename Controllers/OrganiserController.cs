using eventManager.Model;
using eventManager.Service;
using Microsoft.AspNetCore.Mvc;

namespace eventManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrganiserController : Controller
    {
        private readonly OrganiserService _organiserService;
        public OrganiserController(OrganiserService organiserService)
        {
            _organiserService = organiserService;
        }
        [HttpGet("add-staff")]
        public async Task<IActionResult> AddOrganoser([FromBody] users input)
        {
            var res = await _organiserService.AddOrgStaff(input);
            return Ok(res);
        }
        [HttpGet("staff")]
        public async Task<IActionResult> GetStaff(string staff_id)
        {
            var res = await _organiserService.GetStaff(staff_id);
            return Ok(res);
        }
        [HttpGet("staff-list")]
        public async Task<IActionResult> GetStaffList(string staff_id)
        {
            var res = await _organiserService.GetStaffList(staff_id);
            return Ok(res);
        }
        [HttpGet("organiser-list")]
        public async Task<IActionResult> GetOrganiserList()
        {
            var res = await _organiserService.GetOrganiserList();
            return Ok(res);
        }
    }
}
