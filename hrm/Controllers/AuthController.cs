using hrm.Common;
using hrm.DTOs;
using hrm.Entities;
using hrm.Respository.Users;
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
    public async Task<IActionResult> ReFreshToken([FormBody] string accessToken)
    {
        var
    }

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
}
