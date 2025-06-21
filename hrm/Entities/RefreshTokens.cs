namespace hrm.Entities
{
    public class RefreshTokens
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime ExpiriesAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public int UserId { get; set; }
        public Users user { get; set; }
    }
}
