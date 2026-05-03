using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.API.Data;
using UrlShortener.API.Dtos;
using UrlShortener.API.Helpers;
using UrlShortener.API.Models;

namespace UrlShortener.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly JwtHelper _jwtHelper;

        public AuthController(AppDbContext context, JwtHelper jwtHelper)
        {
            _context = context;
            _jwtHelper = jwtHelper;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto request)
        {
            if (await _context.Users.AnyAsync(x => x.Email == request.Email))
                return BadRequest("This email is already registered.");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtHelper.GenerateToken(user);

            return Ok(new
            {
                message = "Registration successful.",
                token,
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email
                }
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto request)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Email == request.Email);

            if (user == null)
                return BadRequest("Invalid email or password.");

            var isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
                return BadRequest("Invalid email or password.");

            var token = _jwtHelper.GenerateToken(user);

            return Ok(new
            {
                message = "Login successful.",
                token,
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email
                }
            });
        }
    }
}