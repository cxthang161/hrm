using Dapper;
using hrm.Context;
using hrm.Providers;

namespace hrm.Respository.Configs
{
    public class ConfigRespository : IConfigRespository
    {
        private readonly HRMContext _context;


        public ConfigRespository(HRMContext context, TokenProvider tokenProvider, RefreshTokenProvider refreshTokenProvider)
        {
            _context = context;
        }

        public async Task<Entities.Configs?> UploadLogo(int id, string url)
        {
            using var connection = _context.CreateConnection();
            string sqlFound = "SELECT * Configs WHERE Id = @Id";
            var foundConfig = await connection.QueryFirstOrDefaultAsync<Entities.Configs>(sqlFound, new { Id = id });
            var urlStr = url;
            if (foundConfig != null)
            {
                urlStr += "," + foundConfig.LogoUrl;
            }

            string sql = "UPDATE Configs SET LogoUrl = @LogoUrl WHERE Id = @Id RETURNING *";
            var updatedConfig = await connection.QueryFirstOrDefaultAsync(sql, new
            {
                LogoUrl = urlStr,
                Id = id
            });
            return updatedConfig;
        }
    }
}
