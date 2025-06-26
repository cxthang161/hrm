namespace hrm.DTOs
{
    public class AgentDto
    {
        public required string AgentCode { get; set; }
        public required string AgentName { get; set; }
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
    }
}
