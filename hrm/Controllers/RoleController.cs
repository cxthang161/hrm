using hrm.Common;
using hrm.Respository.Roles;
using Microsoft.AspNetCore.Mvc;

namespace hrm.Controllers
{
    [Route("api/role")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRespository _roleRepository;
        public RoleController(IRoleRespository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var roles = await _roleRepository.GetAll();
            if (roles == null || !roles.Any())
            {
                return NotFound("No roles found.");
            }
            return Ok(new BaseResponse<IEnumerable<Entities.Roles>>(roles, "Success!", true));
        }
    }
}
