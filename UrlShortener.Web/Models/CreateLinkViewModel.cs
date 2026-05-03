namespace UrlShortener.Web.Models
{
    public class CreateLinkViewModel
    {
        public string OriginalUrl { get; set; } = string.Empty;
        public string? CustomCode { get; set; }
        public DateTime? ExpireAt { get; set; }
    }
}