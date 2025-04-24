using hrm.Entities;
using hrm.Respository.Users;
using Microsoft.AspNetCore.Mvc;

[Route("api/auth")]
[ApiController]

public class AuthController : ControllerBase
{
    private readonly IUserRespository _userRepository;


    public AuthController(IUserRespository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest user)
    {

        var token = await _userRepository.AuthLogin(user);

        if (token == null)
        {
            return Unauthorized("Invalid username or password");
        }

        return Ok(new { token });
    }
}
