namespace hrm.DTOs
{
    public class ConfigDto
    {
        public required string ConfigValue { get; set; }
        public IFormFile? Logo { get; set; }
        public IFormFile? Background { get; set; }
        public int AgentId { get; set; }
        public required string NameTemplate { get; set; }
    }

    public class ConfigUpdateDto
    {
        public required string ConfigValue { get; set; }
        public IFormFile? Logo { get; set; }
        public IFormFile? Background { get; set; }
        public int AgentId { get; set; }
        public int UpdatedBy { get; set; }
    }
}
