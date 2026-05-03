namespace UrlShortener.API.Models
{
    public class ShortLink
    {
        public int Id { get; set; }

        public string OriginalUrl { get; set; } = string.Empty;

        public string ShortCode { get; set; } = string.Empty;

        public int ClickCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ExpireAt { get; set; }

        public int? UserId { get; set; }

        public User? User { get; set; }
    }
}