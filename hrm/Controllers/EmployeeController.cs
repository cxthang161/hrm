using hrm.Common;
using hrm.Entities;
using hrm.Respository.Employees;
using Microsoft.AspNetCore.Mvc;

namespace hrm.Controllers
{
    [Route("api/employee")]
    [ApiController]
    public class EmployeeController : Controller
    {
        private readonly IEmployeeRespository _employeeRespository;
        public EmployeeController(IEmployeeRespository employeeRespository) => _employeeRespository = employeeRespository;

        [HttpGet]
        [Route("get-all")]
        public async Task<IActionResult> GetAll()
        {
            var employees = await _employeeRespository.GetAll();

            if (employees == null || !employees.Any())
            {
                return NotFound(new BaseResponse<IEnumerable<Employee>>(null, "No employees found", false));
            }

            return Ok(new BaseResponse<IEnumerable<Employee>>(employees, "Employees retrieved successfully", true));
        }
    }
}
