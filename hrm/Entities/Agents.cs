namespace hrm.Entities
{
    public class Agents
    {
        public int Id { get; set; }
        public string AgentCode { get; set; }
        public string AgentName { get; set; }
        public string? Address { get; set; }
        public string? Phone { get; set; }
        //public List<Users> Users { get; set; }
    }
}
