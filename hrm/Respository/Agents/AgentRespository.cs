using Dapper;
using hrm.Context;
using hrm.DTOs;

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
            string sql = "SELECT * FROM Agents;";
            var agents = await connection.QueryAsync<Entities.Agents>(sql);
            return agents;
        }
        public async Task<(string, bool)> CreateAgent(AgentDto agent)
        {
            using var connection = _context.CreateConnection();

            const string checkSql = "SELECT Id FROM Agents WHERE AgentCode = @AgentCode";
            var existingAgent = await connection.QueryFirstOrDefaultAsync<int?>(checkSql, new
            {
                AgentCode = agent.AgentCode,
            });

            if (existingAgent != null)
            {
                return ("Agent code already exists", false);
            }

            string sql = "INSERT INTO Agents (AgentName, AgentCode, Address, Phone) VALUES (@AgentName, @AgentCode, @Address, @Phone)";
            var resutl = await connection.ExecuteAsync(sql, new
            {
                AgentName = agent.AgentName,
                AgentCode = agent.AgentCode,
                Address = agent.Address,
                Phone = agent.Phone
            });
            if (resutl <= 0)
            {
                return ("Create agent error", false);
            }
            return ("Create agent successfully", true);
        }
        public async Task<(string, bool)> DeleteAgent(int agentId)
        {
            using var connection = _context.CreateConnection();
            string sql = "DELETE FROM Agents WHERE Id = @Id";
            var result = await connection.ExecuteAsync(sql, new { Id = agentId });
            if (result <= 0)
            {
                return ("Delete agent error", false);
            }
            return ("Delete agent successfully", true);
        }
        public async Task<(string, bool)> UpdateAgent(int agentId, UpdateAgentDto agentDto)
        {
            using var connection = _context.CreateConnection();
            string sql = "UPDATE Agents SET AgentName = @AgentName, Address = @Address, Phone = @Phone WHERE Id = @Id";
            var result = await connection.ExecuteAsync(sql, new
            {
                Id = agentId,
                AgentName = agentDto.AgentName,
                Address = agentDto.Address,
                Phone = agentDto.Phone
            });
            if (result <= 0)
            {
                return ("Update agent error", false);
            }
            return ("Update agent successfully", true);
        }
    }
}
