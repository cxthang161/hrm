using hrm.Common;
using hrm.DTOs;
using hrm.Respository.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace hrm.Controllers
{
    [Route("api/user")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRespository _userRepository;

        public class AuthResponse
        {
            public UserDto UserInfo { get; set; } = null!;
            public string AccessToken { get; set; } = string.Empty;
            public string RefreshToken { get; set; } = string.Empty;
            public string Permission { get; set; } = string.Empty;
        }


        public UserController(IUserRespository userRepository)
        {
            _userRepository = userRepository;
        }
        [Authorize(Roles = "1")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto request)
        {
            var result = await _userRepository.CreateUser(request);
            return Ok(new BaseResponse<string>("", result.Item1, result.Item2));
        }

        [Authorize(Roles = "1")]
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userRepository.DeleteUser(id);
            return Ok(new BaseResponse<string>("", result.Item1, result.Item2));

        }

        [Authorize(Roles = "1")]
        [HttpPut("update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] CreateUserDto request)
        {
            var result = await _userRepository.UpdateUser(id, request);
            return Ok(new BaseResponse<string>("", result.Item1, result.Item2));

        }

        [Authorize(Roles = "1")]
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
                Agent = u.Agent
            });

            var pagination = new PaginationResponse<UserDto>(
                userDtos,
                pageIndex,
                (int)Math.Ceiling((double)totalRows / pageSize)
            );

            return Ok(new BaseResponse<PaginationResponse<UserDto>>(pagination, "Users retrieved successfully", true));
        }

        //[Authorize(Roles = "1")]
        [HttpGet("get-by-id/{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            var user = await _userRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
            var userDto = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                CreatedAt = user.CreatedAt,
                Agent = user.Agent
            };
            return Ok(new BaseResponse<UserDto>(userDto, "User retrieved successfully", true));
        }
    }
}
