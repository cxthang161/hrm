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
        public async Task<(Entities.Users, string, string)?> AuthLogin(UserLoginDto user)
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

            var userSql = @"
                            SELECT 
                                u.Id,
                                u.UserName,
                                u.CreatedAt,

                                r.Id,
                                r.Name,

                                a.Id,
                                a.AgentName,
                                a.AgentCode,
                                a.Address,
                                a.Phone
                            FROM Users u 
                            JOIN Roles r ON u.RoleId = r.Id 
                            JOIN Agents a ON u.AgentId = a.Id
                            WHERE u.Id = @UserId";

            var result = await connection.QueryAsync<Entities.Users, Entities.Roles, Entities.Agents, Entities.Users>(
                userSql,
                (userEntity, role, agent) =>
                {
                    userEntity.Role = role;
                    userEntity.Agent = agent;
                    return userEntity;
                },
                new { UserId = foundUser.Id },
                splitOn: "Id, Id"
            );

            var fullUser = result.FirstOrDefault();

            var accessToken = _tokenProvider.CreateToken(fullUser!);
            var refreshToken = _tokenProvider.CreateRefreshToken();

            return (fullUser, accessToken, refreshToken);
        }

    }
}
