using LibraryManagementSystem.Data;
using LibraryManagementSystem.Models;
using LibraryManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static LibraryManagementSystem.DTOs.Dtos;
using Microsoft.EntityFrameworkCore;

namespace LibraryManagementSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly LibraryDbContext _db;
        private readonly ITokenService _tokenService;

        public AuthController(LibraryDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        // ── POST /api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _db.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid username or password." });

            var (token, expiresAt) = _tokenService.GenerateToken(user);

            return Ok(new AuthResponse(token, user.Username, user.Role, expiresAt));
        }

        // ── POST /api/auth/register
        [HttpPost("register")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (request.Role is not ("Admin" or "User"))
                return BadRequest(new { message = "Role must be 'Admin' or 'User'." });

            if (await _db.Users.AnyAsync(u => u.Username == request.Username))
                return Conflict(new { message = $"Username '{request.Username}' is already taken." });

            var user = new ApplicationUser
            {
                Username = request.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = request.Role
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return CreatedAtAction(null, null, new { message = "User created successfully.", userId = user.Id });
        }
    }
}
