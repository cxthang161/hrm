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
        private readonly RefreshTokenProvider _refreshTokenProvider;


        public UserRepository(HRMContext context, TokenProvider tokenProvider, RefreshTokenProvider refreshTokenProvider)
        {
            _context = context;
            _tokenProvider = tokenProvider;
            _refreshTokenProvider = refreshTokenProvider;
        }
        public async Task<(Entities.Users, string, string)?> AuthLogin(UserLoginDto user)
        {
            using var connection = _context.CreateConnection();
            string sql = "SELECT * FROM Users WHERE UserName = @UserName";

            var foundUser = await connection.QueryFirstOrDefaultAsync<Entities.Users>(sql, new
            {
                UserName = user.UserName
            });

            if (foundUser == null || !BCrypt.Net.BCrypt.Verify(user.Password, foundUser.Password))
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
            var refreshToken = await _refreshTokenProvider.CreateRefreshToken(accessToken);

            return (fullUser, accessToken, refreshToken);
        }

        public async Task<string> CreateUser(CreateUserDto userDto)
        {
            using var connection = _context.CreateConnection();

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            const string sql = @"
                                INSERT INTO Users (UserName, Password, RoleId, AgentId)
                                VALUES (@UserName, @Password, @RoleId, @AgentId);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var rowsAffected = await connection.ExecuteScalarAsync<int>(sql, new
            {
                UserName = userDto.UserName,
                Password = hashedPassword,
                RoleId = userDto.RoleId,
                AgentId = userDto.AgentId,
            });

            if (rowsAffected > 0)
            {
                return "User created successfully.";
            }
            else
            {
                return "User created error.";
            }
        }

        public async Task<string> DeleteUser(int userId)
        {
            using var connection = _context.CreateConnection();
            const string sql = "DELETE FROM Users WHERE Id = @UserId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId });
            if (rowsAffected > 0)
            {
                return "User deleted successfully.";
            }
            else
            {
                return "User not found.";
            }
        }

        public async Task<IEnumerable<Entities.Users>> GetAll(int pageIndex, int pageSize)
        {
            using var connection = _context.CreateConnection();
            const string sql = @"
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
                                ORDER BY u.CreatedAt DESC
                                OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";

            var result = await connection.QueryAsync<Entities.Users, Entities.Roles, Entities.Agents, Entities.Users>(
                sql,
                (userEntity, role, agent) =>
                {
                    userEntity.Role = role;
                    userEntity.Agent = agent;
                    return userEntity;
                },
                new
                {
                    Offset = (pageIndex - 1) * pageSize,
                    PageSize = pageSize
                },
                splitOn: "Id, Id"
            );

            return result;
        }

        public async Task<string> UpdateUser(int userId, CreateUserDto userDto)
        {
            using var connection = _context.CreateConnection();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            const string sql = @"
                                UPDATE Users 
                                SET UserName = @UserName, Password = @Password, RoleId = @RoleId, AgentId = @AgentId
                                WHERE Id = @UserId;";
            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                UserName = userDto.UserName,
                Password = hashedPassword,
                RoleId = userDto.RoleId,
                AgentId = userDto.AgentId,
                UserId = userId
            });
            return rowsAffected > 0 ? "User updated successfully." : "User not found.";
        }
    }
}
