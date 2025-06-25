using hrm.Entities;

namespace hrm.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public required string UserName { get; set; }
        public string? Permissions { get; set; }
        public DateTime CreatedAt { get; set; }

        public Agents? Agent { get; set; }
    }
    public class UserLoginDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }

    public class CreateUserDto
    {
        public required string UserName { get; set; }
        public string? Password { get; set; }
        public int RoleId { get; set; }
        public int? AgentId { get; set; } = 2;
        public string Permissions { get; set; } = string.Empty;
    }

    public class ChangePasswordDto
    {
        public required string Password { get; set; }
    }
}
