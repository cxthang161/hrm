using hrm.DTOs;

namespace hrm.Respository.Users
{
    public interface IUserRespository
    {
        Task<(Entities.Users, string, string)?> AuthLogin(UserLoginDto user);

        Task<string> CreateUser(CreateUserDto user);
        Task<string> DeleteUser(int userId);
        Task<string> UpdateUser(int userId, CreateUserDto user);
        Task<IEnumerable<Entities.Users>> GetAll(int pageIndex, int pageSize);
    }
}
