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
        private readonly CloudinaryController _cloudinaryProvider;

        public ConfigRespository(HRMContext context, CloudinaryController cloudinaryController, RefreshTokenProvider refreshTokenProvider)
        {
            _context = context;
            _cloudinaryProvider = cloudinaryController;
        }

        public async Task<(string, bool)> CreateConfig(ConfigDto configDto, int userId)
        {
            using var connection = _context.CreateConnection();
            string sql = @"INSERT INTO Configs (ProductKey, ConfigUrl, LogoUrl, BackgroundUrl, NameTemplate, AgentId, UpdatedBy)
                           VALUES (@ProductKey, @ConfigUrl, @LogoUrl, @BackgroundUrl, @NameTemplate, @AgentId, @UpdateBy)";
            var logoUrl = await _cloudinaryProvider.UploadImageOrFile(configDto.Logo);
            var backgroundUrl = await _cloudinaryProvider.UploadImageOrFile(configDto.Background);
            var configUrl = await _cloudinaryProvider.UploadImageOrFile(configDto.ConfigFile);

            if (logoUrl == null || backgroundUrl == null)
            {
                return ("Invalid image", false);
            }
            if (configUrl == null)
            {
                return ("Invalid config file", false);
            }
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            var productKey = Convert.ToBase64String(randomBytes);

            var createConfig = await connection.ExecuteAsync(sql, new
            {
                productKey,
                ConfigUrl = configUrl,
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

            var parameters = new DynamicParameters();
            parameters.Add("UpdatedBy", userId);
            parameters.Add("Id", id);

            string existingSql = "SELECT * FROM Configs WHERE Id = @Id";
            var existingConfig = await connection.QueryFirstOrDefaultAsync<Entities.Configs>(existingSql, new { Id = id });

            string sql = "UPDATE Configs SET UpdatedBy = @UpdatedBy";

            // ====== LOGO ======
            if (configDto.Logo != null)
            {
                var logoUrl = await _cloudinaryProvider.UploadImageOrFile(configDto.Logo);
                if (logoUrl == null) return ("Invalid logo image", false);

                var oldLogoPublicId = existingConfig?.LogoUrl?.Split(',').Length == 2
                                    ? existingConfig.LogoUrl.Split(',')[1]
                                    : null;

                if (!string.IsNullOrEmpty(oldLogoPublicId))
                {
                    await _cloudinaryProvider.DeleteFileFromCloudinary(oldLogoPublicId);
                }

                sql += ", LogoUrl = @LogoUrl";
                parameters.Add("LogoUrl", logoUrl);
            }

            // ====== BACKGROUND ======
            if (configDto.Background != null)
            {
                var backgroundUrl = await _cloudinaryProvider.UploadImageOrFile(configDto.Background);
                if (backgroundUrl == null) return ("Invalid background image", false);

                var oldBgPublicId = existingConfig?.BackgroundUrl?.Split(',').Length == 2
                                  ? existingConfig.BackgroundUrl.Split(',')[1]
                                  : null;

                if (!string.IsNullOrEmpty(oldBgPublicId))
                {
                    await _cloudinaryProvider.DeleteFileFromCloudinary(oldBgPublicId);
                }

                sql += ", BackgroundUrl = @BackgroundUrl";
                parameters.Add("BackgroundUrl", backgroundUrl);
            }

            // ====== CONFIG FILE ======
            if (configDto.ConfigFile != null)
            {
                var configValueUrl = await _cloudinaryProvider.UploadImageOrFile(configDto.ConfigFile);
                if (configValueUrl == null) return ("Invalid config file", false);

                var oldConfigPublicId = existingConfig?.ConfigUrl?.Split(',').Length == 2
                                      ? existingConfig.ConfigUrl.Split(',')[1]
                                      : null;

                if (!string.IsNullOrEmpty(oldConfigPublicId))
                {
                    await _cloudinaryProvider.DeleteFileFromCloudinary(oldConfigPublicId);
                }

                sql += ", ConfigUrl = @ConfigUrl";
                parameters.Add("ConfigUrl", configValueUrl);
            }

            sql += " WHERE Id = @Id";

            var rows = await connection.ExecuteAsync(sql, parameters);
            if (rows <= 0)
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
                            c.Id, c.ProductKey, c.ConfigValue, c.LogoUrl, c.BackgroundUrl, c.NameTemplate, c.UpdatedAt, c.AgentId, c.UpdatedBy,
                            a.Id, a.AgentName, a.AgentCode, a.Address, a.Phone,
                            u.Id, u.UserName, u.Password, u.CreatedAt, u.RoleId, u.AgentId
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