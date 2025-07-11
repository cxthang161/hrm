﻿namespace hrm.Entities
{
    public class RefreshTokens
    {
        public int Id { get; set; }
        public required string Token { get; set; }
        public DateTime ExpiriesAt { get; set; }
        public DateTime CreatedAt { get; set; }

        public required int UserId { get; set; }
        public Users? User { get; set; }
    }
}
