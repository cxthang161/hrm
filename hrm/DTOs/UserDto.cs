namespace hrm.DTOs
{
    public interface UserDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public int AgentId { get; set; }

    }
}
