using Dapper;
using hrm.Context;
using hrm.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;

namespace hrm.Providers
{
    public class RefreshTokenProvider
    {
        private readonly HRMContext _context;
        private readonly TokenProvider _tokenProvider;

        public RefreshTokenProvider(HRMContext context, TokenProvider tokenProvider)
        {
            _context = context;
            _tokenProvider = tokenProvider;
        }

        public async Task<string> CreateRefreshToken(string accessToken)
        {
            using var connection = _context.CreateConnection();

            var principal = _tokenProvider.GetPrincipalFromExpiredToken(accessToken);
            if (principal == null)
                return "";

            var userIdStr = principal.FindFirst(JwtRegisteredClaimNames.Sid)?.Value;
            if (!int.TryParse(userIdStr, out var userId))
                return "";

            // Tạo refresh token mới
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var newRefreshToken = Convert.ToBase64String(randomBytes);

            var sqlFindToken = @"SELECT * FROM RefreshTokens WHERE UserId = @UserId";
            var existingToken = await connection.QueryFirstOrDefaultAsync<RefreshTokens>(
                sqlFindToken, new { UserId = userId });

            if (existingToken == null)
            {
                const string insertSql = @"
                    INSERT INTO RefreshTokens (UserId, Token, ExpiresAt, CreatedAt)
                    VALUES (@UserId, @Token, @ExpiresAt, @CreatedAt);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                await connection.ExecuteAsync(insertSql, new
                {
                    UserId = userId,
                    Token = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedAt = DateTime.UtcNow
                });

                return newRefreshToken;
            }

            const string updateSql = @"
                UPDATE RefreshTokens 
                SET Token = @Token, ExpiresAt = @ExpiresAt, CreatedAt = @CreatedAt
                WHERE UserId = @UserId";

            await connection.ExecuteAsync(updateSql, new
            {
                UserId = existingToken.UserId,
                Token = newRefreshToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });

            return newRefreshToken;
        }
    }
}
