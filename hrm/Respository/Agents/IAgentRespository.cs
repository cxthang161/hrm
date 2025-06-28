using hrm.DTOs;

namespace hrm.Respository.Agents
{
    public interface IAgentRespository
    {
        public Task<IEnumerable<Entities.Agents>> GetAllAgents();
        public Task<(string, bool)> CreateAgent(AgentDto agent);
        public Task<(string, bool)> DeleteAgent(int agentId);
        public Task<(string, bool)> UpdateAgent(int agentId, UpdateAgentDto agent);
    }
}
