﻿using hrm.Common;
using hrm.DTOs;
using hrm.Respository.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace hrm.Controllers
{
    [Route("api/permission")]
    [ApiController]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionRespository _permissionRespository;

        public PermissionController(IPermissionRespository permissionRespository)
        {
            _permissionRespository = permissionRespository;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPermissions()
        {
            var permissions = await _permissionRespository.GetAllPermissions();
            if (permissions == null || !permissions.Any())
            {
                return NotFound("No permissions found.");
            }
            return Ok(new BaseResponse<IEnumerable<Entities.Permissions>>(permissions, "Success!", true));
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> CreatePermission([FromBody] PermissionDto permissionDto)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return BadRequest(error ?? "Invalid data");
            }

            var (message, success) = await _permissionRespository.CreatePermission(permissionDto);
            if (!success)
            {
                return BadRequest(message);
            }
            return Ok(new BaseResponse<string>("", message, success));
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePermission([FromBody] PermissionDto permissionDto, int id)
        {
            if (!ModelState.IsValid)
            {
                var error = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                return BadRequest(error ?? "Invalid data");
            }
            var (message, success) = await _permissionRespository.UpdatePermission(id, permissionDto);
            if (!success)
            {
                return BadRequest(message);
            }
            return Ok(new BaseResponse<string>("", message, success));
        }
    }
}