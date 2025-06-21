using hrm.DTOs;

namespace hrm.Respository.Users
{
    public interface IUserRespository
    {
        Task<(Entities.Users, string, string)?> AuthLogin(UserLoginDto user);
    }
}
