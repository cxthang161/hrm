using hrm.DTOs;

namespace hrm.Respository.Users
{
    public interface IUserRespository
    {
        Task<(string, bool)> CreateUser(CreateUserDto user);
        Task<(string, bool)> DeleteUser(int userId);
        Task<(string, bool)> UpdateUser(int userId, CreateUserDto user);
        Task<(IEnumerable<Entities.Users>, int)> GetAll(int pageIndex, int pageSize);
        Task<Entities.Users?> GetUserById(int userId);
        Task<(string, bool)> ChangePassword(string newPassword, int userId);
    }
}
