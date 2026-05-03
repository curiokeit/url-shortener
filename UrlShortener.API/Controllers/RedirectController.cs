using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Data;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("")]
    public class RedirectController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RedirectController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("{code}")]
        public async Task<IActionResult> RedirectToOriginalUrl(string code)
        {
            var link = await _context.ShortLinks
                .FirstOrDefaultAsync(x => x.ShortCode == code);

            if (link == null)
                return NotFound("Short link not found.");

            if (link.ExpireAt.HasValue && link.ExpireAt.Value < DateTime.UtcNow)
                return BadRequest("This short link has expired.");

            link.ClickCount++;
            await _context.SaveChangesAsync();

            return Redirect(link.OriginalUrl);
        }
    }
}