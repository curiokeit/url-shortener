namespace UrlShortener.API.Dtos
{
    public class CreateShortLinkDto
    {
        public string OriginalUrl { get; set; } = string.Empty;

        public string? CustomCode { get; set; }

        public DateTime? ExpireAt { get; set; }
    }
}