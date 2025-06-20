namespace hrm.Entities
{
    public class Configs
    {
        public int Id { get; set; }
        public string ProductKey { get; set; }
        public string ConfigValue { get; set; }
        public string LogoUrl { get; set; }
        public string BackgroundUrl { get; set; }
        public DateTime UpdatedAt { get; set; }

        public int AgentId { get; set; }
        public int UpdatedBy { get; set; }

        public Agents AgentInfo { get; set; }
        public Users UpdatedByUser { get; set; }
    }
}
