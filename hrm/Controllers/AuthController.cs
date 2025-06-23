using hrm.Common;
using hrm.DTOs;
using hrm.Entities;
using hrm.Respository.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/auth")]
[ApiController]

public class AuthController : ControllerBase
{
    private readonly IUserRespository _userRepository;

    public class AuthResponse
    {
        public UserDto UserInfo { get; set; } = null!;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
    }


    public AuthController(IUserRespository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost("refresh-token")]
    //public async Task<IActionResult> ReFreshToken([FormBody] string accessToken)
    //{
    //    var
    //}

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto request)
    {

        var userInfo = await _userRepository.AuthLogin(request);

        if (userInfo is not (Users user, string accessToken, string refreshToken))
        {
            return Unauthorized("Invalid username or password");
        }
        var response = new AuthResponse
        {
            UserInfo = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Role = user.Role,
                Agent = user.Agent,
                CreatedAt = user.CreatedAt
            },
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

        return Ok(new BaseResponse<AuthResponse>(response, "", true));
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
        var users = await _userRepository.GetAll(pageIndex, pageSize);
        if (users == null || !users.Any())
        {
            return NotFound(new BaseResponse<IEnumerable<UserDto>>(null, "No users found", false));
        }
        var userDtos = users.Select(u => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            CreatedAt = u.CreatedAt,
            Role = u.Role,
            Agent = u.Agent
        });
        return Ok(new BaseResponse<IEnumerable<UserDto>>(userDtos, "Users retrieved successfully", true));
    }
}
