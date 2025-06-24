using hrm.Common;
using hrm.DTOs;
using hrm.Entities;
using hrm.Providers;
using hrm.Respository.Auth;
using Microsoft.AspNetCore.Mvc;

[Route("api/auth")]
[ApiController]

public class AuthController : ControllerBase
{
    private readonly IAuthRespository _authRespository;
    private readonly AesCryptoProvider _aesCryptoProvider;

    public class AuthResponse
    {
        public UserDto UserInfo { get; set; } = null!;
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Permission { get; set; } = string.Empty;
    }


    public AuthController(IAuthRespository authRespository, AesCryptoProvider aesCryptoProvider)
    {
        _authRespository = authRespository;
        _aesCryptoProvider = aesCryptoProvider;
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenResponseDto request)
    {
        var result = await _authRespository.RefreshToken(request.AccessToken, request.RefreshToken);
        if (result is null)
        {
            return Unauthorized("Invalid refresh token or access token");
        }
        var response = new RefreshTokenResponseDto
        {
            AccessToken = result.Value.Item1,
            RefreshToken = result.Value.Item2
        };

        return Ok(new BaseResponse<RefreshTokenResponseDto>(response, "Success", true));

    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDto request)
    {

        var userInfo = await _authRespository.AuthLogin(request);

        if (userInfo is not (Users user, string accessToken, string refreshToken))
        {
            return Unauthorized("Invalid username or password");
        }
        string permission = _aesCryptoProvider.Encrypt(user.Role.Name);
        var response = new AuthResponse
        {
            UserInfo = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Agent = user.Agent,
                CreatedAt = user.CreatedAt
            },
            Permission = permission,
            AccessToken = accessToken,
            RefreshToken = refreshToken
        };

        return Ok(new BaseResponse<AuthResponse>(response, "", true));
    }
}
