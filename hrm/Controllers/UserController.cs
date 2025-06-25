using hrm.Common;
using hrm.DTOs;
using hrm.Providers;
using hrm.Respository.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


namespace hrm.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRespository _userRepository;
        private readonly AesCryptoProvider _aesCryptoProvider;

        public UserController(IUserRespository userRepository, AesCryptoProvider aesCryptoProvider)
        {
            _userRepository = userRepository;
            _aesCryptoProvider = aesCryptoProvider;
        }

        [Authorize(Roles = "admin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
        {
            var result = await _userRepository.CreateUser(request);
            return Ok(new BaseResponse<string>("", result.Item1, result.Item2));
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userRepository.DeleteUser(id);
            return Ok(new BaseResponse<string>("", result.Item1, result.Item2));

        }

        [Authorize(Roles = "admin")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] CreateUserDto request)
        {
            var result = await _userRepository.UpdateUser(id, request);
            return Ok(new BaseResponse<string>("", result.Item1, result.Item2));

        }

        [HttpGet("get-all")]
        public async Task<IActionResult> GetAllUsers([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = 10)
        {
            var (users, totalRows) = await _userRepository.GetAll(pageIndex, pageSize);

            if (!users.Any())
            {
                return NotFound("No users found.");
            }

            var userDtos = users.Select(u => new UserDto
            {
                Id = u.Id,
                UserName = u.UserName,
                CreatedAt = u.CreatedAt,
                Agent = u.Agent,
                Permissions = u.Permissions
            });

            var pagination = new PaginationResponse<UserDto>(
                userDtos,
                pageIndex,
                (int)Math.Ceiling((double)totalRows / pageSize)
            );

            return Ok(new BaseResponse<PaginationResponse<UserDto>>(pagination, "Users retrieved successfully", true));
        }

        [Authorize(Roles = "admin")]
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            List<string> permissionList = string.IsNullOrEmpty(user.Permissions)
                                        ? new List<string>()
                                        : user.Permissions.Split(',').Select(p => p.Trim()).ToList();

            string? permissions = user.Permissions != null
                                    ? _aesCryptoProvider.Encrypt(JsonConvert.SerializeObject(permissionList))
                                    : null;

            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                CreatedAt = user.CreatedAt,
                Agent = user.Agent,
                Permissions = user.Permissions,
            };
            return Ok(new BaseResponse<UserDto>(userDto, "User retrieved successfully", true));
        }

        [Authorize(Roles = "admin")]
        [HttpPost("change-password/{id}")]
        public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto request)
        {
            if (string.IsNullOrEmpty(request.Password))
            {
                return BadRequest("New password and confirm password cannot be empty.");
            }
            var result = await _userRepository.ChangePassword(request.Password, id);
            return Ok(new BaseResponse<string>("", result.Item1, result.Item2));
        }
    }
}
