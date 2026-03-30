using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LibraryManagementSystem.Models;
using Microsoft.IdentityModel.Tokens;

namespace LibraryManagementSystem.Services
{
    public interface ITokenService
    {
        (string Token, DateTime ExpiresAt) GenerateToken(ApplicationUser user);
    }
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config) => _config = config;

        public (string Token, DateTime ExpiresAt) GenerateToken(ApplicationUser user)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secretKey = jwtSettings["SecretKey"]
                ?? throw new InvalidOperationException("JWT SecretKey is not configured.");

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(JwtRegisteredClaimNames.Sub,  user.Username),
            new Claim(JwtRegisteredClaimNames.Jti,  Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.Name,               user.Username),
            new Claim(ClaimTypes.Role,               user.Role),
            new Claim("userId",                      user.Id.ToString())
        };

            var expiresAt = DateTime.UtcNow.AddHours(
                double.Parse(jwtSettings["ExpiryHours"] ?? "8"));

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials
            );

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
