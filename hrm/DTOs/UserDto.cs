using hrm.Entities;

namespace hrm.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public required string UserName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }

        public Agents Agent { get; set; } = null!;
    }
    public class UserLoginDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }

    public class CreateUserDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
        public int RoleId { get; set; }
        public int AgentId { get; set; }
    }


}
