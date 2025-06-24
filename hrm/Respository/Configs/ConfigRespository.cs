using System.Security.Cryptography;
using Dapper;
using hrm.Context;
using hrm.DTOs;
using hrm.Providers;

namespace hrm.Respository.Configs
{
    public class ConfigRespository : IConfigRespository
    {
        private readonly HRMContext _context;
        private readonly UploadFileProvider _uploadFileProvider;

        public ConfigRespository(HRMContext context, UploadFileProvider uploadFileProvider, RefreshTokenProvider refreshTokenProvider)
        {
            _context = context;
            _uploadFileProvider = uploadFileProvider;
        }

        public async Task<(string, bool)> CreateConfig(ConfigDto configDto, int userId)
        {
            using var connection = _context.CreateConnection();
            string sql = @"INSERT INTO Configs (ProductKey, ConfigValue, LogoUrl, BackgroundUrl, NameTemplate, AgentId, UpdatedBy)
                           VALUES (@ProductKey, @ConfigValue, @LogoUrl, @BackgroundUrl, @NameTemplate, @AgentId, @UpdateBy)";
            var logoUrl = await _uploadFileProvider.UploadImage(configDto.Logo);
            var backgroundUrl = await _uploadFileProvider.UploadImage(configDto.Background);

            if (logoUrl == null || backgroundUrl == null)
            {
                return ("Invalid image", false);
            }
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var productKey = Convert.ToBase64String(randomBytes);

            var createConfig = await connection.ExecuteAsync(sql, new
            {
                productKey,
                ConfigValue = configDto.ConfigValue,
                LogoUrl = logoUrl,
                BackgroundUrl = backgroundUrl,
                AgentId = configDto.AgentId,
                UpdateBy = userId,
                NameTemplate = configDto.NameTemplate
            });
            if (createConfig <= 0)
            {
                return ("Created config error", false);
            }
            return ("Created config successfully", true);
        }

        public async Task<(string, bool)> UpdateConfig(ConfigUpdateDto configDto, int userId, int id)
        {
            using var connection = _context.CreateConnection();
            string sql = @"UPDATE Configs 
                           SET ConfigValue = @ConfigValue, LogoUrl = @LogoUrl, BackgroundUrl = @BackgroundUrl, UpdatedBy = @UpdatedBy
                           WHERE Id = @Id";
            var logoUrl = await _uploadFileProvider.UploadImage(configDto.Logo);
            var backgroundUrl = await _uploadFileProvider.UploadImage(configDto.Background);
            if (logoUrl == null || backgroundUrl == null)
            {
                return ("Invalid image", false);
            }
            var updateConfig = await connection.ExecuteAsync(sql, new
            {
                ConfigValue = configDto.ConfigValue,
                LogoUrl = logoUrl,
                BackgroundUrl = backgroundUrl,
                UpdatedBy = userId,
                Id = id
            });
            if (updateConfig <= 0)
            {
                return ("Update config error", false);
            }
            return ("Update config successfully", true);
        }

        public async Task<Entities.Configs?> GetConfigById(int id)
        {
            using var connection = _context.CreateConnection();

            string sql = @"SELECT * 
                   FROM Configs c
                   JOIN Agents a ON c.AgentId = a.Id
                   JOIN Users u ON c.UpdatedBy = u.Id
                   WHERE c.Id = @Id";

            var result = await connection.QueryAsync<Entities.Configs, Entities.Agents, Entities.Users, Entities.Configs>(
                sql,
                (config, agent, user) =>
                {
                    config.AgentInfo = agent;
                    config.UpdatedByUser = user;
                    return config;
                },
                new { Id = id },
                splitOn: "AgentId,UpdatedBy"
            );

            return result.FirstOrDefault();
        }

        public async Task<(IEnumerable<Entities.Configs>, int)> GetAllConfigs(int pageIndex, int pageSize)
        {
            using var connection = _context.CreateConnection();
            string sql = @"SELECT 
                            c.Id,
                            c.ProductKey,
                            c.ConfigValue,
                            c.LogoUrl,
                            c.BackgroundUrl,
                            c.NameTemplate,
                            c.UpdatedAt,
                            c.AgentId,
                            c.UpdatedBy,

                            a.Id,
                            a.AgentName,
                            a.AgentCode,
                            a.Address,
                            a.Phone,

                            u.Id,
                            u.UserName,
                            u.Password,
                            u.CreatedAt   AS UserCreatedAt,
                            u.RoleId,
                            u.AgentId     AS UserAgentId
                        FROM Configs c
                        JOIN Agents a ON c.AgentId = a.Id
                        JOIN Users u ON c.UpdatedBy = u.Id
                        ORDER BY c.UpdatedAt DESC
                        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";
            var result = await connection.QueryAsync<Entities.Configs, Entities.Agents, Entities.Users, Entities.Configs>(
                sql,
                (config, agent, user) =>
                {
                    config.AgentInfo = agent;
                    config.UpdatedByUser = user;
                    return config;
                },
                new
                {
                    Offset = (pageIndex - 1) * pageSize,
                    PageSize = pageSize
                },
                splitOn: "Id,Id"
            );

            const string countSql = "SELECT COUNT(*) FROM Configs";
            var totalRows = await connection.ExecuteScalarAsync<int>(countSql);
            return (result, totalRows);
        }
    }
}