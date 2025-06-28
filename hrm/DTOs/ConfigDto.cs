using hrm.Entities;

namespace hrm.DTOs
{
    public class ConfigDto
    {
        public required IFormFile ConfigFile { get; set; }
        public required IFormFile Logo { get; set; }
        public required IFormFile Background { get; set; }
        public required int AgentId { get; set; }
        public required string NameTemplate { get; set; }
    }

    public class ConfigUpdateDto
    {
        public IFormFile? ConfigFile { get; set; }
        public IFormFile? Logo { get; set; }
        public IFormFile? Background { get; set; }
        public required int AgentId { get; set; }
    }
    public class ConfigInfoDto
    {
        public int Id { get; set; }
        public required string ProductKey { get; set; }
        public string ConfigUrl { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string BackgroundUrl { get; set; } = string.Empty;
        public string NameTemplate { get; set; } = string.Empty;
        public Agents? AgentInfo { get; set; }
        public string? UpdatedBy { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
