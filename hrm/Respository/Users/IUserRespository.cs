using hrm.DTOs;

namespace hrm.Respository.Users
{
    public interface IUserRespository
    {
        Task<string?> AuthLogin(UserLoginDto user);
    }
}
