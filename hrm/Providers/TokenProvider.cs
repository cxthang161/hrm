using System.Security.Claims;
using System.Security.Cryptography;
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

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    [
                        new Claim(JwtRegisteredClaimNames.Sid, user.Id.ToString()),
                        new Claim(JwtRegisteredClaimNames.Name, user.UserName),
                        new Claim(ClaimTypes.Role, user.RoleId.ToString())
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

        public string CreateRefreshToken()
        {
            var randomBytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }
}