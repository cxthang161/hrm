using System.IdentityModel.Tokens.Jwt;
using Dapper;
using hrm.Context;
using hrm.DTOs;
using hrm.Providers;

namespace hrm.Respository.Auth
{
    public class AuthRespository : IAuthRespository
    {
        private readonly HRMContext _context;
        private readonly TokenProvider _tokenProvider;
        private readonly RefreshTokenProvider _refreshTokenProvider;
        private readonly IConfiguration _configuration;


        public AuthRespository(HRMContext context, TokenProvider tokenProvider, RefreshTokenProvider refreshTokenProvider, IConfiguration configuration)
        {
            _context = context;
            _tokenProvider = tokenProvider;
            _refreshTokenProvider = refreshTokenProvider;
            _configuration = configuration;
        }
        public async Task<(Entities.Users, string, string)?> AuthLogin(UserLoginDto user)
        {
            using var connection = _context.CreateConnection();
            string sql = "SELECT * FROM Users WHERE UserName = @UserName";

            var foundUser = await connection.QueryFirstOrDefaultAsync<Entities.Users>(sql, new
            {
                UserName = user.UserName
            });
            string salt = _configuration["Cryptoraphy:Salt"]!;
            string passwordWithSalt = user.Password + salt;


            if (foundUser == null || !BCrypt.Net.BCrypt.Verify(passwordWithSalt, foundUser.Password))
            {
                return null;
            }

            var userSql = @"
                            SELECT 
                                u.Id, u.UserName, u.RoleId, u.CreatedAt,
                                r.Id, r.Name,
                                a.Id, a.AgentName, a.AgentCode, a.Address, a.Phone
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

            var permissionSql = @"
                                SELECT p.Id, p.Name, p.Description
                                FROM UserPermissions up
                                JOIN Permissions p ON up.PermissionId = p.Id
                                WHERE up.UserId = @UserId";

            var permissions = (await connection.QueryAsync<Entities.Permissions>(permissionSql, new { UserId = foundUser.Id })).ToList();
            var joined = string.Join(", ", permissions.Select(p => p.Name));
            fullUser!.Permissions = joined == null ? string.Empty : joined;

            var accessToken = _tokenProvider.CreateToken(fullUser!);
            var refreshToken = await _refreshTokenProvider.CreateRefreshToken(accessToken);
            const string upsertTokenSql = @"
                                            IF EXISTS (SELECT 1 FROM RefreshTokens WHERE UserId = @UserId)
                                            BEGIN
                                                UPDATE RefreshTokens 
                                                SET Token = @Token, ExpiresAt = @ExpiresAt, CreatedAt = @CreatedAt
                                                WHERE UserId = @UserId
                                            END
                                            ELSE
                                            BEGIN
                                                INSERT INTO RefreshTokens (UserId, Token, ExpiresAt, CreatedAt)
                                                VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt)
                                            END";

            await connection.ExecuteAsync(upsertTokenSql, new
            {
                UserId = foundUser.Id,
                Token = refreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });

            return (fullUser, accessToken, refreshToken);
        }

        public async Task<(string, string)?> RefreshToken(string expiredAccessToken, string refreshToken)
        {
            using var connection = _context.CreateConnection();

            // Giải mã access token đã hết hạn để lấy thông tin user
            var principal = _tokenProvider.GetPrincipalFromExpiredToken(expiredAccessToken);
            if (principal == null)
                return null;

            var userIdStr = principal.FindFirst(JwtRegisteredClaimNames.Sid)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return null;

            // Tìm người dùng
            var user = await connection.QueryFirstOrDefaultAsync<Entities.Users>(
                "SELECT * FROM Users WHERE Id = @UserId", new { UserId = userId });

            if (user == null)
                return null;

            // Kiểm tra refresh token trong DB
            var existingToken = await connection.QueryFirstOrDefaultAsync<Entities.RefreshTokens>(
                @"SELECT * FROM RefreshTokens WHERE UserId = @UserId AND Token = @Token",
                new { UserId = userId, Token = refreshToken });

            if (existingToken == null || existingToken.ExpiriesAt < DateTime.UtcNow)
                return null;

            // Tạo token mới
            var newAccessToken = _tokenProvider.CreateToken(user);
            var newRefreshToken = await _refreshTokenProvider.CreateRefreshToken(newAccessToken);

            // Cập nhật refresh token mới vào DB
            const string updateSql = @"
                                        UPDATE RefreshTokens 
                                        SET Token = @NewToken, ExpiresAt = @ExpiriesAt, CreatedAt = @CreatedAt 
                                        WHERE UserId = @UserId AND Token = @OldToken";

            await connection.ExecuteAsync(updateSql, new
            {
                UserId = userId,
                NewToken = newRefreshToken,
                OldToken = refreshToken,
                ExpiriesAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });

            return (newAccessToken, newRefreshToken);
        }

    }
}
