using eventManager.Service;
using Microsoft.AspNetCore.Mvc;

namespace eventManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : Controller
    {
        private readonly RoleService _roleService;

        public RoleController(RoleService roleService)
        {
            _roleService = roleService;
        }
        [HttpGet("roleList")]
        public async Task<IActionResult> GetRoles()
        {
            var roleList = await _roleService.GetAllRoles();
            return Ok(roleList);
        }
    }
}
