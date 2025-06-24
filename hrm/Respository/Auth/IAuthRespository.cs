using hrm.DTOs;

namespace hrm.Respository.Auth
{
    public interface IAuthRespository
    {
        Task<(Entities.Users, string, string)?> AuthLogin(UserLoginDto user);
        Task<(string, string)?> RefreshToken(string accessToken, string refreshToken);
    }
}
