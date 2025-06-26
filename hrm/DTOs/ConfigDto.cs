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
}
