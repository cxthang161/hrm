using Dapper;
using hrm.Context;

namespace hrm.Respository.Agents
{
    public class AgentRespository : IAgentRespository
    {
        private readonly HRMContext _context;

        public AgentRespository(HRMContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<Entities.Agents>> GetAllAgents()
        {
            using var connection = _context.CreateConnection();
            string sql = "SELECT * FROM Agents";
            var agents = await connection.QueryAsync<Entities.Agents>(sql);
            return agents;
        }
    }
}
