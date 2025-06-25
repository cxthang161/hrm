namespace hrm.Respository.Agents
{
    public interface IAgentRespository
    {
        public Task<IEnumerable<Entities.Agents>> GetAllAgents();
    }
}
