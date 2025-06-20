using Dapper;
using hrm.Context;
using hrm.DTOs;
using hrm.Providers;

namespace hrm.Respository.Users
{
    public class UserRepository : IUserRespository
    {
        private readonly HRMContext _context;
        private readonly TokenProvider _tokenProvider;


        public UserRepository(HRMContext context, TokenProvider tokenProvider)
        {
            _context = context;
            _tokenProvider = tokenProvider;
        }
        public async Task<string?> AuthLogin(UserLoginDto user)
        {
            using var connection = _context.CreateConnection();
            string sql = "SELECT * FROM Users WHERE UserName = @UserName AND Password = @Password";

            var foundUser = await connection.QueryFirstOrDefaultAsync<Entities.Users>(sql, new
            {
                UserName = user.UserName,
                Password = user.Password
            });

            if (foundUser == null)
            {
                return null;
            }

            return _tokenProvider.CreateToken(foundUser);
        }
    }
}
