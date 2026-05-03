using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using UrlShortener.API.Data;
using UrlShortener.API.Dtos;
using UrlShortener.API.Models;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LinksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LinksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateShortLink(CreateShortLinkDto request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (string.IsNullOrWhiteSpace(request.OriginalUrl))
                return BadRequest("Original URL is required.");

            if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out _))
                return BadRequest("Invalid URL format.");

            string shortCode;

            if (!string.IsNullOrWhiteSpace(request.CustomCode))
            {
                shortCode = request.CustomCode.Trim();

                var customCodeExists = await _context.ShortLinks
                    .AnyAsync(x => x.ShortCode == shortCode);

                if (customCodeExists)
                    return BadRequest("This custom code is already in use.");
            }
            else
            {
                shortCode = GenerateShortCode();

                while (await _context.ShortLinks.AnyAsync(x => x.ShortCode == shortCode))
                {
                    shortCode = GenerateShortCode();
                }
            }

            var shortLink = new ShortLink
            {
                OriginalUrl = request.OriginalUrl,
                ShortCode = shortCode,
                ExpireAt = request.ExpireAt,
                UserId = userId
            };

            _context.ShortLinks.Add(shortLink);
            await _context.SaveChangesAsync();

            var shortUrl = $"{Request.Scheme}://{Request.Host}/{shortCode}";

            return Ok(new
            {
                message = "Short link created successfully.",
                shortLink.Id,
                shortLink.OriginalUrl,
                shortLink.ShortCode,
                ShortUrl = shortUrl,
                shortLink.ClickCount,
                shortLink.CreatedAt,
                shortLink.ExpireAt
            });
        }

        [HttpGet]
        public async Task<IActionResult> GetMyLinks()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var links = await _context.ShortLinks
                .Where(x => x.UserId == userId)
                .OrderByDescending(x => x.CreatedAt)
                .Select(x => new
                {
                    x.Id,
                    x.OriginalUrl,
                    x.ShortCode,
                    ShortUrl = $"{Request.Scheme}://{Request.Host}/{x.ShortCode}",
                    x.ClickCount,
                    x.CreatedAt,
                    x.ExpireAt
                })
                .ToListAsync();

            return Ok(links);
        }

        [HttpGet("{id}/stats")]
        public async Task<IActionResult> GetStats(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var link = await _context.ShortLinks
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (link == null)
                return NotFound("Short link not found.");

            return Ok(new
            {
                link.Id,
                link.OriginalUrl,
                link.ShortCode,
                ShortUrl = $"{Request.Scheme}://{Request.Host}/{link.ShortCode}",
                link.ClickCount,
                link.CreatedAt,
                link.ExpireAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLink(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var link = await _context.ShortLinks
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (link == null)
                return NotFound("Short link not found.");

            _context.ShortLinks.Remove(link);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Short link deleted successfully."
            });
        }

        private static string GenerateShortCode()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();

            return new string(Enumerable.Repeat(chars, 6)
                .Select(x => x[random.Next(x.Length)])
                .ToArray());
        }
    }
}