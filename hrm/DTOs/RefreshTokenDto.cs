namespace hrm.DTOs
{
    public class RefreshTokenDto
    {
        public required string Token { get; set; }
        public DateTime ExpiriesAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public required int UserId { get; set; }
    }

    public class RefreshTokenResponseDto
    {
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
