using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using UrlShortener.Web.Models;

namespace UrlShortener.Web.Controllers
{
    public class LinksController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public LinksController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var response = await client.GetAsync($"{baseUrl}/api/Links");

            if (!response.IsSuccessStatusCode)
            {
                return View(new DashboardViewModel());
            }

            var json = await response.Content.ReadAsStringAsync();

            var links = JsonSerializer.Deserialize<List<ShortLinkViewModel>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new List<ShortLinkViewModel>();

            var dashboard = new DashboardViewModel
            {
                Links = links,
                TotalLinks = links.Count,
                TotalClicks = links.Sum(x => x.ClickCount),
                ActiveLinks = links.Count(x => !x.ExpireAt.HasValue || x.ExpireAt.Value > DateTime.UtcNow),
                ExpiredLinks = links.Count(x => x.ExpireAt.HasValue && x.ExpireAt.Value <= DateTime.UtcNow)
            };

            return View(dashboard);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateLinkViewModel model)
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var baseUrl = _configuration["ApiSettings:BaseUrl"];

            var json = JsonSerializer.Serialize(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{baseUrl}/api/Links", content);

            if (!response.IsSuccessStatusCode)
            {
                ViewBag.Error = "Kısa link oluşturulamadı.";
                return View(model);
            }

            return RedirectToAction("Index");
        }

        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            await client.DeleteAsync($"{baseUrl}/api/Links/{id}");

            return RedirectToAction("Index");
        }
        public async Task<IActionResult> Stats(int id)
        {
            var token = HttpContext.Session.GetString("JWToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Auth");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var baseUrl = _configuration["ApiSettings:BaseUrl"];
            var response = await client.GetAsync($"{baseUrl}/api/Links/{id}/stats");

            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var json = await response.Content.ReadAsStringAsync();

            var link = JsonSerializer.Deserialize<ShortLinkViewModel>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return View(link);
        }
    }
}