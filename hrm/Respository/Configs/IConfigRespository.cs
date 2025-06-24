using hrm.DTOs;

namespace hrm.Respository.Configs
{
    public interface IConfigRespository
    {
        public Task<(string, bool)> CreateConfig(ConfigDto configDto, int userId);
        public Task<(string, bool)> UpdateConfig(ConfigUpdateDto configDto, int userId, int id);
        public Task<Entities.Configs?> GetConfigById(int id);
        public Task<(IEnumerable<Entities.Configs>, int)> GetAllConfigs(int pageIndex, int pageSize);
    }
}
