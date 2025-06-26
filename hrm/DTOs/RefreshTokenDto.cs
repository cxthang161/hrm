namespace hrm.DTOs
{
    public class RefreshTokenDto
    {
        public required string Token { get; set; }
        public required DateTime ExpiriesAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public required int UserId { get; set; }
    }

    public class RefreshTokenResponseDto
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
