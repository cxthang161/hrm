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
            string salt = _configuration["Cryptoraphy:Salt"];
            string passwordWithSalt = user.Password + salt;


            if (foundUser == null || !BCrypt.Net.BCrypt.Verify(passwordWithSalt, foundUser.Password))
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

            return (fullUser, accessToken, refreshToken);
        }

        public async Task<(string, string)?> RefreshToken(string accessToken, string refreshToken)
        {
            using var connection = _context.CreateConnection();
            var principal = _tokenProvider.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
            {
                return null;
            }
            var userIdStr = principal.FindFirst(JwtRegisteredClaimNames.Sid)?.Value;
            if (!int.TryParse(userIdStr, out var userIdFromToken))
            {
                return null;
            }

            int userId = int.Parse(userIdStr!);
            var sql = "SELECT * FROM Users WHERE Id = @UserId";
            var user = await connection.QueryFirstOrDefaultAsync<Entities.Users>(sql, new { UserId = userId });
            if (user == null)
            {
                return null;
            }

            var sqlFindToken = @"SELECT * FROM RefreshTokens WHERE UserId = @UserId AND Token = @Token";
            var existingToken = await connection.QueryFirstOrDefaultAsync<Entities.RefreshTokens>(
                sqlFindToken, new { UserId = userId, Token = refreshToken });
            if (existingToken == null || existingToken.ExpiriesAt < DateTime.UtcNow)
            {
                return null;
            }

            var newAccessToken = _tokenProvider.CreateToken(user);
            var newRefreshToken = await _refreshTokenProvider.CreateRefreshToken(newAccessToken);

            if (existingToken == null)
            {
                return null;
            }

            const string updateSql = @"
                    UPDATE RefreshTokens 
                    SET Token = @Token, ExpiresAt = @ExpiresAt, CreatedAt = @CreatedAt 
                    WHERE UserId = @UserId AND Token = @OldToken";
            await connection.ExecuteAsync(updateSql, new RefreshTokenDto
            {
                UserId = userId,
                Token = newRefreshToken,
                ExpiriesAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
            });

            return (accessToken, newRefreshToken);
        }
    }
}
