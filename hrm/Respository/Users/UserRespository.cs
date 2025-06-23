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
                                u.RoleId,
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
                    userEntity.RoleId = role.Id;
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

        public async Task<(string, bool)> CreateUser(CreateUserDto userDto)
        {
            using var connection = _context.CreateConnection();

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            const string checkUserNameSql = "SELECT UserName FROM Users WHERE UserName = @UserName";
            if (!string.IsNullOrEmpty(userDto.UserName))
            {
                var existingUser = await connection.QueryFirstOrDefaultAsync<string>(checkUserNameSql, new { UserName = userDto.UserName });
                if (existingUser != null)
                {
                    return ("Username already exists", false);
                }
            }

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
                return ("User created successfully", true);
            }

            return ("User create error", false);
        }

        public async Task<(string, bool)> DeleteUser(int userId)
        {
            using var connection = _context.CreateConnection();
            const string sql = "DELETE FROM Users WHERE Id = @UserId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { UserId = userId });
            if (rowsAffected <= 0)
            {
                return ("User not found", false);
            }
            return ("Deleted user successfully", true);
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

        public async Task<(string, bool)> UpdateUser(int userId, CreateUserDto userDto)
        {
            using var connection = _context.CreateConnection();
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            const string checkIdSql = "SELECT * FROM Users WHERE Id = @Id";
            var existingUserById = await connection.QueryFirstOrDefaultAsync<Entities.Users>(checkIdSql, new { Id = userId });
            if (existingUserById == null)
            {
                return ("User not exists", false);
            }

            const string checkUserNameSql = "SELECT * FROM Users WHERE UserName = @UserName";
            var existingUserByName = await connection.QueryFirstOrDefaultAsync<Entities.Users>(checkUserNameSql, new { UserName = userDto.UserName });
            if (existingUserByName != null && existingUserByName.Id != userId)
            {
                return ("Username already exists", false);
            }

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

            if (rowsAffected <= 0)
            {
                return ("Failed to update user", false);
            }

            return ("Updated user successfully", true);
        }

    }
}
