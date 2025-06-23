using hrm.DTOs;

namespace hrm.Respository.Users
{
    public interface IUserRespository
    {
        Task<(Entities.Users, string, string)?> AuthLogin(UserLoginDto user);

        Task<(string, bool)> CreateUser(CreateUserDto user);
        Task<(string, bool)> DeleteUser(int userId);
        Task<(string, bool)> UpdateUser(int userId, CreateUserDto user);
        Task<IEnumerable<Entities.Users>> GetAll(int pageIndex, int pageSize);
    }
}
