using hrm.Entities;

namespace hrm.Respository.Users
{
    public interface IUserRespository
    {
        Task<string?> AuthLogin(LoginRequest user);
    }
}
