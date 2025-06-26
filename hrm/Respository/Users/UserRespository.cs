﻿using Dapper;
using hrm.Context;
using hrm.DTOs;

namespace hrm.Respository.Users
{
    public class UserRepository : IUserRespository
    {
        private readonly HRMContext _context;
        private readonly IConfiguration _configuration;

        public UserRepository(HRMContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<(string, bool)> CreateUser(CreateUserDto userDto)
        {
            using var connection = _context.CreateConnection();
            string salt = _configuration["Cryptoraphy:Salt"];
            string password = userDto.Password + salt;

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
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
                                VALUES (@UserName, @Password, 2, @AgentId);
                                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            var userId = await connection.ExecuteScalarAsync<int>(sql, new
            {
                UserName = userDto.UserName,
                Password = hashedPassword,
                AgentId = userDto.AgentId,
            });


            if (userId > 0)
            {
                if (userDto.Permissions?.Any() == true)
                {
                    const string insertPermissionSql = @"
                                                    INSERT INTO UserPermissions (UserId, PermissionId)
                                                    VALUES (@UserId, @PermissionId);";

                    foreach (var permissionId in userDto.Permissions)
                    {
                        await connection.ExecuteAsync(insertPermissionSql, new
                        {
                            UserId = userId,
                            PermissionId = permissionId
                        });
                    }
                }
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

            const string deletePermissionsSql = "DELETE FROM UserPermissions WHERE UserId = @UserId";
            await connection.ExecuteAsync(deletePermissionsSql, new { UserId = userId });

            return ("Deleted user successfully", true);
        }

        public async Task<(IEnumerable<Entities.Users>, int)> GetAll(int pageIndex, int pageSize)
        {
            using var connection = _context.CreateConnection();

            const string userSql = @"
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

            var users = (await connection.QueryAsync<Entities.Users, Entities.Roles, Entities.Agents, Entities.Users>(
                userSql,
                (user, role, agent) =>
                {
                    user.Role = role;
                    user.Agent = agent;
                    return user;
                },
                new
                {
                    Offset = (pageIndex - 1) * pageSize,
                    PageSize = pageSize
                },
                splitOn: "Id,Id"
            )).ToList();

            const string countSql = "SELECT COUNT(*) FROM Users";
            var totalRows = await connection.ExecuteScalarAsync<int>(countSql);

            const string permissionSql = @"
                                            SELECT p.Id, p.Name, p.Description
                                            FROM UserPermissions up
                                            JOIN Permissions p ON up.PermissionId = p.Id
                                            WHERE up.UserId = @UserId";

            foreach (var user in users)
            {
                var permissions = (await connection.QueryAsync<Entities.Permissions>(
                    permissionSql,
                    new { UserId = user.Id }
                )).ToList();

                var joined = string.Join(", ", permissions.Select(p => p.Name));

                user.Permissions = joined == null ? string.Empty : joined;
            }

            return (users, totalRows);
        }


        public async Task<(string, bool)> UpdateUser(int userId, UpdateUserDto userDto)
        {
            using var connection = _context.CreateConnection();

            // Kiểm tra user tồn tại
            const string checkIdSql = "SELECT * FROM Users WHERE Id = @Id";
            var existingUser = await connection.QueryFirstOrDefaultAsync<Entities.Users>(checkIdSql, new { Id = userId });
            if (existingUser == null)
            {
                return ("User not exists", false);
            }

            // Kiểm tra trùng username
            const string checkUserNameSql = "SELECT * FROM Users WHERE UserName = @UserName";
            var existingUserByName = await connection.QueryFirstOrDefaultAsync<Entities.Users>(checkUserNameSql, new { UserName = userDto.UserName });
            if (existingUserByName != null && existingUserByName.Id != userId)
            {
                return ("Username already exists", false);
            }

            // Xử lý permissions
            var permissionsCsv = userDto.Permissions != null
                ? string.Join(",", userDto.Permissions)
                : null;

            // Thực hiện cập nhật
            const string sql = @"
                                UPDATE Users 
                                SET UserName = @UserName, 
                                    AgentId = @AgentId, 
                                    Permissions = @Permissions
                                WHERE Id = @UserId;";

            var rowsAffected = await connection.ExecuteAsync(sql, new
            {
                UserName = userDto.UserName,
                AgentId = userDto.AgentId,
                UserId = userId,
                Permissions = permissionsCsv
            });

            if (rowsAffected <= 0)
            {
                return ("Failed to update user", false);
            }

            return ("Updated user successfully", true);
        }

        public async Task<Entities.Users?> GetUserById(int userId)
        {
            using var connection = _context.CreateConnection();
            const string userSql = @"
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
                (user, role, agent) =>
                {
                    user.Role = role;
                    user.Agent = agent;
                    return user;
                },
                new { UserId = userId },
                splitOn: "Id, Id"
            );

            var permissionSql = @"
                                SELECT p.Id, p.Name, p.KeyName
                                FROM UserPermissions up
                                JOIN Permissions p ON up.PermissionId = p.Id
                                WHERE up.UserId = @UserId";

            var permissions = (await connection.QueryAsync<Entities.Permissions>(permissionSql, new { UserId = userId })).ToList();
            var joined = string.Join(", ", permissions.Select(p => p.Name));

            result.FirstOrDefault()!.Permissions = joined == null ? string.Empty : joined;
            return result.FirstOrDefault();

        }
        public async Task<(string, bool)> ChangePassword(string newPassword, int userId)
        {
            using var connection = _context.CreateConnection();
            string salt = _configuration["Cryptoraphy:Salt"];
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword + salt);
            const string sql = "UPDATE Users SET Password = @Password WHERE Id = @UserId";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Password = hashedPassword, UserId = userId });
            if (rowsAffected <= 0)
            {
                return ("Failed to change password", false);
            }
            return ("Changed password successfully", true);
        }
    }
}
