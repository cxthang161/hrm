using hrm.Entities;

namespace hrm.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string UserName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public Roles Role { get; set; } = null!;
        public Agents Agent { get; set; } = null!;
    }
    public class UserLoginDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }

    public class CreateUserDto
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public int RoleId { get; set; }
        public int AgentId { get; set; }
    }


}
