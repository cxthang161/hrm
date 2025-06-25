using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using hrm.Entities;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace hrm.Providers
{
    public class TokenProvider(IConfiguration configuration)
    {
        public string CreateToken(Users user)
        {
            string? secretKey = configuration["JWT:Secret"];

            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentNullException("JWT:Secret is missing in configuration!");
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var permissionKeys = user.Permissions ?? string.Empty;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    [
                        new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sid, user.Id.ToString()),
                        new Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Name, user.UserName),
                        new Claim(ClaimTypes.Role, user.Role.Name),
                        new Claim("permissions", permissionKeys)
                    ]
                ),
                Expires = DateTime.UtcNow.AddHours(configuration.GetValue<int>("JWT:Expiration")),
                SigningCredentials = creds,
                Issuer = configuration["JWT:Issuer"],
                Audience = configuration["JWT:Audience"]
            };

            var handle = new JsonWebTokenHandler();

            return handle.CreateToken(tokenDescriptor);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var secretKey = configuration["JWT:Secret"];
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentNullException("JWT:Secret is missing in configuration!");
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}