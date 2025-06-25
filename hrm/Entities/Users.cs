namespace hrm.Entities
{
    public class Users
    {
        public int Id { get; set; }
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public string Permissions { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public required int RoleId { get; set; }
        public Roles? Role { get; set; }

        public required int AgentId { get; set; }
        public Agents? Agent { get; set; }
    }
}
