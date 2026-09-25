using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediCore.Api.Contracts;
using MediCore.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace MediCore.Api.Services
{
    public class JwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtTokenService(
            IConfiguration configuration,
            UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public async Task<AuthResponse> CreateAuthResponseAsync(
            ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var expiresInMinutes = _configuration.GetValue<int>(
                "Jwt:ExpiresInMinutes"
            );

            var expiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes);

            var jwtKey = _configuration["Jwt:Key"]!;
            var jwtIssuer = _configuration["Jwt:Issuer"]!;
            var jwtAudience = _configuration["Jwt:Audience"]!;

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Name, user.DisplayName)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            );

            var signingCredentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: signingCredentials
            );

            var accessToken = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return new AuthResponse
            {
                AccessToken = accessToken,
                ExpiresAt = expiresAt,
                User = new AuthenticatedUserResponse
                {
                    Id = user.Id,
                    DisplayName = user.DisplayName,
                    Email = user.Email ?? string.Empty,
                    Roles = roles.ToList()
                }
            };
        }
    }
}