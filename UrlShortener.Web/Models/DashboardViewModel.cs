namespace UrlShortener.Web.Models
{
    public class DashboardViewModel
    {
        public int TotalLinks { get; set; }
        public int TotalClicks { get; set; }
        public int ActiveLinks { get; set; }
        public int ExpiredLinks { get; set; }

        public List<ShortLinkViewModel> Links { get; set; } = new();
    }
}