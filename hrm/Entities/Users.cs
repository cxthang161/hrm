namespace hrm.Entities
{
    public class Users
    {
        public int Id { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public DateTime CreatedAt { get; set; }

        public int RoleId { get; set; }
        public Roles Role { get; set; }

        public int AgentId { get; set; }
        public Agents Agent { get; set; }
    }
}
