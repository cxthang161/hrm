namespace hrm.Entities
{
    public class Agents
    {
        public int Id { get; set; }
        public required string AgentCode { get; set; }
        public required string AgentName { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        //public List<Users> Users { get; set; }
    }
}
