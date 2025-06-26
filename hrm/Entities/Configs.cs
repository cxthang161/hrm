namespace hrm.Entities
{
    public class Configs
    {
        public int Id { get; set; }
        public required string ProductKey { get; set; }
        public required string ConfigUrl { get; set; } = string.Empty;
        public string LogoUrl { get; set; } = string.Empty;
        public string BackgroundUrl { get; set; } = string.Empty;
        public string NameTemplate { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }

        public required int AgentId { get; set; }
        public int UpdatedBy { get; set; }

        public Agents? AgentInfo { get; set; }
        public Users? UpdatedByUser { get; set; }
    }
}
