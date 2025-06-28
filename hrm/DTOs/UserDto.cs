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
        public required string Password { get; set; }
        public required int AgentId { get; set; } = 2;
        public required List<int> Permissions { get; set; } = new();
    }

    public class UpdateUserDto
    {
        public required string UserName { get; set; }
        public required int AgentId { get; set; } = 2;
        public List<int> Permissions { get; set; } = new();
    }

    public class ChangePasswordDto
    {
        public required string Password { get; set; }
    }

    public class InfoUserDto
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
    }
}
