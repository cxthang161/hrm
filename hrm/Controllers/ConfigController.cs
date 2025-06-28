using System.IdentityModel.Tokens.Jwt;
using hrm.Common;
using hrm.DTOs;
using hrm.Respository.Configs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrm.Controllers
{
    [Route("api/config")]
    [ApiController]
    public class ConfigController : ControllerBase
    {
        private readonly IConfigRespository _configRespository;


        public ConfigController(IConfigRespository configRespository)
        {
            _configRespository = configRespository;
        }

        [Authorize(Policy = "Permission:create_config")]
        [HttpPost]
        public async Task<IActionResult> CreateConfig([FromForm] ConfigDto configDto)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return BadRequest(error ?? "Invalid data");
            }

            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sid)!;

            int userId = int.Parse(userIdClaim.Value);

            var (message, success) = await _configRespository.CreateConfig(configDto, userId);
            if (!success)
            {
                return BadRequest(message);
            }
            return Ok(new BaseResponse<string>("", message, success));
        }

        [Authorize(Policy = "Permission:edit_config")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateConfig([FromForm] ConfigUpdateDto configDto, int id)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return BadRequest(error ?? "Invalid data");
            }

            var userIdClaim = User.FindFirst(JwtRegisteredClaimNames.Sid);
            if (userIdClaim == null)
            {
                return Unauthorized("User ID not found in token.");
            }
            int userId = int.Parse(userIdClaim.Value);
            var (message, success) = await _configRespository.UpdateConfig(configDto, userId, id);
            if (!success)
            {
                return BadRequest(message);
            }
            return Ok(new BaseResponse<string>("", message, success));
        }

        [Authorize(Policy = "Permission:get_config")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetConfigById(int id)
        {
            var config = await _configRespository.GetConfigById(id);
            if (config == null)
            {
                return NotFound("Configuration not found.");
            }
            return Ok(new BaseResponse<ConfigInfoDto>(config, "Success", true));
        }

        [Authorize(Policy = "Permission:getAll_config")]
        [HttpGet]
        public async Task<IActionResult> GetAllConfigs([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var (configs, totalRows) = await _configRespository.GetAllConfigs(pageIndex, pageSize);
            if (configs == null || !configs.Any())
            {
                return NotFound("No configurations found.");
            }
            var pagination = new PaginationResponse<ConfigInfoDto>(
                configs,
                pageIndex,
                (int)Math.Ceiling((double)totalRows / pageSize)
            );
            return Ok(new BaseResponse<PaginationResponse<ConfigInfoDto>>(pagination, "Success", true));
        }
    }
}
